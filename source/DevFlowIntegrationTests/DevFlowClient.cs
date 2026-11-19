using System;
using Xunit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AvalonDock.DevFlowIntegrationTests
{
	// Thin, reusable HTTP client for the DevFlow agent embedded in the running
	// UnoDock sample (docs/refactoring.md, "DevFlow as test infra"). Integration
	// tests use this to drive deterministic [DevFlowAction] verbs and assert on the
	// structured result, instead of bespoke PowerShell or pixel screenshots.
	//
	// Reachability is opt-in: tests resolve the port from DEVFLOW_TEST_PORT and skip
	// when no sample is running, so the default (headless) test run stays green.
	public sealed class DevFlowClient : IDisposable
	{
		// The agent rejects every mutation with 409 unless the request carries a mutation lease
		// (AgentOptions.RequireMutationLease defaults to true). The lease is claimed from
		// /api/v1/agent/lease and echoed in X-DevFlow-Lease; an agent that predates leases answers
		// that endpoint with 404, which is treated as "every mutation is allowed".
		private const string LeaseEndpoint = "/api/v1/agent/lease";
		private const string LeaseHeader = "X-DevFlow-Lease";
		private const string HolderHeader = "X-DevFlow-Holder";
		private const string HolderKind = "test";

		// Lease ids are kept per agent port and shared by every client in this process; see
		// AcquireLeaseAsync. The gate serializes the initial claim so two clients cannot race and
		// steal the lease from each other.
		private static readonly object LeaseSync = new();
		private static readonly SemaphoreSlim LeaseGate = new(1, 1);
		private static readonly Dictionary<int, string> LeaseIds = new();
		private static readonly HashSet<int> UnleasedPorts = new();

		private readonly HttpClient _http;
		private readonly int _port;

		public DevFlowClient(int port)
		{
			_port = port;
			_http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
			_http.Timeout = TimeSpan.FromSeconds(20);
		}

		/// <summary>Port from DEVFLOW_TEST_PORT, or null when integration tests should be skipped.</summary>
		public static int? ResolvePortOrNull()
			=> int.TryParse(Environment.GetEnvironmentVariable("DEVFLOW_TEST_PORT"), out var p) && p > 0
				? p
				: (int?)null;

		public async Task<bool> IsReachableAsync(CancellationToken ct = default)
		{
			try
			{
				using var resp = await _http.GetAsync("/api/v1/agent/status", ct).ConfigureAwait(false);
				return resp.IsSuccessStatusCode;
			}
			catch { return false; }
		}

		public Task<string> GetTreeAsync(CancellationToken ct = default)
			=> GetStringAsync("/api/v1/ui/tree", ct);

		public async Task<JsonElement> GetStatusAsync(CancellationToken ct = default)
		{
			var raw = await GetStringAsync("/api/v1/agent/status", ct).ConfigureAwait(false);
			return JsonDocument.Parse(raw).RootElement.Clone();
		}

		public async Task<List<string>> ListActionsAsync(CancellationToken ct = default)
		{
			var raw = await GetStringAsync("/api/v1/invoke/actions", ct).ConfigureAwait(false);
			using var doc = JsonDocument.Parse(raw);
			var result = new List<string>();
			if (doc.RootElement.TryGetProperty("actions", out var actions) && actions.ValueKind == JsonValueKind.Array)
			{
				foreach (var action in actions.EnumerateArray())
				{
					if (action.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
						result.Add(name.GetString());
				}
			}

			return result;
		}

		/// <summary>
		/// Invoke a custom [DevFlowAction] and return its string result (the wrapper's
		/// "result"/"value"/"output" field if present, otherwise the raw body).
		/// </summary>
		public async Task<string> InvokeAsync(string action, params object[] args)
		{
			var body = JsonSerializer.Serialize(new { args = args ?? Array.Empty<object>() });
			using var content = new StringContent(body, Encoding.UTF8, "application/json");
			var watch = Stopwatch.StartNew();
			try
			{
				using var resp = await SendMutationAsync(HttpMethod.Post, $"/api/v1/invoke/actions/{action}", content, default).ConfigureAwait(false);
				var raw = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
				// A restart per test makes slow round trips the dominant cost on slow runners; say which.
				if (watch.ElapsedMilliseconds > 1000)
					Console.Error.WriteLine($"[devflow-slow] {action} took {watch.ElapsedMilliseconds} ms");
				if (!resp.IsSuccessStatusCode)
				{
					// A lease rejection and an unimplemented primitive are control flow the suites handle
					// (retry and skip); a capture for each of them would turn a normal run into a slow one.
					if (resp.StatusCode is not (HttpStatusCode.Conflict or HttpStatusCode.NotImplemented))
					{
						await CaptureFailureAsync($"action '{action}' returned {(int)resp.StatusCode}").ConfigureAwait(false);
					}

					throw new HttpRequestException(
						$"DevFlow action '{action}' returned {(int)resp.StatusCode}: {raw}");
				}
				return ExtractResult(raw);
			}
			catch (TaskCanceledException ex)
			{
				await CaptureFailureAsync($"action '{action}' timed out").ConfigureAwait(false);
				throw new TimeoutException($"DevFlow action '{action}' did not complete within {_http.Timeout}.", ex);
			}
		}

		/// <summary>
		/// Keeps the screen and the app state when a call to the agent fails. The capture issues calls of
		/// its own, so it is guarded by the capture helper rather than by anything here.
		/// </summary>
		private Task CaptureFailureAsync(string reason)
		{
			Console.Error.WriteLine($"[devflow] {reason}");
			return FailureArtifacts.CaptureAsync(this, FailureArtifacts.CurrentCaseName);
		}

		/// <summary>
		/// Sends a mutating request, claiming the agent's mutation lease first when this process does
		/// not hold it yet.
		/// </summary>
		private async Task<HttpResponseMessage> SendMutationAsync(HttpMethod method, string path, HttpContent content, CancellationToken ct)
		{
			// Buffered so the request can be replayed if the lease turns out to be stale. A restarted
			// app (the suite restarts TestApp between native-drag rows) and a lease that expired are
			// both only discovered when a mutation is rejected, so the retry has to resend the body.
			var body = await content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
			var contentType = content.Headers.ContentType;
			content.Dispose();

			for (var attempt = 0; ; attempt++)
			{
				var leaseId = await AcquireLeaseAsync(ct).ConfigureAwait(false);
				using var request = new HttpRequestMessage(method, path) { Content = new ByteArrayContent(body) };
				if (contentType != null) request.Content.Headers.ContentType = contentType;
				if (leaseId != null)
				{
					request.Headers.TryAddWithoutValidation(LeaseHeader, leaseId);
					request.Headers.TryAddWithoutValidation(HolderHeader, HolderKind);
				}

				var response = await _http.SendAsync(request, ct).ConfigureAwait(false);

				// One retry is enough: the second attempt claims a fresh lease with force, which
				// succeeds unless the app is genuinely driven by someone else.
				if (attempt == 0 && response.StatusCode == HttpStatusCode.Conflict)
				{
					var rejected = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
					response.Dispose();
					if (!rejected.Contains("\"reason\":\"lease\"", StringComparison.Ordinal))
						throw new HttpRequestException(
							$"DevFlow {method} {path} returned {(int)HttpStatusCode.Conflict}: {rejected}");

					ForgetLease(_port);
					continue;
				}

				return response;
			}
		}

		/// <summary>
		/// Drops the cached lease for an agent, so the next mutation claims one again.
		/// </summary>
		/// <remarks>
		/// Called when the app under test is restarted: a new process has no lease at all, and the id
		/// this process holds refers to the previous instance.
		/// </remarks>
		internal static void ForgetLease(int port)
		{
			lock (LeaseSync) LeaseIds.Remove(port);
		}

		/// <summary>
		/// Returns the mutation lease this process holds on the app, claiming it on first use.
		/// </summary>
		/// <remarks>
		/// The lease is per app, not per client: the suite builds one client per test but they all
		/// drive the same TestApp, and the agent only ever has one holder. A per-client lease would
		/// make every test after the first take over from the one before it.
		/// </remarks>
		private async Task<string> AcquireLeaseAsync(CancellationToken ct)
		{
			lock (LeaseSync)
			{
				if (UnleasedPorts.Contains(_port))
					return null;
				if (LeaseIds.TryGetValue(_port, out var held))
					return held;
			}

			await LeaseGate.WaitAsync(ct).ConfigureAwait(false);
			try
			{
				lock (LeaseSync)
				{
					if (UnleasedPorts.Contains(_port))
						return null;
					if (LeaseIds.TryGetValue(_port, out var held))
						return held;
				}

				return await ClaimLeaseAsync(forceTakeover: false, ct).ConfigureAwait(false);
			}
			finally
			{
				LeaseGate.Release();
			}
		}

		/// <summary>
		/// Sends one lease claim. Must only be called while holding <see cref="LeaseGate"/>.
		/// </summary>
		/// <param name="forceTakeover">
		/// Takes the lease even if another session left one behind. A claim that is merely refused
		/// leaves the current holder in place, which is what a real second client wants; this suite
		/// is the only driver of the app, so a stale lease from an earlier client or run is taken.
		/// </param>
		/// <param name="ct">Cancellation token.</param>
		private async Task<string> ClaimLeaseAsync(bool forceTakeover, CancellationToken ct)
		{
			var attemptedLeaseId = Guid.NewGuid().ToString("N");
			var payload = new Dictionary<string, object>
			{
				["action"] = "claim",
				["leaseId"] = attemptedLeaseId,
				["holderKind"] = HolderKind,
				["label"] = "AvalonDock DevFlow tests",
				["force"] = forceTakeover,
			};

			// Serialized up front: JsonContent posts a chunked body, which the agent's HTTP server
			// does not read, so the claim would fail on a dropped connection instead of a lease.
			using var content = new StringContent(
				JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			using var resp = await _http.PostAsync(LeaseEndpoint, content, ct).ConfigureAwait(false);
			var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

			if (resp.StatusCode == HttpStatusCode.NotFound)
			{
				// An agent that predates mutation leases allows every mutation.
				lock (LeaseSync) UnleasedPorts.Add(_port);
				return null;
			}

			if (!resp.IsSuccessStatusCode)
				throw new HttpRequestException(
					$"DevFlow lease claim returned {(int)resp.StatusCode}: {raw}");

			if (!string.IsNullOrWhiteSpace(raw))
			{
				using var doc = JsonDocument.Parse(raw);
				if (doc.RootElement.TryGetProperty("youHold", out var youHold)
					&& youHold.ValueKind == JsonValueKind.False)
				{
					if (!forceTakeover)
						return await ClaimLeaseAsync(forceTakeover: true, ct).ConfigureAwait(false);

					throw new InvalidOperationException(
						$"The app refused to hand over the DevFlow mutation lease: {raw}");
				}
			}

			lock (LeaseSync) LeaseIds[_port] = attemptedLeaseId;
			return attemptedLeaseId;
		}

		/// <summary>
		/// A 501 means the agent build has no implementation of that pointer primitive at all (the WPF
		/// agent has none for drag on some Windows configurations). That is a platform limitation, not a
		/// product regression, and it is reported as a skip so it neither fails the run nor spends the
		/// readiness deadlines of every row that needs the primitive.
		/// </summary>
		private static void SkipIfAgentLacksInput(HttpStatusCode status, string label, string raw)
		{
			if (status == HttpStatusCode.NotImplemented)
				Assert.Skip($"The DevFlow agent does not implement '{label}' on this platform: {raw}");
		}

		public async Task<JsonElement> DragAsync(DragRequest request, CancellationToken ct = default)
		{
			var body = JsonSerializer.Serialize(request);
			using var content = new StringContent(body, Encoding.UTF8, "application/json");
			using var resp = await SendMutationAsync(HttpMethod.Post, "/api/v1/ui/actions/drag", content, ct).ConfigureAwait(false);
			var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			SkipIfAgentLacksInput(resp.StatusCode, "drag", raw);
			if (!resp.IsSuccessStatusCode)
				throw new HttpRequestException($"DevFlow drag returned {(int)resp.StatusCode}: {raw}");
			return JsonDocument.Parse(raw).RootElement.Clone();
		}

		public async Task<JsonElement> DragAndAssertOkAsync(DragRequest request, CancellationToken ct = default)
		{
			var result = await DragAsync(request, ct).ConfigureAwait(false);
			if (!result.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
				throw new InvalidOperationException($"DevFlow drag did not report success: {result.GetRawText()}");
			return result;
		}

		// Decomposed press/drag-move/release: unlike DragAsync (one monolithic press-move...-release
		// call), these let a caller inspect live app state BETWEEN steps of a real drag gesture - e.g.
		// AvalonDock's drop-target compass, which only shows/updates once the pointer enters the
		// relevant host and only for whichever host it's currently over. Used to precisely target a
		// specific DropTargetType compass indicator instead of guessing screen offsets.
		public Task PressAsync(double x, double y, CancellationToken ct = default)
			=> PostPointActionAndAssertOkAsync("/api/v1/ui/actions/press", x, y, "press", ct);

		public Task DragMoveAsync(double x, double y, CancellationToken ct = default)
			=> PostPointActionAndAssertOkAsync("/api/v1/ui/actions/drag-move", x, y, "drag-move", ct);

		public Task ReleaseAsync(double x, double y, CancellationToken ct = default)
			=> PostPointActionAndAssertOkAsync("/api/v1/ui/actions/release", x, y, "release", ct);

		/// <summary>
		/// Presses and holds a real OS-level key, so a gesture can run with a modifier down (cliclick's
		/// <c>kd:</c> on macOS). Returns the agent's answer: on a platform without that support it reports
		/// <c>ok:false</c> with a reason, which the caller turns into a skip.
		/// </summary>
		public Task<JsonElement> KeyDownAsync(string key, CancellationToken ct = default)
			=> PostKeyActionAsync("keydown", key, ct);

		/// <summary>Releases a key held by <see cref="KeyDownAsync"/>.</summary>
		public Task<JsonElement> KeyUpAsync(string key, CancellationToken ct = default)
			=> PostKeyActionAsync("keyup", key, ct);

		private async Task<JsonElement> PostKeyActionAsync(string action, string key, CancellationToken ct)
		{
			var body = JsonSerializer.Serialize(new { key });
			using var content = new StringContent(body, Encoding.UTF8, "application/json");
			using var resp = await SendMutationAsync(HttpMethod.Post, $"/api/v1/ui/actions/{action}", content, ct).ConfigureAwait(false);
			var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			if (!resp.IsSuccessStatusCode)
				throw new HttpRequestException($"DevFlow {action} returned {(int)resp.StatusCode}: {raw}");
			return JsonDocument.Parse(raw).RootElement.Clone();
		}

		public async Task ClickAsync(double x, double y, CancellationToken ct = default)
		{
			// Said on stderr, which the CI log keeps for every platform: what the point lands on and
			// what the application's input diagnostics counted afterwards, so a click that "did
			// nothing" can be told from one that hit something else or never arrived. TestApp only.
			string before = null;
			try { before = await InvokeAsync("avd.hit-test", x, y).ConfigureAwait(false); }
			catch (Exception) { }

			await PostPointActionAndAssertOkAsync("/api/v1/ui/actions/click", x, y, "click", ct).ConfigureAwait(false);

			if (before != null)
			{
				try
				{
					await Task.Delay(150, ct).ConfigureAwait(false);
					var after = await InvokeAsync("avd.input.query").ConfigureAwait(false);
					Console.Error.WriteLine($"[click] ({x},{y}) hit={Truncate(before, 700)} input={Truncate(after, 600)}");
				}
				catch (Exception ex) when (ex is not OperationCanceledException)
				{
					Console.Error.WriteLine($"[click] ({x},{y}) diagnostics failed: {ex.Message}");
				}
			}
		}

		private static string Truncate(string value, int max)
			=> value.Length <= max ? value : value.Substring(0, max) + "...";

		private async Task PostPointActionAndAssertOkAsync(string path, double x, double y, string label, CancellationToken ct)
		{
			var body = JsonSerializer.Serialize(new { x, y });
			using var content = new StringContent(body, Encoding.UTF8, "application/json");
			using var resp = await SendMutationAsync(HttpMethod.Post, path, content, ct).ConfigureAwait(false);
			var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			SkipIfAgentLacksInput(resp.StatusCode, label, raw);
			if (!resp.IsSuccessStatusCode)
				throw new HttpRequestException($"DevFlow {label} returned {(int)resp.StatusCode}: {raw}");
			using var doc = JsonDocument.Parse(raw);
			if (!doc.RootElement.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
				throw new InvalidOperationException($"DevFlow {label} did not report success: {raw}");
		}

		/// <summary>
		/// Queries every drop-target compass indicator currently visible during an in-progress drag
		/// (see avd.query.active-drop-targets), keyed by DropTargetType name.
		/// </summary>
		public async Task<List<DropTargetInfo>> QueryActiveDropTargetsAsync(CancellationToken ct = default)
		{
			var json = await InvokeAsync("avd.query.active-drop-targets").ConfigureAwait(false);
			using var doc = JsonDocument.Parse(json);
			var result = new List<DropTargetInfo>();
			foreach (var element in doc.RootElement.EnumerateArray())
			{
				result.Add(new DropTargetInfo(
					element.GetProperty("type").GetString(),
					element.GetProperty("x").GetDouble(),
					element.GetProperty("y").GetDouble(),
					element.GetProperty("width").GetDouble(),
					element.GetProperty("height").GetDouble(),
					element.GetProperty("centerX").GetDouble(),
					element.GetProperty("centerY").GetDouble()));
			}

			return result;
		}

		/// <summary>
		/// Polls avd.query.active-drop-targets until a target of the given DropTargetType name appears
		/// (the compass only populates once the pointer has settled over the relevant host after a
		/// drag-move), or throws after the timeout.
		/// </summary>
		public async Task<DropTargetInfo> WaitForActiveDropTargetAsync(string dropTargetType, CancellationToken ct, TimeSpan? timeout = null)
		{
			var deadline = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromSeconds(8));
			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				var targets = await QueryActiveDropTargetsAsync(ct).ConfigureAwait(false);
				var match = PickPrimaryDropTarget(targets, dropTargetType);
				if (match != null)
					return match;

				await Task.Delay(250, ct).ConfigureAwait(false);
			}

			throw new TimeoutException($"Timed out waiting for drop target '{dropTargetType}' to appear in the active compass.");
		}

		/// <summary>Selects the primary compass indicator for a DropTargetType from the active set.
		/// Several indicators can share one type name - notably the *DockInside types, where the same
		/// name is used by the central ~40x40 "dock inside" button AND by the wide/short tab-header and
		/// thin tab-strip "dock as a new tab" targets. A plain first-match can capture one of those tab
		/// slivers (whose position/order varies frame to frame), so a later drag to its centre never
		/// re-registers as that zone. The primary direction/inside buttons are all roughly square, so
		/// prefer the most-square match; ties break toward the larger button.</summary>
		public static DropTargetInfo PickPrimaryDropTarget(IReadOnlyList<DropTargetInfo> targets, string dropTargetType)
		{
			return targets
				.Where(t => t.Type == dropTargetType)
				.OrderBy(t => Math.Abs(t.Width - t.Height))
				.ThenByDescending(t => t.Width * t.Height)
				.FirstOrDefault();
		}

		public async Task<ElementBounds> QueryBoundsAsync(string target, string contentId = null)
		{
			var json = contentId == null
				? await InvokeAsync("avd.query.bounds", target).ConfigureAwait(false)
				: await InvokeAsync("avd.query.bounds", target, contentId).ConfigureAwait(false);
			return ReadElementBounds(json);
		}

		public async Task<ElementBounds> QueryDragHandleAsync(string target, string contentId = null)
		{
			var json = contentId == null
				? await InvokeAsync("avd.query.drag-handle", target).ConfigureAwait(false)
				: await InvokeAsync("avd.query.drag-handle", target, contentId).ConfigureAwait(false);
			return ReadElementBounds(json);
		}

		private static ElementBounds ReadElementBounds(string json)
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			if (!root.TryGetProperty("found", out var found) || found.ValueKind != JsonValueKind.True)
				throw new InvalidOperationException($"AvalonDock target bounds not found: {json}");
			var bounds = new ElementBounds(
				root.GetProperty("x").GetDouble(),
				root.GetProperty("y").GetDouble(),
				root.GetProperty("width").GetDouble(),
				root.GetProperty("height").GetDouble());
			if (bounds.Width <= 0 || bounds.Height <= 0)
				throw new InvalidOperationException($"AvalonDock target bounds are empty: {json}");
			return bounds;
		}

		public async Task AssertFloatingWindowAboveMainAsync(string contentId)
		{
			var json = await InvokeAsync("avd.query.floating-zorder", contentId).ConfigureAwait(false);
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			if (!root.TryGetProperty("found", out var found) || found.ValueKind != JsonValueKind.True)
				throw new InvalidOperationException($"Floating window '{contentId}' was not found while checking z-order: {json}");

			var hasMainZ = root.TryGetProperty("mainZOrderFound", out var mainFound) && mainFound.ValueKind == JsonValueKind.True;
			var hasFloatingZ = root.TryGetProperty("floatingZOrderFound", out var floatingFound) && floatingFound.ValueKind == JsonValueKind.True;
			if (!hasMainZ || !hasFloatingZ)
			{
				string systemEventsFailure = null;
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
					&& TryAssertFloatingWindowAboveMainWithSystemEvents(root, out systemEventsFailure))
				{
					return;
				}

				if (systemEventsFailure != null)
					throw new InvalidOperationException(systemEventsFailure + $" Raw z-order payload: {json}");

				// On Linux the verb answers from the X server's _NET_CLIENT_LIST_STACKING, which needs a
				// reachable X display (X11, or XWayland under a Wayland session). Skip only when the
				// verb reports it had no source at all - once z-order IS readable, a window missing
				// from the stacking order is a real result and must fail rather than quietly skip,
				// otherwise re-enabling this check would just trade one blind spot for another.
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
					&& !string.Equals(ReadZOrderSource(root), "x11", StringComparison.Ordinal))
				{
					Xunit.Assert.Skip(
						$"OS z-order for floating windows needs an X display, which this session does " +
						$"not have: {json}");
					return;
				}

				throw new InvalidOperationException($"Could not read OS z-order for floating window '{contentId}': {json}");
			}

			if (!root.TryGetProperty("isFloatingAboveMain", out var above) || above.ValueKind != JsonValueKind.True)
				throw new InvalidOperationException($"Floating window '{contentId}' is not above the main window: {json}");
		}

		/// <summary>
		/// Reports which OS facility the z-order verb answered from ("win32", "appkit", "x11", or
		/// "none"), so a caller can tell "this platform cannot report z-order" apart from "z-order is
		/// readable and the answer is no". Older TestApp builds omit the field entirely.
		/// </summary>
		private static string ReadZOrderSource(JsonElement zOrderPayload)
			=> zOrderPayload.TryGetProperty("zOrderSource", out var source) && source.ValueKind == JsonValueKind.String
				? source.GetString()
				: null;

		private static bool TryAssertFloatingWindowAboveMainWithSystemEvents(JsonElement zOrderPayload, out string failure)
		{
			failure = null;
			var mainTitle = zOrderPayload.TryGetProperty("mainTitle", out var mainTitleElement)
				? mainTitleElement.GetString()
				: "MainWindow";
			var floatingTitle = zOrderPayload.TryGetProperty("floatingTitle", out var floatingTitleElement)
				? floatingTitleElement.GetString()
				: null;
			if (string.IsNullOrWhiteSpace(floatingTitle))
			{
				failure = "Could not verify z-order with System Events because the floating window title was empty.";
				return false;
			}

			var script =
				"tell application \"System Events\"\n" +
				"  tell process \"TestApp\"\n" +
				"    set rows to {}\n" +
				"    repeat with i from 1 to count of windows\n" +
				"      try\n" +
				"        set w to window i\n" +
				"        set rows to rows & ((i as text) & tab & (name of w as text))\n" +
				"      end try\n" +
				"    end repeat\n" +
				"  end tell\n" +
				"end tell\n" +
				"set AppleScript's text item delimiters to linefeed\n" +
				"return rows as text";

			string output;
			try
			{
				output = RunOsaScript(script);
			}
			catch (Exception ex)
			{
				failure = $"Could not verify z-order with System Events: {ex.Message}";
				return false;
			}

			var windows = output
				.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(line => line.Split('\t', 2))
				.Where(parts => parts.Length == 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
				.Select(parts => new
				{
					Index = int.Parse(parts[0], CultureInfo.InvariantCulture),
					Title = parts[1],
				})
				.ToList();
			var main = windows.FirstOrDefault(w => string.Equals(w.Title, mainTitle, StringComparison.Ordinal));
			var floating = windows.FirstOrDefault(w => string.Equals(w.Title, floatingTitle, StringComparison.Ordinal));
			if (main == null || floating == null)
			{
				failure = $"System Events could not find both windows. main='{mainTitle}', floating='{floatingTitle}', windows=[{string.Join(", ", windows.Select(w => $"{w.Index}:{w.Title}"))}]";
				return false;
			}

			if (floating.Index < main.Index)
				return true;

			failure = $"Floating window '{floatingTitle}' is not above main window '{mainTitle}' according to System Events. " +
				$"Lower index is frontmost; mainIndex={main.Index}, floatingIndex={floating.Index}, windows=[{string.Join(", ", windows.Select(w => $"{w.Index}:{w.Title}"))}]";
			return false;
		}

		private static string RunOsaScript(string script)
		{
			var psi = new ProcessStartInfo("osascript")
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			psi.ArgumentList.Add("-e");
			psi.ArgumentList.Add(script);
			using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start osascript.");
			var stdout = process.StandardOutput.ReadToEnd();
			var stderr = process.StandardError.ReadToEnd();
			process.WaitForExit();
			if (process.ExitCode != 0)
				throw new InvalidOperationException(stderr.Trim());
			return stdout.Trim();
		}

		public async Task<List<JsonElement>> QueryElementsAsync(string type, int maxResults = 20, int maxDepth = 96, CancellationToken ct = default)
		{
			var path = $"/api/v1/ui/elements?type={Uri.EscapeDataString(type)}&maxResults={maxResults}&maxDepth={maxDepth}";
			var raw = await GetStringAsync(path, ct).ConfigureAwait(false);
			using var doc = JsonDocument.Parse(raw);
			var result = new List<JsonElement>();
			foreach (var element in doc.RootElement.EnumerateArray())
				result.Add(element.Clone());
			return result;
		}

		public async Task<ElementBounds> WaitForStableElementBoundsAsync(string type, CancellationToken ct)
		{
			ElementBounds? previous = null;
			var stableCount = 0;
			var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30);

			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				var elements = await QueryElementsAsync(type, maxResults: 10, maxDepth: 96, ct).ConfigureAwait(false);
				foreach (var element in elements)
				{
					if (!element.TryGetProperty("bounds", out var bounds) || bounds.ValueKind != JsonValueKind.Object)
						continue;

					var current = DevFlowTree.Bounds(element);
					if (current.Width <= 0 || current.Height <= 0)
						continue;

					if (previous.HasValue && current.IsCloseTo(previous.Value))
					{
						stableCount++;
						if (stableCount >= 2)
							return current;
					}
					else
					{
						previous = current;
						stableCount = 1;
					}

					break;
				}

				await Task.Delay(500, ct).ConfigureAwait(false);
			}

			throw new TimeoutException($"Timed out waiting for stable bounds for WPF element type '{type}'.");
		}

		public async Task<DockLayoutSnapshot> WaitForAvalonDockReadyAsync(CancellationToken ct)
		{
			var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30);
			DockLayoutSnapshot snapshot = null;

			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				try
				{
					var json = await InvokeAsync("avd.query.layout").ConfigureAwait(false);
					snapshot = DockLayoutSnapshot.Parse(json);
					if (snapshot.Documents.Exists(d => d.ContentId == "document1")
						&& snapshot.Anchorables.Exists(a => a.ContentId == "toolWindow1"))
					{
						return snapshot;
					}
				}
				catch (Exception ex) when (ex is HttpRequestException or JsonException or TimeoutException or TaskCanceledException)
				{
				}

				await Task.Delay(500, ct).ConfigureAwait(false);
			}

			throw new TimeoutException("Timed out waiting for AvalonDock TestApp layout and visual bounds to become ready.");
		}

		public async Task<DockLayoutSnapshot> WaitForAvalonDockTestLayoutReadyAsync(CancellationToken ct)
		{
			var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30);
			DockLayoutSnapshot snapshot = null;

			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				try
				{
					var json = await InvokeAsync("avd.query.layout").ConfigureAwait(false);
					snapshot = DockLayoutSnapshot.Parse(json);
					if (snapshot.Documents.Exists(d => d.ContentId == "dragTestDocument")
						&& snapshot.Anchorables.Exists(a => a.ContentId == "dragTestTool"))
					{
						return snapshot;
					}
				}
				catch (Exception ex) when (ex is HttpRequestException or JsonException or TimeoutException or TaskCanceledException)
				{
				}

				await Task.Delay(500, ct).ConfigureAwait(false);
			}

			throw new TimeoutException("Timed out waiting for AvalonDock drag/drop test layout to become ready.");
		}

		private async Task<string> GetStringAsync(string path, CancellationToken ct)
		{
			using var resp = await _http.GetAsync(path, ct).ConfigureAwait(false);
			resp.EnsureSuccessStatusCode();
			return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
		}

		// DevFlow may return the action's string directly or wrap it in an envelope.
		// Be tolerant of both so tests don't couple to the exact wrapper shape.
		private static string ExtractResult(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return raw;
			try
			{
				using var doc = JsonDocument.Parse(raw);
				if (doc.RootElement.ValueKind == JsonValueKind.Object)
				{
					foreach (var key in new[] { "returnValue", "result", "value", "output", "data" })
					{
						if (doc.RootElement.TryGetProperty(key, out var el))
							return el.ValueKind == JsonValueKind.String ? el.GetString() : el.GetRawText();
					}
				}
				if (doc.RootElement.ValueKind == JsonValueKind.String)
					return doc.RootElement.GetString();
			}
			catch { /* not JSON — fall through to raw */ }
			return raw;
		}

		// The mutation lease is deliberately not released: the next client for this app is the same
		// process and reuses it, and a released lease would only have to be re-claimed.
		public void Dispose() => _http.Dispose();
	}

	public sealed class DragRequest
	{
		public string FromId { get; set; }
		public string ToId { get; set; }
		public double? FromX { get; set; }
		public double? FromY { get; set; }
		public double? ToX { get; set; }
		public double? ToY { get; set; }
		public double? Dx { get; set; }
		public double? Dy { get; set; }
		public int? Steps { get; set; }
		public bool Global { get; set; }
	}

	/// <summary>A live drop-target compass indicator: its DropTargetType name and screen bounds.</summary>
	public sealed class DropTargetInfo
	{
		public DropTargetInfo(string type, double x, double y, double width, double height, double centerX, double centerY)
		{
			Type = type;
			X = x;
			Y = y;
			Width = width;
			Height = height;
			CenterX = centerX;
			CenterY = centerY;
		}

		public string Type { get; }
		public double X { get; }
		public double Y { get; }
		public double Width { get; }
		public double Height { get; }
		public double CenterX { get; }
		public double CenterY { get; }

		public override string ToString() => $"{Type} [{X},{Y},{Width},{Height}] @ {CenterX},{CenterY}";
	}

	public readonly struct ElementBounds
	{
		public ElementBounds(double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public double X { get; }
		public double Y { get; }
		public double Width { get; }
		public double Height { get; }
		public double CenterX => X + Width / 2d;
			public double CenterY => Y + Height / 2d;
			public double Right => X + Width;
			public double Bottom => Y + Height;
			public bool Contains(double x, double y) => x >= X && x <= Right && y >= Y && y <= Bottom;

			public override string ToString()
			=> string.Create(CultureInfo.InvariantCulture, $"{X},{Y} {Width}x{Height}");

		public bool IsCloseTo(ElementBounds other)
			=> Math.Abs(X - other.X) < 0.5
				&& Math.Abs(Y - other.Y) < 0.5
				&& Math.Abs(Width - other.Width) < 0.5
				&& Math.Abs(Height - other.Height) < 0.5;
	}

	public static class DevFlowTree
	{
		public static JsonElement ParseTree(string json)
			=> JsonDocument.Parse(json).RootElement.Clone();

		public static JsonElement? FindFirst(JsonElement root, Func<JsonElement, bool> predicate)
		{
			if (root.ValueKind == JsonValueKind.Object)
			{
				if (predicate(root))
					return root;

				if (root.TryGetProperty("elements", out var elements))
				{
					var found = FindFirst(elements, predicate);
					if (found.HasValue)
						return found;
				}

				if (root.TryGetProperty("children", out var children))
				{
					var found = FindFirst(children, predicate);
					if (found.HasValue)
						return found;
				}
			}
			else if (root.ValueKind == JsonValueKind.Array)
			{
				foreach (var child in root.EnumerateArray())
				{
					var found = FindFirst(child, predicate);
					if (found.HasValue)
						return found;
				}
			}

			return null;
		}

		public static string Id(JsonElement element)
			=> element.TryGetProperty("id", out var id) ? id.GetString() : null;

		public static string Type(JsonElement element)
			=> element.TryGetProperty("type", out var type) ? type.GetString() : null;

		public static string Text(JsonElement element)
			=> element.TryGetProperty("text", out var text) ? text.GetString() : null;

		public static ElementBounds Bounds(JsonElement element)
		{
			var bounds = element.GetProperty("bounds");
			return new ElementBounds(
				bounds.GetProperty("x").GetDouble(),
				bounds.GetProperty("y").GetDouble(),
				bounds.GetProperty("width").GetDouble(),
				bounds.GetProperty("height").GetDouble());
		}
	}

	// Strongly-typed view of the dock-query-layout JSON, so assertions read cleanly.
	public sealed class DockLayoutSnapshot
	{
		public List<ContentInfo> Documents { get; } = new();
		public List<ContentInfo> Anchorables { get; } = new();
		public List<PaneInfo> DocumentPanes { get; } = new();
		public List<PaneInfo> AnchorablePanes { get; } = new();
		public List<FloatingInfo> FloatingWindows { get; } = new();
		public List<string> Hidden { get; } = new();

		public sealed class ContentInfo
		{
			public string ContentId { get; set; }
			public string Title { get; set; }
			public bool IsVisible { get; set; }
			public bool IsHidden { get; set; }
			public bool IsFloat { get; set; }
		}

		public sealed class PaneInfo
		{
			public List<string> Tabs { get; } = new();
			public int Selected { get; set; }
		}

		public sealed class FloatingInfo
		{
			public string Kind { get; set; }
			public List<string> Contents { get; } = new();
			public double? FloatingLeft { get; set; }
			public double? FloatingTop { get; set; }
			public double? FloatingWidth { get; set; }
			public double? FloatingHeight { get; set; }
		}

		public static DockLayoutSnapshot Parse(string json)
		{
			var snap = new DockLayoutSnapshot();
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			foreach (var p in EnumArray(root, "documentPanes"))
				snap.DocumentPanes.Add(ReadPane(p));
			foreach (var p in EnumArray(root, "anchorablePanes"))
				snap.AnchorablePanes.Add(ReadPane(p));
			foreach (var d in EnumArray(root, "documents"))
				snap.Documents.Add(ReadContent(d));
			foreach (var a in EnumArray(root, "anchorables"))
				snap.Anchorables.Add(ReadContent(a));
			foreach (var f in EnumArray(root, "floatingWindows"))
			{
				var fi = new FloatingInfo
				{
					Kind = f.TryGetProperty("kind", out var k)
						? k.GetString()
						: f.TryGetProperty("type", out var t) ? t.GetString() : null,
				};
				if (!f.TryGetProperty("contents", out var contents))
					f.TryGetProperty("contentIds", out contents);
				if (contents.ValueKind == JsonValueKind.Array)
					foreach (var c in contents.EnumerateArray())
						fi.Contents.Add(c.GetString());
				fi.FloatingLeft = ReadNullableDouble(f, "floatingLeft");
				fi.FloatingTop = ReadNullableDouble(f, "floatingTop");
				fi.FloatingWidth = ReadNullableDouble(f, "floatingWidth");
				fi.FloatingHeight = ReadNullableDouble(f, "floatingHeight");
				snap.FloatingWindows.Add(fi);
			}
			if (root.TryGetProperty("hidden", out var hidden) && hidden.ValueKind == JsonValueKind.Array)
				foreach (var h in hidden.EnumerateArray())
					snap.Hidden.Add(h.GetString());

			return snap;
		}

		private static IEnumerable<JsonElement> EnumArray(JsonElement root, string name)
		{
			if (root.TryGetProperty(name, out var arr) && arr.ValueKind == JsonValueKind.Array)
				foreach (var e in arr.EnumerateArray())
					yield return e;
		}

		private static PaneInfo ReadPane(JsonElement p)
		{
			var info = new PaneInfo();
			if (p.TryGetProperty("tabs", out var tabs))
				foreach (var t in tabs.EnumerateArray())
					info.Tabs.Add(t.GetString());
			if (p.TryGetProperty("selected", out var sel) && sel.ValueKind == JsonValueKind.Number)
				info.Selected = sel.GetInt32();
			return info;
		}

		private static ContentInfo ReadContent(JsonElement element)
		{
			return new ContentInfo
			{
				ContentId = element.TryGetProperty("contentId", out var contentId) ? contentId.GetString() : null,
				Title = element.TryGetProperty("title", out var title) ? title.GetString() : null,
				IsVisible = element.TryGetProperty("isVisible", out var isVisible) && isVisible.ValueKind == JsonValueKind.True,
				IsHidden = element.TryGetProperty("isHidden", out var isHidden) && isHidden.ValueKind == JsonValueKind.True,
				IsFloat = element.TryGetProperty("isFloat", out var isFloat) && isFloat.ValueKind == JsonValueKind.True,
			};
		}

		private static double? ReadNullableDouble(JsonElement element, string name)
		{
			if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
				return null;
			return value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
		}
	}
}
