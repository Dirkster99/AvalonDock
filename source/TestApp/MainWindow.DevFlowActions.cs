// The action surface of the sample, kept as static methods on purpose.
//
// DevFlow resolves an action either as a static method or, on the LibreWPF agent only, as an
// instance method of a window. The WPF agent (real WPF, the Windows legs) resolves static methods
// only, so an action declared on the window is invisible there and every call answers 404 - which
// is how the Windows CI leg failed before this file existed. Each action is therefore a one-line
// forwarder to the window method that implements it, on both platforms and with the action names
// the suites use.
using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows;
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace TestApp
{
	public partial class MainWindow
	{
		/// <summary>
		/// Runs an action body on the thread that owns the window. DevFlow starts the agent from
		/// <c>App.OnStartup</c>, before the window exists, and the WPF agent invokes an action on the
		/// HTTP request thread - where even reading <c>Application.MainWindow</c> throws - while the
		/// LibreWPF agent already dispatches to the application's thread. Looking the window up and
		/// calling the body both therefore happen inside the application's dispatcher.
		/// </summary>
		private static T OnUi<T>(Func<MainWindow, T> action)
		{
			var application = Application.Current
				?? throw new InvalidOperationException("The sample application is not running.");

			// The exception is carried out of the dispatcher rather than thrown inside it: an exception
			// that escapes a dispatcher callback is first offered to the dispatcher's unhandled-exception
			// handler on the UI thread, and can end the process there before the caller ever sees it.
			// Carrying it out means the agent reports one failed action instead.
			ExceptionDispatchInfo failure = null;
			T result = default;
			void Run()
			{
				try
				{
					result = action(ResolveWindow(application));
				}
				catch (Exception ex)
				{
					failure = ExceptionDispatchInfo.Capture(ex);
				}
			}

			if (application.Dispatcher.CheckAccess())
				Run();
			else
				application.Dispatcher.Invoke(Run);

			failure?.Throw();
			return result;
		}

		private static MainWindow ResolveWindow(Application application) =>
			(application.MainWindow as MainWindow)
			?? application.Windows.OfType<MainWindow>().FirstOrDefault()
			?? throw new InvalidOperationException("The sample's main window is not available yet.");

		[DevFlowAction("avd.splitter.width",
			Description = "Sets DockingManager.GridSplitterWidth/Height. The docked splitter is only 6px " +
			              "wide; widening it tests whether the resize cursor simply never appears, or " +
			              "appears but is too narrow a target to notice.")]
		public static string SetSplitterWidthAction(double width) => OnUi(w => w.SetSplitterWidth(width));

		[DevFlowAction("avd.query.splitters",
			Description = "Reports every LayoutGridResizerControl in the docked layout: the Cursor it " +
			              "carries, its size, and whether it is hit-test visible. Distinguishes 'the " +
			              "cursor was never assigned' from 'assigned but never applied'.")]
		public static string QuerySplittersAction() => OnUi(w => w.QuerySplitters());

		[DevFlowAction("avd.timer.query",
			Description = "Returns the DispatcherTimer-driven counter. Auto-hide's close timer is a " +
			              "DispatcherTimer, so whether this advances tells you if auto-close can work.")]
		public static string TimerQueryAction() => OnUi(w => w.TimerQuery());

		[DevFlowAction("avd.menu.trace", Description = "Returns (and clears) the recorded menu open/close trace")]
		public static string MenuTraceAction() => OnUi(w => w.MenuTrace());

		[DevFlowAction("avd.float", Description = "Float an anchorable by ContentId")]
		public static string FloatAnchorableAction(string contentId) => OnUi(w => w.FloatAnchorable(contentId));

		[DevFlowAction("avd.position-floating",
			Description = "Move the floating window containing the given ContentId to a fixed screen " +
			              "position (default: clear of the main window and screen origin, where other " +
			              "app windows commonly sit and can steal synthetic clicks aimed at the tester).")]
		public static string PositionFloatingWindowAction(string contentId, double left = 900, double top = 200) => OnUi(w => w.PositionFloatingWindow(contentId, left, top));

		[DevFlowAction("avd.query.floating-zorder", Description = "Compare a floating window's OS z-order against the main window")]
		public static string QueryFloatingZOrderAction(string contentId) => OnUi(w => w.QueryFloatingZOrder(contentId));

		[DevFlowAction("avd.query.x11-stacking", Description = "Dump the X server's window stacking order and which entries belong to this app")]
		public static string QueryX11StackingAction() => OnUi(w => w.QueryX11Stacking());

		[DevFlowAction("avd.query.macos-view-tree", Description = "Return the native AppKit view class tree for main, floating, or overlay window diagnostics")]
		public static string QueryMacOSViewTreeAction(string target = "main", string contentId = "dragTestTool") => OnUi(w => w.QueryMacOSViewTree(target, contentId));

		[DevFlowAction("avd.dock", Description = "Dock a floating anchorable back to main layout")]
		public static string DockAnchorableAction(string contentId) => OnUi(w => w.DockAnchorable(contentId));

		[DevFlowAction("avd.hide", Description = "Hide an anchorable")]
		public static string HideAnchorableAction(string contentId) => OnUi(w => w.HideAnchorable(contentId));

		[DevFlowAction("avd.show", Description = "Show a hidden anchorable")]
		public static string ShowAnchorableAction(string contentId) => OnUi(w => w.ShowAnchorable(contentId));

		[DevFlowAction("avd.add-documents", Description = "Add two test documents to the first document pane")]
		public static string AddDocumentsAction() => OnUi(w => w.AddDocuments());

		[DevFlowAction("avd.debug.border-repro", Description = "Show a window reproducing the compass Border rendering")]
		public static string ShowBorderReproAction(double left = 60, double top = 700) => OnUi(w => w.ShowBorderRepro(left, top));

		[DevFlowAction("avd.add-anchorable", Description = "Add a new anchorable to the layout")]
		public static string AddAnchorableAction(string title = null) => OnUi(w => w.AddAnchorable(title));

		[DevFlowAction("avd.test-layout.reset", Description = "Reset to a deterministic AvalonDock drag/drop test layout")]
		public static string ResetDragDropTestLayoutAction() => OnUi(w => w.ResetDragDropTestLayout());

		[DevFlowAction("avd.switch-theme", Description = "Switch AvalonDock theme by tag name")]
		public static string SwitchThemeAction(string themeTag) => OnUi(w => w.SwitchTheme(themeTag));

		[DevFlowAction("avd.layout.serialize", Description = "Serialize current layout to XML and return it")]
		public static string SerializeLayoutAction() => OnUi(w => w.SerializeLayout());

		[DevFlowAction("avd.layout.restore", Description = "Restore layout from XML string")]
		public static string RestoreLayoutAction(string xml) => OnUi(w => w.RestoreLayout(xml));

		[DevFlowAction("avd.query.layout", Description = "Query current layout state as JSON")]
		public static string QueryLayoutAction() => OnUi(w => w.QueryLayout());

			[DevFlowAction("avd.query.bounds", Description = "Query screen bounds for a dock test target")]
			public static string QueryBoundsAction(string target, string contentId = null) => OnUi(w => w.QueryBounds(target, contentId));

			[DevFlowAction("avd.query.drag-handle", Description = "Query a verified screen drag start point for an AvalonDock target")]
			public static string QueryDragHandleAction(string target, string contentId = null) => OnUi(w => w.QueryDragHandle(target, contentId));

			[DevFlowAction("avd.debug-show-overlay", Description = "Force-show the DockingManager compass overlay (with a visible debug border) without a drag; optionally enter a drop-target zone to also render its blue preview box, for inspecting overlay/preview alignment vs the manager")]
			public static string DebugShowOverlayAction(string previewZone = null) => OnUi(w => w.DebugShowOverlay(previewZone));

			[DevFlowAction("avd.debug-hide-overlay", Description = "Hide the debug compass overlay shown by avd.debug-show-overlay")]
			public static string DebugHideOverlayAction() => OnUi(w => w.DebugHideOverlay());

			[DevFlowAction("avd.activate", Description = "Activate and foreground the AvalonDock test window")]
			public static string ActivateTestWindowAction() => OnUi(w => w.ActivateTestWindow());

			[DevFlowAction("avd.activate-floating", Description = "Raise a floating AvalonDock window before native input safety checks")]
			public static string ActivateFloatingWindowAction(string contentId = null) => OnUi(w => w.ActivateFloatingWindow(contentId));

			[DevFlowAction("avd.position-main-window", Description = "Move the AvalonDock test window to the primary-screen test area")]
			public static string PositionMainWindowAction(double left = 50, double top = 40) => OnUi(w => w.PositionMainWindow(left, top));

			[DevFlowAction("avd.main-window-position-guard.start", Description = "Start recording any native main-window movement during the current test")]
			public static string StartMainWindowPositionGuardAction() => OnUi(w => w.StartMainWindowPositionGuard());

			[DevFlowAction("avd.main-window-position-guard.query", Description = "Report whether the native main window moved since the guard was started")]
			public static string QueryMainWindowPositionGuardAction() => OnUi(w => w.QueryMainWindowPositionGuard());

			[DevFlowAction("avd.input.reset", Description = "Reset AvalonDock routed input diagnostics")]
			public static string ResetInputDiagnosticsAction() => OnUi(w => w.ResetInputDiagnostics());

		[DevFlowAction("avd.input.query", Description = "Query AvalonDock routed input diagnostics")]
		public static string QueryInputDiagnosticsAction() => OnUi(w => w.QueryInputDiagnostics());

		[DevFlowAction("avd.menu.capture-test",
			Description = "Diagnostic: attempts the subtree mouse capture that WPF's menu mode requires " +
			              "(Mouse.Capture(menu, CaptureMode.SubTree)) and reports whether it succeeds. " +
			              "Menus open on click only if this capture takes; run it with and without a " +
			              "floating window present to see whether extra windows break it.")]
		public static string MenuCaptureTestAction() => OnUi(w => w.MenuCaptureTest());

		[DevFlowAction("avd.menu.probe",
			Description = "Diagnostic: reports the main menu's items and tries to open one submenu " +
			              "programmatically (IsSubmenuOpen=true), capturing any exception. Separates " +
			              "'menu mode / popup cannot open at all' from 'the click never reaches it'.")]
		public static string MenuProbeAction(string header = "Layout") => OnUi(w => w.MenuProbe(header));

		[DevFlowAction("avd.menu.items",
			Description = "List the main menu: without a path the top-level headers, with a 'Layout/Save' " +
			              "path the children of that submenu (header, enabled, checkable, checked, command)")]
		public static string MenuItemsAction(string path = null) => OnUi(w => w.MenuItems(path));

		[DevFlowAction("avd.menu.invoke",
			Description = "Invoke a menu item by header path, e.g. 'Layout/Save/Layout_1' or 'Tools/New floating window'")]
		public static string InvokeMenuItemAction(string path) => OnUi(w => w.InvokeMenuItem(path));

		[DevFlowAction("avd.context.invoke", Description = "Invoke an entry of a tab's context menu by header, e.g. 'Window' on an anchorable tab")]
		public static string InvokeTabContextMenuItemAction(string title, string header, string kind = "anchorable") => OnUi(w => w.InvokeTabContextMenuItem(title, header, kind));

		[DevFlowAction("avd.menu.close", Description = "Close every open menu and context menu so the next gesture is not swallowed")]
		public static string CloseMenusAction() => OnUi(w => w.CloseMenus());

		[DevFlowAction("avd.tab.right-click",
			Description = "Raise the tab's right-click event (document or anchorable) and report whether a context menu opened")]
		public static string RightClickTabAction(string title, string kind = "document") => OnUi(w => w.RightClickTab(title, kind));

		[DevFlowAction("avd.query.window", Description = "Query the main window's position, size and state")]
		public static string QueryWindowAction() => OnUi(w => w.QueryWindow());

		[DevFlowAction("avd.window.resize", Description = "Resize the main window without moving it, as dragging its edge does")]
		public static string ResizeWindowAction(double width, double height) => OnUi(w => w.ResizeWindow(width, height));

		[DevFlowAction("avd.dialogs.answer",
			Description = "Answer the sample's confirmation dialogs from the test (yes/no) instead of showing a modal one; 'ask' restores the dialog")]
		public static string AnswerDialogsAction(string answer) => OnUi(w => w.AnswerDialogs(answer));

		[DevFlowAction("avd.document.add-two",
			Description = "Raise Click on Document 1's 'Click to add 2 documents' button (adds Test1 and Test2)")]
		public static string AddTwoDocumentsAction() => OnUi(w => w.AddTwoDocuments());

		[DevFlowAction("avd.document.activate", Description = "Make a document active by content id or title")]
		public static string ActivateDocumentAction(string contentIdOrTitle) => OnUi(w => w.ActivateDocument(contentIdOrTitle));

		[DevFlowAction("avd.dialogs.query", Description = "Report the confirmations the sample has asked for since the last avd.dialogs.answer")]
		public static string QueryDialogsAction() => OnUi(w => w.QueryDialogs());

		[DevFlowAction("avd.close-floating", Description = "Close a floating window through its own close lifecycle, as a window manager's close button does")]
		public static string CloseFloatingWindowAction(string contentId) => OnUi(w => w.CloseFloatingWindow(contentId));

		[DevFlowAction("avd.content", Description = "Report the TextBox texts, TextBlocks and Button labels inside a document or anchorable")]
		public static string ContentReportAction(string contentIdOrTitle) => OnUi(w => w.ContentReport(contentIdOrTitle));

		[DevFlowAction("avd.query.autohide", Description = "Query the auto-hide flyout: which contents it hosts and whether the named one is open")]
		public static string QueryAutoHideAction(string contentIdOrTitle = null) => OnUi(w => w.QueryAutoHide(contentIdOrTitle));

		[DevFlowAction("avd.autohide.dismiss", Description = "Close the auto-hide flyout, which overlays the panes and takes clicks aimed at them")]
		public static string DismissAutoHideFlyoutAction() => OnUi(w => w.DismissAutoHideFlyout());

		[DevFlowAction("avd.winforms.tree", Description = "Report the hosted WinForms control tree: text, tab index, focus and geometry in WinForms pixels")]
		public static string QueryWinFormsTreeAction() => OnUi(w => w.QueryWinFormsTree());

		[DevFlowAction("avd.winforms.bounds", Description = "Screen rectangle of one hosted WinForms control, in the same space the agent's clicks use")]
		public static string QueryWinFormsBoundsAction(string name) => OnUi(w => w.QueryWinFormsBounds(name));

		[DevFlowAction("avd.winforms.scale", Description = "Apply a layout scale to the interop host, which changes the same factor drawing and pointer conversion use")]
		public static string SetWinFormsScaleAction(double scaleX, double scaleY) => OnUi(w => w.SetWinFormsScale(scaleX, scaleY));

		[DevFlowAction("avd.winforms.mnemonic", Description = "Offer a mnemonic character to the hosted controls through the WinForms system-character path; unsupported on real WPF")]
		public static string InvokeWinFormsMnemonicAction(string letter) => OnUi(w => w.InvokeWinFormsMnemonic(letter));

		[DevFlowAction("avd.winforms.paint", Description = "Report what the host draws for the hosted tree under this renderer: the images in its retained drawing, their portable surface size and the renderer mode")]
		public static string QueryWinFormsPaintAction() => OnUi(w => w.QueryWinFormsPaint());



		[DevFlowAction("avd.focus.query", Description = "Report the focused element, its type and the document/anchorable it belongs to")]
		public static string QueryFocusAction() => OnUi(w => w.QueryFocus());

		[DevFlowAction("avd.pane.set-width", Description = "Set the dock width of the container that sizes a content's pane (its pane group, or the pane itself when it has no group), so narrow panes (Tool Window 1 is declared 50 wide) have tabs that can be clicked")]
		public static string SetPaneGroupWidthAction(string contentId, double width) => OnUi(w => w.SetPaneGroupWidth(contentId, width));

		[DevFlowAction("avd.focus.set", Description = "Focus the first TextBox of a document or anchorable, as clicking into its content does")]
		public static string SetFocusAction(string contentIdOrTitle) => OnUi(w => w.SetFocus(contentIdOrTitle));

		[DevFlowAction("avd.input.key",
			Description = "Raise a key press (Key name, e.g. 'Up', 'Tab') through routed input on the focused element; modifiers are reported but not injected")]
		public static string RaiseKeyAction(string key, string modifiers = null) => OnUi(w => w.RaiseKey(key, modifiers));

		[DevFlowAction("avd.input.text", Description = "Raise text input on the focused element, as typing into it does")]
		public static string RaiseTextInputAction(string text) => OnUi(w => w.RaiseTextInput(text));

		[DevFlowAction("avd.transparency.probe",
			Description = "Diagnostic A/B: opens two plain top-level windows next to each other - one " +
			              "with AllowsTransparency=true/WindowStyle=None, one opaque - each containing a " +
			              "solid red block. Isolates 'does the backend render transparent windows at all' " +
			              "from anything AvalonDock-specific. Pass false to close them again.")]
		public static string TransparencyProbeAction(bool show = true) => OnUi(w => w.TransparencyProbe(show));

		[DevFlowAction("avd.overlay.diagnostics",
			Description = "Diagnostic: show the drop-target compass persistently WITHOUT a drag, by " +
			              "driving IOverlayWindowHost.ShowOverlayWindow + DragEnter directly. Lets " +
			              "overlay-window RENDERING be judged independently of drag/hit-test logic. " +
			              "Requires a floating window to exist (call avd.float first). Returns the " +
			              "overlay's geometry and visibility state.")]
		public static string ShowOverlayDiagnosticsAction(bool show = true) => OnUi(w => w.ShowOverlayDiagnostics(show));

		[DevFlowAction("avd.query.platform", Description = "Query LibreWPF platform coordinate diagnostics")]
		public static string QueryPlatformDiagnosticsAction() => OnUi(w => w.QueryPlatformDiagnostics());

		[DevFlowAction("avd.query.active-drop-targets",
			Description = "During an active drag (a floating window whose caption is currently being " +
			              "dragged), returns every currently-visible compass drop-target indicator as " +
			              "{type, x, y, width, height, centerX, centerY} - type is a DropTargetType name " +
			              "(e.g. DockingManagerDockLeft, AnchorablePaneDockInside). Which indicators are " +
			              "visible depends on which host area the pointer is currently over, matching " +
			              "real AvalonDock drag behavior, so move the pointer near the desired target's " +
			              "pane/edge before calling this.")]
		public static string QueryActiveDropTargetsAction() => OnUi(w => w.QueryActiveDropTargets());

		[DevFlowAction("avd.query.logical-children", Description = "Count the DockingManager's live logical children")]
		public static string QueryLogicalChildrenAction() => OnUi(w => w.QueryLogicalChildren());

		[DevFlowAction("avd.query.drag-state", Description = "Query live floating-window drag state")]
		public static string QueryDragStateAction() => OnUi(w => w.QueryDragState());

		[DevFlowAction("avd.query.cursor", Description = "Query the native cursor position in screen coordinates")]
		public static string QueryNativeCursorAction() => OnUi(w => w.QueryNativeCursor());

		[DevFlowAction("avd.complete-current-drop", Description = "Complete the current held AvalonDock drag after its preview has been inspected")]
		public static string CompleteCurrentDropAction(string contentId) => OnUi(w => w.CompleteCurrentDrop(contentId));

		[DevFlowAction("avd.hit-test", Description = "Hit test a screen point against the AvalonDock manager")]
		public static string HitTestAction(double screenX, double screenY) => OnUi(w => w.HitTest(screenX, screenY));

		[DevFlowAction("avd.query.tabs", Description = "Query visible anchorable tabs and diagnostic bounds")]
		public static string QueryAnchorableTabsAction() => OnUi(w => w.QueryAnchorableTabs());

			[DevFlowAction("avd.query.anchorable-drag-surfaces", Description = "Query visible anchorable title/control drag surface diagnostics")]
			public static string QueryAnchorableDragSurfacesAction() => OnUi(w => w.QueryAnchorableDragSurfaces());

		[DevFlowAction("avd.new-floating", Description = "Create a new floating anchorable window")]
		public static string NewFloatingWindowAction(string title = null) => OnUi(w => w.NewFloatingWindow(title));

		[DevFlowAction("avd.close-content", Description = "Close a document or anchorable by content id or title, honouring CanClose")]
		public static string CloseContentAction(string contentIdOrTitle) => OnUi(w => w.CloseContent(contentIdOrTitle));
	}
}
