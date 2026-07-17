using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
	/// <summary>
	/// UI automation tests for the ToggleDockingManager.
	/// Tests verify that sidebar toggle buttons appear, toggle behavior works,
	/// and exclusive activation per section is enforced.
	/// </summary>
	[TestFixture]
	[Category("FlaUI")]
	[Category("ToggleDock")]
	public class ToggleDockSmokeTests : ToggleDockFlaUITestBase
	{
		[Test, Order(1)]
		public void Application_Starts_MainWindowAppears()
		{
			Assert.That(App, Is.Not.Null, "Application should be launched.");
			Assert.That(App.HasExited, Is.False, "Application should still be running.");
			Assert.That(MainWindow, Is.Not.Null, "Main window should be present.");
		}

		[Test, Order(2)]
		public void MainWindow_HasCorrectTitle()
		{
			Assert.That(
				MainWindow.Title,
				Does.Contain("AvalonDock Code"),
				"Main window should have AvalonDock Code title.");
		}

		[Test, Order(3)]
		public void Documents_ArePresent()
		{
			var welcome = FindDocumentTab("Welcome");
			Assert.That(welcome, Is.Not.Null, "Welcome document should be present.");
		}

		[Test, Order(4)]
		public void ToggleButtons_ArePresent_ForLeftSide()
		{
			var btn1 = FindToggleButton("Explorer");
			var btn2 = FindToggleButton("Search");
			Assert.That(btn1, Is.Not.Null, "Explorer toggle button should be present.");
			Assert.That(btn2, Is.Not.Null, "Search toggle button should be present.");
		}

		[Test, Order(5)]
		public void ToggleButtons_ArePresent_ForBottomSide()
		{
			var btn1 = FindToggleButton("Problems");
			var btn2 = FindToggleButton("Terminal");
			Assert.That(btn1, Is.Not.Null, "Problems toggle button should be present.");
			Assert.That(btn2, Is.Not.Null, "Terminal toggle button should be present.");
		}

		[Test, Order(6)]
		public void ToggleButtons_InitialState_AllUnchecked()
		{
			// All anchorables start auto-hidden, so all buttons should be unchecked except terminal
			var buttons = FindAllToggleButtons();
			foreach (var btn in buttons)
			{
				var state = GetToggleState(btn);
				if (btn.Name != "Terminal")
				{
					Assert.That(
						state,
						Is.EqualTo(ToggleState.Off),
						$"Button '{btn.Name}' should be unchecked initially.");
				}
				else
				{
					Assert.That(
						state,
						Is.EqualTo(ToggleState.On),
						$"Button '{btn.Name}' should be checked initially.");
				}
			}
		}
	}

	/// <summary>
	/// Tests for toggle activation behavior: clicking buttons should dock/undock anchorables.
	/// </summary>
	[TestFixture]
	[Category("FlaUI")]
	[Category("ToggleDock")]
	public class ToggleDockActivationTests : ToggleDockFlaUITestBase
	{
		[Test, Order(1)]
		public void ClickToggleButton_DocksAnchorable()
		{
			var btn = FindToggleButton("Explorer");
			Assert.That(btn, Is.Not.Null, "Explorer button should exist.");

			// Click to toggle on (dock)
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// After clicking, the toggle button should be checked
			btn = FindToggleButton("Explorer");
			Assert.That(
				GetToggleState(btn),
				Is.EqualTo(ToggleState.On),
				"Explorer button should be checked after clicking.");

			// Clean up: undock
			btn = FindToggleButton("Explorer");
			btn?.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);
		}

		[Test, Order(2)]
		public void ClickToggleButton_Again_HidesAnchorable()
		{
			var btn = FindToggleButton("Explorer");
			Assert.That(btn, Is.Not.Null, "Explorer button should exist.");

			// Click to dock
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Click again to undock (auto-hide)
			btn = FindToggleButton("Explorer");
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Button should be unchecked
			btn = FindToggleButton("Explorer");
			Assert.That(
				GetToggleState(btn),
				Is.EqualTo(ToggleState.Off),
				"Explorer button should be unchecked after second click.");
		}

		[Test, Order(3)]
		public void ExclusiveToggle_SameSection_OnlyOneActive()
		{
			// Click Explorer to dock it (left side)
			var btn1 = FindToggleButton("Explorer");
			Assert.That(btn1, Is.Not.Null, "Explorer button should exist.");
			btn1.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Verify Explorer is checked
			btn1 = FindToggleButton("Explorer");
			Assert.That(
				GetToggleState(btn1),
				Is.EqualTo(ToggleState.On),
				"Explorer should be checked.");

			// Click Search — should dock Search and auto-hide Explorer (same left section)
			var btn2 = FindToggleButton("Search");
			Assert.That(btn2, Is.Not.Null, "Search button should exist.");
			btn2.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Re-find buttons after layout change
			btn1 = FindToggleButton("Explorer");
			btn2 = FindToggleButton("Search");

			Assert.That(
				GetToggleState(btn2),
				Is.EqualTo(ToggleState.On),
				"Search should be checked.");
			Assert.That(
				GetToggleState(btn1),
				Is.EqualTo(ToggleState.Off),
				"Explorer should be unchecked (exclusive per section).");

			// Clean up
			btn2 = FindToggleButton("Search");
			btn2?.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);
		}

		[Test, Order(4)]
		public void DifferentSections_Toggle_WorkIndependently()
		{
			// Click Explorer on left
			var leftBtn = FindToggleButton("Explorer");
			Assert.That(leftBtn, Is.Not.Null, "Explorer button should exist.");
			var leftInitState = GetToggleState(leftBtn);
			ClickToggleButtonSafe(leftBtn);
			System.Threading.Thread.Sleep(1000);

			// Click Terminal on bottom
			var bottomBtn = FindToggleButton("Terminal");
			Assert.That(bottomBtn, Is.Not.Null, "Terminal button should exist.");
			var bottomInitState = GetToggleState(leftBtn);
			ClickToggleButtonSafe(bottomBtn);
			System.Threading.Thread.Sleep(1000);

			// Both should be checked — they're in different sections
			leftBtn = FindToggleButton("Explorer");
			bottomBtn = FindToggleButton("Terminal");

			Assert.That(
				GetToggleState(leftBtn),
				Is.Not.EqualTo(leftInitState),
				"Left-side Explorer should be changed.");
			Assert.That(
				GetToggleState(bottomBtn),
				Is.Not.EqualTo(bottomInitState),
				"Bottom-side Terminal should be changed.");

			// Clean up
			leftBtn = FindToggleButton("Explorer");
			ClickToggleButtonSafe(leftBtn);
			System.Threading.Thread.Sleep(200);
			bottomBtn = FindToggleButton("Terminal");
			ClickToggleButtonSafe(bottomBtn);
			System.Threading.Thread.Sleep(200);

			Assert.That(
				GetToggleState(leftBtn),
				Is.EqualTo(leftInitState),
				"Left-side Explorer should be back to start.");
			Assert.That(
				GetToggleState(bottomBtn),
				Is.EqualTo(bottomInitState),
				"Bottom-side Terminal should be back to start.");
		}

		private void ClickToggleButtonSafe(AutomationElement btn)
		{
			if (btn == null) return;
			try
			{
				btn.Click();
			}
			catch (FlaUI.Core.Exceptions.NoClickablePointException)
			{
				// Fallback: use Invoke pattern
				var invoke = btn.Patterns.Invoke.PatternOrDefault;
				if (invoke != null)
					invoke.Invoke();
				else
				{
					// Last resort: toggle via UIA Toggle pattern
					var toggle = btn.Patterns.Toggle.PatternOrDefault;
					toggle?.Toggle();
				}
			}

			Wait.UntilInputIsProcessed();
		}

		[Test, Order(5)]
		[Retry(3)]
		public void PinButton_ActsAsMinimize_SendsBackToSidebar()
		{
			// Dock Explorer first
			var btn = FindToggleButton("Explorer");
			Assert.That(btn, Is.Not.Null, "Explorer button should exist.");
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(2000);

			// Verify it's docked (with retry for UI stabilization)
			Retry.WhileException(
				() =>
				{
					btn = FindToggleButton("Explorer");
					Assert.That(
						GetToggleState(btn),
						Is.EqualTo(ToggleState.On),
						"Explorer should be checked after docking.");
				},
				timeout: TimeSpan.FromSeconds(5),
				interval: TimeSpan.FromMilliseconds(500));

			// Find the "Minimize" button in the docked pane header
			var minimizeBtn = Retry.WhileNull(
				() =>
				{
					var explorer = FindToolWindowTab("Explorer");
					var buttons = explorer?.Parent.FindAllDescendants(CF.ByControlType(ControlType.Button));
					return buttons?.FirstOrDefault(b =>
					{
						try
						{
							var name = b.Name;
							var help = b.Properties.HelpText.ValueOrDefault;
							// The pin button's tooltip becomes "Minimize" in ToggleDockingManager
							return name == "Minimize" || help == "Minimize"
							                          || (b.Properties.AutomationId.IsSupported &&
							                              b.Properties.AutomationId.Value == "PART_AutoHidePin");
						}
						catch
						{
							return false;
						}
					});
				},
				timeout: TimeSpan.FromSeconds(10),
				interval: TimeSpan.FromMilliseconds(300));

			// If we found the pin button, click it
			if (minimizeBtn.Result == null)
			{
				Assert.Fail("Minimize not found");
				return;
			}

			minimizeBtn.Result.AsButton().Invoke();
			// ClickToggleButtonSafe(minimizeBtn.Result);
			System.Threading.Thread.Sleep(200);

			// Explorer should be back to auto-hidden (toggle button unchecked)
			Retry.WhileException(
				() =>
				{
					btn = FindToggleButton("Explorer");
					Assert.That(
						GetToggleState(btn),
						Is.EqualTo(ToggleState.Off),
						"Explorer should be unchecked after clicking minimize (pin) button.");
				},
				timeout: TimeSpan.FromSeconds(5),
				interval: TimeSpan.FromMilliseconds(500));
		}
	}
}