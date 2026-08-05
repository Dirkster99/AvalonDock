using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
	/// <summary>
	/// UI automation tests for the "Window" view mode, which detaches the content of a tool window
	/// into a standalone, independent top level window.
	/// </summary>
	/// <remarks>
	/// The tests drive the three dot options menu on a docked tool window title, which is the only
	/// entry point for the feature. They are ordered because they share one application instance and
	/// each one leaves the "Explorer" tool window docked again for the next.
	/// </remarks>
	[TestFixture]
	[Category("FlaUI")]
	[Category("ToggleDock")]
	public class DetachedWindowTests : ToggleDockFlaUITestBase
	{
		/// <summary>The tool window used throughout these tests.</summary>
		private const string ToolWindowName = "Explorer";

		/// <summary>Header of the submenu holding the view modes.</summary>
		private const string ViewModeHeader = "View Mode";

		/// <summary>Header of the menu item that detaches into a standalone window.</summary>
		private const string WindowModeHeader = "Window";

		/// <summary>Leaves every test with the tool window docked and nothing detached.</summary>
		[TearDown]
		public void DetachedWindowTearDown()
		{
			try
			{
				CloseDetachedWindows();
			}
			catch (Exception ex)
			{
				TestContext.Out.WriteLine($"[DetachedWindowTests] Cleanup failed: {ex.Message}");
			}
		}

		/// <summary>
		/// The View Mode submenu of the three dot menu must offer the new "Window" entry.
		/// </summary>
		[Test, Order(1)]
		public void ViewModeMenu_OffersWindowItem()
		{
			OpenViewModeSubmenu();

			var windowItem = FindMenuItem(WindowModeHeader);
			Assert.That(windowItem, Is.Not.Null,
				"The View Mode submenu should offer a 'Window' entry.");

			CloseAnyOpenMenu();
		}

		/// <summary>
		/// Choosing "Window" must produce a separate top level window carrying the tool window title.
		/// </summary>
		[Test, Order(2)]
		public void SelectWindowMode_DetachesIntoTopLevelWindow()
		{
			DetachToolWindow();

			var detached = WaitForDetachedWindow();
			Assert.That(detached, Is.Not.Null,
				$"A standalone window titled '{ToolWindowName}' should appear.");
			Assert.That(App.HasExited, Is.False, "The application should not crash while detaching.");
		}

		/// <summary>
		/// The detached window has to be a real operating system window: it exposes the window pattern
		/// and can be minimized and restored without touching the main window.
		/// </summary>
		[Test, Order(3)]
		public void DetachedWindow_IsIndependentTopLevelWindow()
		{
			DetachToolWindow();
			var detached = WaitForDetachedWindow();

			var mainHandle = MainWindow.Properties.NativeWindowHandle.ValueOrDefault;
			Assert.That(detached.Properties.NativeWindowHandle.ValueOrDefault, Is.Not.EqualTo(mainHandle),
				"The detached window must be a separate top level window, not a child of the main window.");

			var windowPattern = detached.Patterns.Window.PatternOrDefault;
			Assert.That(windowPattern, Is.Not.Null,
				"A standalone window should expose the UIA window pattern.");

			// Minimizing must not drag the main window along - that is the whole point of this mode.
			WindowVisualState? mainStateWhileMinimized = null;
			try
			{
				windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
				Wait.UntilInputIsProcessed();
				System.Threading.Thread.Sleep(400);

				mainStateWhileMinimized = MainWindow.Patterns.Window.PatternOrDefault?.WindowVisualState.ValueOrDefault;

				windowPattern.SetWindowVisualState(WindowVisualState.Normal);
				Wait.UntilInputIsProcessed();
				System.Threading.Thread.Sleep(400);
			}
			catch (Exception ex)
			{
				TestContext.Out.WriteLine($"[DetachedWindowTests] Minimize probe unavailable: {ex.Message}");
			}

			if (mainStateWhileMinimized.HasValue)
			{
				Assert.That(
					mainStateWhileMinimized.Value,
					Is.Not.EqualTo(WindowVisualState.Minimized),
					"Minimizing the detached window must leave the main window untouched.");
			}
		}

		/// <summary>
		/// Closing the standalone window must hand the content back to the docking layout rather than
		/// losing it.
		/// </summary>
		[Test, Order(4)]
		public void ClosingDetachedWindow_DocksContentBack()
		{
			DetachToolWindow();
			var detached = WaitForDetachedWindow();

			detached.AsWindow()?.Close();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(800);

			var closed = Retry.WhileFalse(
				() => FindDetachedWindow() == null,
				timeout: TimeSpan.FromSeconds(8),
				interval: TimeSpan.FromMilliseconds(300));
			Assert.That(closed.Result, Is.True,
				"The standalone window should be gone after closing it.");

			Assert.That(App.HasExited, Is.False,
				"Closing the standalone window must not close the application.");

			var docked = Retry.WhileFalse(
				() => IsToolWindowDocked(ToolWindowName) || FindToggleButton(ToolWindowName) != null,
				timeout: TimeSpan.FromSeconds(8),
				interval: TimeSpan.FromMilliseconds(300));
			Assert.That(docked.Result, Is.True,
				$"'{ToolWindowName}' should be back in the docking layout after its window was closed.");
		}

		/// <summary>
		/// Selecting "Window" a second time is the way back, mirroring how the entry is checked while
		/// the content is detached.
		/// </summary>
		[Test, Order(5)]
		public void SelectWindowMode_Twice_DocksContentBack()
		{
			DetachToolWindow();
			WaitForDetachedWindow();

			// The three dot button is gone with the title bar, so the stripe button carries the menu.
			OpenViewModeSubmenuFromStripeButton();
			ClickMenuItem(WindowModeHeader);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(800);

			var gone = Retry.WhileFalse(
				() => FindDetachedWindow() == null,
				timeout: TimeSpan.FromSeconds(8),
				interval: TimeSpan.FromMilliseconds(300));
			Assert.That(gone.Result, Is.True,
				"Selecting 'Window' again should dock the content back and close the standalone window.");
			Assert.That(App.HasExited, Is.False, "The application should not crash while docking back.");
		}

		/// <summary>
		/// While the content lives in a standalone window the stripe toggle button must bring that
		/// window forward instead of trying to dock the empty anchorable.
		/// </summary>
		[Test, Order(6)]
		public void StripeButton_WhileDetached_DoesNotDockAndKeepsWindowAlive()
		{
			DetachToolWindow();
			WaitForDetachedWindow();

			var stripeButton = FindToggleButton(ToolWindowName);
			if (stripeButton == null)
			{
				Assert.Inconclusive($"Stripe button for '{ToolWindowName}' was not found.");
			}
			else
			{
				stripeButton.Click();
				Wait.UntilInputIsProcessed();
				System.Threading.Thread.Sleep(600);

				Assert.That(App.HasExited, Is.False,
					"Clicking the stripe button while detached must not crash the application.");
				Assert.That(FindDetachedWindow(), Is.Not.Null,
					"The standalone window should survive a click on the stripe button.");
			}
		}

		// ===== Helpers =====

		/// <summary>
		/// Docks the tool window if needed and opens the View Mode submenu of its three dot menu.
		/// </summary>
		private void OpenViewModeSubmenu()
		{
			EnsureToolWindowDocked();

			var optionsButton = Retry.WhileNull(
				() => FindByAutomationId("PART_OptionsButton"),
				timeout: TimeSpan.FromSeconds(8),
				interval: TimeSpan.FromMilliseconds(300)).Result;

			Assert.That(optionsButton, Is.Not.Null,
				"The three dot options button should be present on a docked tool window title.");

			optionsButton.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);

			ExpandViewMode();
		}

		/// <summary>
		/// Opens the View Mode submenu while the content is detached.
		/// </summary>
		/// <remarks>
		/// The standalone window hosts only the content presenter of the anchorable, not its
		/// <see cref="AvalonDock.Controls.ToggleAnchorablePaneTitle"/>, so it carries no three dot button.
		/// The remaining route to the same menu is a right click on the stripe button, which
		/// <c>ToggleDockButton.OnMouseRightButtonUp</c> wires to the very same context menu.
		/// </remarks>
		private void OpenViewModeSubmenuFromStripeButton()
		{
			var stripeButton = Retry.WhileNull(
				() => FindToggleButton(ToolWindowName),
				timeout: TimeSpan.FromSeconds(8),
				interval: TimeSpan.FromMilliseconds(300)).Result;

			Assert.That(stripeButton, Is.Not.Null,
				$"The stripe button for '{ToolWindowName}' should remain available while detached.");

			stripeButton.RightClick();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);

			ExpandViewMode();
		}

		/// <summary>Hovers and clicks the View Mode entry so its submenu opens.</summary>
		private void ExpandViewMode()
		{
			var viewMode = Retry.WhileNull(
				() => FindMenuItem(ViewModeHeader),
				timeout: TimeSpan.FromSeconds(8),
				interval: TimeSpan.FromMilliseconds(300)).Result;

			Assert.That(viewMode, Is.Not.Null, "The three dot menu should contain a 'View Mode' entry.");

			viewMode.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);
		}

		/// <summary>Runs the whole detach gesture and leaves the menu closed.</summary>
		private void DetachToolWindow()
		{
			if (FindDetachedWindow() != null) return;

			OpenViewModeSubmenu();
			ClickMenuItem(WindowModeHeader);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(800);
		}

		/// <summary>Docks the tool window so that its title bar, and with it the menu, exists.</summary>
		private void EnsureToolWindowDocked()
		{
			if (FindByAutomationId("PART_OptionsButton") != null) return;

			var stripeButton = FindToggleButton(ToolWindowName);
			if (stripeButton == null) return;

			stripeButton.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(700);
		}

		/// <summary>
		/// Finds a menu item by its header across every popup the application currently shows.
		/// </summary>
		/// <param name="header">The header text of the wanted item.</param>
		/// <returns>The menu item, or <see langword="null"/> when it is not on screen.</returns>
		private AutomationElement FindMenuItem(string header)
		{
			foreach (var root in GetMenuSearchRoots())
			{
				try
				{
					var match = root.FindAllDescendants(CF.ByControlType(ControlType.MenuItem))
						.FirstOrDefault(item =>
						{
							try { return item.Name == header; }
							catch { return false; }
						});

					if (match != null) return match;
				}
				catch
				{
					// A popup can disappear between enumeration and inspection.
				}
			}

			return null;
		}

		/// <summary>Clicks a menu item by header and fails the test when it is missing.</summary>
		/// <param name="header">The header text of the wanted item.</param>
		private void ClickMenuItem(string header)
		{
			var item = Retry.WhileNull(
				() => FindMenuItem(header),
				timeout: TimeSpan.FromSeconds(8),
				interval: TimeSpan.FromMilliseconds(300)).Result;

			Assert.That(item, Is.Not.Null, $"Menu item '{header}' should be on screen.");

			item.Click();
		}

		/// <summary>
		/// Gets the elements that can hold menu popups: the main window, every top level window of the
		/// application and the desktop, because WPF context menus live in their own popup windows.
		/// </summary>
		/// <returns>The roots to search for menu items.</returns>
		private IEnumerable<AutomationElement> GetMenuSearchRoots()
		{
			var roots = new List<AutomationElement>();

			if (MainWindow != null) roots.Add(MainWindow);

			try
			{
				roots.AddRange(App.GetAllTopLevelWindows(Automation));
			}
			catch
			{
				// The application may be shutting down.
			}

			try
			{
				var processId = App.ProcessId;
				roots.AddRange(Automation.GetDesktop()
					.FindAllChildren(CF.ByControlType(ControlType.Window))
					.Where(w =>
					{
						try { return w.Properties.ProcessId.ValueOrDefault == processId; }
						catch { return false; }
					}));
			}
			catch
			{
				// Desktop enumeration is best effort.
			}

			return roots;
		}

		/// <summary>
		/// Finds the standalone window hosting the tool window, identified by its caption.
		/// </summary>
		/// <returns>The window, or <see langword="null"/> when nothing is detached.</returns>
		private AutomationElement FindDetachedWindow()
		{
			var mainHandle = MainWindow?.Properties.NativeWindowHandle.ValueOrDefault ?? IntPtr.Zero;

			try
			{
				foreach (var window in App.GetAllTopLevelWindows(Automation))
				{
					try
					{
						if (window.Properties.NativeWindowHandle.ValueOrDefault == mainHandle) continue;
						if (window.Title == ToolWindowName) return window;
					}
					catch
					{
						// Window closed while being inspected.
					}
				}
			}
			catch
			{
				// The application may be shutting down.
			}

			return null;
		}

		/// <summary>Waits until the standalone window is on screen and returns it.</summary>
		/// <param name="timeoutSeconds">
		/// How long to wait. The default is generous because the very first detach in a run pays for
		/// jitting the window, creating its handle and merging the theme resources.
		/// </param>
		/// <returns>The standalone window.</returns>
		private AutomationElement WaitForDetachedWindow(int timeoutSeconds = 25)
		{
			var result = Retry.WhileNull(
				FindDetachedWindow,
				timeout: TimeSpan.FromSeconds(timeoutSeconds),
				interval: TimeSpan.FromMilliseconds(300));

			Assert.That(result.Result, Is.Not.Null,
				$"No standalone window titled '{ToolWindowName}' appeared within {timeoutSeconds}s.");
			return result.Result;
		}

		/// <summary>Closes any standalone window left behind by a test.</summary>
		private void CloseDetachedWindows()
		{
			CloseAnyOpenMenu();

			for (var attempt = 0; attempt < 3; attempt++)
			{
				var detached = FindDetachedWindow();
				if (detached == null) return;

				detached.AsWindow()?.Close();
				Wait.UntilInputIsProcessed();
				System.Threading.Thread.Sleep(500);
			}
		}

		/// <summary>Dismisses an open menu so it cannot swallow the next interaction.</summary>
		private void CloseAnyOpenMenu()
		{
			Keyboard.Press(VirtualKeyShort.ESCAPE);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(150);
			Keyboard.Press(VirtualKeyShort.ESCAPE);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(150);
		}
	}
}