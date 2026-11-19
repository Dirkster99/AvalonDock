// Static action surface of the sample; see MainWindow.DevFlowActions.cs in TestApp for why the
// DevFlow actions are static and marshal to the window's thread themselves.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Controls;
using AvalonDock.Layout;
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace ToggleTestApp
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

		[DevFlowAction("avd.query.toggle-bars", Description = "Query every sidebar toggle button: zone, title, content id, checked state and bounds")]
		public static string QueryToggleBarsAction() => OnUi(w => w.QueryToggleBars());

		[DevFlowAction("avd.query.anchorables", Description = "Query the layout's anchorables: title, content id, visibility, auto-hide state")]
		public static string QueryAnchorablesAction() => OnUi(w => w.QueryAnchorables());

		[DevFlowAction("avd.toggle.click", Description = "Press a sidebar toggle button (content id, else title) and report the resulting state")]
		public static string ClickToggleButtonAction(string contentIdOrTitle) => OnUi(w => w.ClickToggleButton(contentIdOrTitle));

		[DevFlowAction("avd.query.shortcuts", Description = "Query the window's key bindings: gesture, toolbox and whether the gesture is registered")]
		public static string QueryShortcutsAction() => OnUi(w => w.QueryShortcuts());

		[DevFlowAction("avd.shortcut.invoke", Description = "Execute the key binding registered for a gesture (e.g. 'Ctrl+Alt+E')")]
		public static string InvokeShortcutAction(string gesture) => OnUi(w => w.InvokeShortcut(gesture));

		[DevFlowAction("avd.layout.reset", Description = "Delete the saved layout files and close every sidebar section")]
		public static string ResetLayoutAction() => OnUi(w => w.ResetLayout());

		[DevFlowAction("avd.layout.save", Description = "Save the current layout to the app's layout file")]
		public static string SaveLayoutToFileAction() => OnUi(w => w.SaveLayoutToFile());

		[DevFlowAction("avd.layout.load", Description = "Load the app's saved layout file")]
		public static string LoadLayoutFromFileAction() => OnUi(w => w.LoadLayoutFromFile());

		[DevFlowAction("avd.layout.load-unknown", Description = "Load a saved layout that also carries a tool window unknown to this app")]
		public static string LoadLayoutWithUnknownToolboxAction() => OnUi(w => w.LoadLayoutWithUnknownToolbox());

		[DevFlowAction("avd.window-mode.query", Description = "Query the live View Mode menu entries for a toolbox")]
		public static string QueryWindowModeAction(string contentIdOrTitle) => OnUi(w => w.QueryWindowMode(contentIdOrTitle));

		[DevFlowAction("avd.window-mode.toggle", Description = "Select View Mode > Window; selecting it again docks the toolbox")]
		public static string ToggleWindowModeAction(string contentIdOrTitle) => OnUi(w => w.ToggleWindowMode(contentIdOrTitle));

		[DevFlowAction("avd.detached.query", Description = "Query standalone Window-mode windows owned by the docking manager")]
		public static string QueryDetachedWindowsAction() => OnUi(w => w.QueryDetachedWindows());

		[DevFlowAction("avd.detached.close", Description = "Close a standalone Window-mode window through its ordinary close path")]
		public static string CloseDetachedWindowAction(string contentIdOrTitle) => OnUi(w => w.CloseDetachedWindow(contentIdOrTitle));

		// This partial is omitted from AvalonDockCodeApp.csproj and compiled only by its LibreWPF
		// twin. Keeping both the agent entry points and their probe implementation here lets an
		// upstream product-only change avoid MainWindow.xaml.cs entirely.
		public string QueryWindowMode(string contentIdOrTitle)
		{
			var anchorable = FindDevFlowAnchorable(contentIdOrTitle);
			return JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["contentId"] = anchorable.ContentId,
				["items"] = new[]
				{
					new Dictionary<string, object>
					{
						["header"] = "Window",
						["isEnabled"] = dockManager.AllowDetachedWindows,
						["isChecked"] = dockManager.IsDetached(anchorable),
					},
				},
			});
		}

		public string ToggleWindowMode(string contentIdOrTitle)
		{
			var anchorable = FindDevFlowAnchorable(contentIdOrTitle);
			if (dockManager.IsDetached(anchorable))
				dockManager.ReattachAnchorable(anchorable);
			else
				dockManager.DetachAnchorableToWindow(anchorable);

			return QueryDetachedWindows();
		}

		public string QueryDetachedWindows()
		{
			var windows = Application.Current.Windows.OfType<DetachedAnchorableWindow>().ToArray();
			return JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["count"] = windows.Length,
				["windows"] = windows.Select(window => new Dictionary<string, object>
				{
					["contentId"] = window.Model.ContentId,
					["title"] = window.Title,
					["isVisible"] = window.IsVisible,
					["isActive"] = window.IsActive,
					["ownerIsNull"] = window.Owner == null,
					["showInTaskbar"] = window.ShowInTaskbar,
					["windowStyle"] = window.WindowStyle.ToString(),
					["resizeMode"] = window.ResizeMode.ToString(),
					["hasOptionsHeader"] = window.Content is Grid root
						&& root.Children.OfType<ToggleAnchorablePaneTitle>().Any(),
				}).ToArray(),
			});
		}

		public string CloseDetachedWindow(string contentIdOrTitle)
		{
			var anchorable = FindDevFlowAnchorable(contentIdOrTitle);
			Application.Current.Windows.OfType<DetachedAnchorableWindow>()
				.FirstOrDefault(candidate => candidate.Model == anchorable)?.Close();
			return QueryDetachedWindows();
		}

		private LayoutAnchorable FindDevFlowAnchorable(string contentIdOrTitle) =>
			dockManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(anchorable =>
				string.Equals(anchorable.ContentId, contentIdOrTitle, StringComparison.Ordinal)
				|| string.Equals(anchorable.Title, contentIdOrTitle, StringComparison.Ordinal))
			?? throw new InvalidOperationException($"No tool window matches '{contentIdOrTitle}'.");
	}
}
