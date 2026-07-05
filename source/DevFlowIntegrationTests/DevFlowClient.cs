using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
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
		private readonly HttpClient _http;

		public DevFlowClient(int port)
		{
			_http = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };
			_http.Timeout = TimeSpan.FromSeconds(60);
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
			using var resp = await _http.PostAsync($"/api/v1/invoke/actions/{action}", content).ConfigureAwait(false);
			var raw = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
			if (!resp.IsSuccessStatusCode)
				throw new HttpRequestException(
					$"DevFlow action '{action}' returned {(int)resp.StatusCode}: {raw}");
			return ExtractResult(raw);
		}

		public async Task<JsonElement> DragAsync(DragRequest request, CancellationToken ct = default)
		{
			var body = JsonSerializer.Serialize(request);
			using var content = new StringContent(body, Encoding.UTF8, "application/json");
			using var resp = await _http.PostAsync("/api/v1/ui/actions/drag", content, ct).ConfigureAwait(false);
			var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
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

		public async Task<ElementBounds> QueryBoundsAsync(string target, string contentId = null)
		{
			var json = contentId == null
				? await InvokeAsync("avd.query.bounds", target).ConfigureAwait(false)
				: await InvokeAsync("avd.query.bounds", target, contentId).ConfigureAwait(false);
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
