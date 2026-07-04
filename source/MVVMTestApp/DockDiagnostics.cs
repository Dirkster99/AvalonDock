// DevFlow diagnostic actions for the AvalonDock MVVM sample (WPF / ProGPU).
// These static methods are discovered by DevFlow's InvokeAction scanner and are
// the deterministic verbs the integration test suite drives (see
// AvalonDockTest.Integration.FloatRoundTripIntegrationTests).
//
// Every action marshals to the UI thread via Application.Current.Dispatcher and
// operates purely on the DockingManager layout model, so results are stable and
// don't depend on injected OS mouse events.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AvalonDock;
using AvalonDock.Layout;
using LeXtudio.DevFlow.Agent.Core;

namespace AvalonDock.MVVMTestApp
{
	public static class DockDiagnostics
	{
		private static DockingManager GetDM()
			=> (Application.Current?.MainWindow as MainWindow)?.DockManager;

		// Run an action on the UI thread and wait for it to complete (up to 10s).
		private static T RunOnUI<T>(Func<T> fn)
		{
			var dispatcher = Application.Current?.Dispatcher;
			if (dispatcher == null) throw new InvalidOperationException("no Dispatcher");
			return dispatcher.Invoke(fn, System.Windows.Threading.DispatcherPriority.Normal);
		}

		[DevFlowAction("dock-query-layout",
			Description = "Returns the current layout as deterministic JSON (document panes, " +
			              "anchorable panes, floating windows, hidden) for reliable test assertions.")]
		public static string QueryLayout() => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "{\"error\":\"no DockingManager\"}";
			var layout = dm.Layout;
			if (layout == null) return "{\"error\":\"no Layout\"}";

			string Q(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
			string Arr(IEnumerable<string> items) => "[" + string.Join(",", items) + "]";

			string DocPane(LayoutDocumentPane p) =>
				"{\"tabs\":" + Arr(p.Children.Select(c => Q(c.ContentId ?? c.Title))) +
				",\"selected\":" + p.SelectedContentIndex + "}";
			string AnchPane(LayoutAnchorablePane p) =>
				"{\"tabs\":" + Arr(p.Children.Select(c => Q(c.ContentId ?? c.Title))) +
				",\"selected\":" + p.SelectedContentIndex + "}";
			string Fw(LayoutFloatingWindow f) =>
				"{\"kind\":" + Q(f is LayoutAnchorableFloatingWindow ? "anchorable" : "document") +
				",\"contents\":" + Arr(f.Descendents().OfType<LayoutContent>().Select(c => Q(c.ContentId ?? c.Title))) + "}";

			// Docked panes come from RootPanel only — NOT layout.Descendents(), which also
			// walks into floating windows' internal panes (so a floated tool would wrongly
			// appear both here and under floatingWindows). Floating panes are reported
			// separately below via layout.FloatingWindows.
			var rootDescendents = layout.RootPanel?.Descendents() ?? Enumerable.Empty<ILayoutElement>();
			var docPanes = rootDescendents.OfType<LayoutDocumentPane>().Select(DocPane);
			var anchPanes = rootDescendents.OfType<LayoutAnchorablePane>().Select(AnchPane);
			var fws = (layout.FloatingWindows ?? Enumerable.Empty<LayoutFloatingWindow>()).Select(Fw);
			var hidden = (layout.Hidden ?? Enumerable.Empty<LayoutAnchorable>()).Select(a => Q(a.ContentId ?? a.Title));

			return "{\"documentPanes\":" + Arr(docPanes) +
			       ",\"anchorablePanes\":" + Arr(anchPanes) +
			       ",\"floatingWindows\":" + Arr(fws) +
			       ",\"hidden\":" + Arr(hidden) + "}";
		});

		[DevFlowAction("dock-seed-documents",
			Description = "Ensure the layout has 'count' documents with deterministic ContentIds " +
			              "(doc1, doc2, ...). Idempotent. Call in test setup to guarantee documents exist.")]
		public static string SeedDocuments(int count = 2) => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			for (var i = 1; i <= count; i++)
				Workspace.This.AddDocument($"doc{i}");
			dm.UpdateLayout();
			var docPanes = dm.Layout?.RootPanel?.Descendents().OfType<LayoutDocumentPane>().ToList() ?? new();
			return $"seeded count={count} docTabs=[{string.Join(",", docPanes.SelectMany(p => p.Children.Select(c => c.ContentId ?? c.Title)))}]";
		});

		[DevFlowAction("dock-float-active",
			Description = "Float the currently active/selected document tab.")]
		public static string FloatActive() => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			var pane = dm.Layout?.Descendents().OfType<LayoutDocumentPane>()
				.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);
			var content = pane?.SelectedContent ?? pane?.Children.FirstOrDefault();
			if (content == null) return "no selected content";
			content.Float();
			var layoutFwCount = dm.Layout?.FloatingWindows?.Count ?? -1;
			return $"floated: {content.Title} layoutFw={layoutFwCount}";
		});

		[DevFlowAction("dock-float-anchorable",
			Description = "Float an anchorable/tool tab by ContentId.")]
		public static string FloatAnchorable(string contentId) => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			var content = dm.Layout?.Descendents().OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId);
			if (content == null) return $"anchorable '{contentId}' not found";
			content.Float();
			var layoutFwCount = dm.Layout?.FloatingWindows?.Count ?? -1;
			return $"floated: {content.Title} layoutFw={layoutFwCount}";
		});

		[DevFlowAction("dock-simulate-drop",
			Description = "Drop the first floating window's content at the given zone " +
			              "(Center/Left/Right/Top/Bottom/OuterLeft/OuterRight/OuterTop/OuterBottom).")]
		public static string SimulateDrop(string zone = "Center") => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			var layout = dm.Layout;
			var fw = layout?.FloatingWindows?.FirstOrDefault();
			if (fw == null) return "no floating windows";
			if (!Enum.TryParse<CompassDropZone>(zone, true, out var z))
				z = CompassDropZone.Center;

			var content = fw.Descendents().OfType<LayoutContent>().FirstOrDefault();
			if (content == null) return "floating window has no content";

			// Detach the content from its floating pane, then re-insert into the docked
			// layout at the requested zone. CollectGarbage prunes the now-empty floating
			// window (model + control).
			if (content.Parent is ILayoutGroup group)
			{
				var idx = group.IndexOfChild(content);
				if (idx >= 0) group.RemoveChildAt(idx);
			}

			LayoutRootMutations.InsertPane(layout, content, z);
			layout.CollectGarbage();

			var docPanes = layout.RootPanel?.Descendents().OfType<LayoutDocumentPane>().ToList() ?? new();
			var remaining = layout.FloatingWindows?.Count ?? 0;
			return $"done zone={z} floatingRemaining={remaining} docPanes={docPanes.Count} " +
			       $"tabCounts=[{string.Join(",", docPanes.Select(p => p.ChildrenCount))}]";
		});

		[DevFlowAction("dock-toggle-autohide",
			Description = "Toggle auto-hide for anchorable with given ContentId (moves to/from side panel).")]
		public static string ToggleAutoHide(string contentId) => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			var anc = dm.Layout?.Descendents().OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId);
			if (anc == null) return $"anchorable '{contentId}' not found";
			var wasDocked = !anc.IsAutoHidden;
			anc.ToggleAutoHide();
			return $"{anc.Title}: was docked={wasDocked} now isAutoHidden={anc.IsAutoHidden}";
		});

		[DevFlowAction("dock-hide-anchorable",
			Description = "Hide the anchorable with the given ContentId (moves it to LayoutRoot.Hidden).")]
		public static string HideAnchorable(string contentId) => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			var anc = dm.Layout?.Descendents().OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId);
			if (anc == null) return $"anchorable '{contentId}' not found";
			anc.Hide();
			return $"hidden: {anc.Title} isHidden={anc.IsHidden} hiddenCount={dm.Layout?.Hidden?.Count ?? 0}";
		});

		[DevFlowAction("dock-show-hidden",
			Description = "Show the first hidden anchorable (restores to previous container).")]
		public static string ShowHidden() => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			var hidden = dm.Layout?.Hidden?.ToList() ?? new();
			if (hidden.Count == 0) return "no hidden anchorables";
			var anc = hidden[0];
			var title = anc.Title;
			anc.Show();
			return $"shown: {title} isVisible={anc.IsVisible} hiddenRemaining={dm.Layout?.Hidden?.Count ?? 0}";
		});

		[DevFlowAction("dock-list-hidden",
			Description = "Lists all hidden anchorables.")]
		public static string ListHidden() => RunOnUI(() =>
		{
			var dm = GetDM();
			if (dm == null) return "no DockingManager";
			var hidden = dm.Layout?.Hidden?.ToList() ?? new();
			if (hidden.Count == 0) return "no hidden anchorables";
			return string.Join(", ", hidden.Select(a => $"{a.Title}[{a.ContentId}]"));
		});
	}
}
