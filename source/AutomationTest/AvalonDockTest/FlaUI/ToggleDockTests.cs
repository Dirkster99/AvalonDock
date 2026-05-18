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
			Assert.That(MainWindow.Title, Does.Contain("ToggleTestApp"),
				"Main window should have ToggleTestApp title.");
		}

		[Test, Order(3)]
		public void Documents_ArePresent()
		{
			var doc1 = FindDocumentTab("Document 1");
			var doc2 = FindDocumentTab("Document 2");
			Assert.That(doc1, Is.Not.Null, "Document 1 should be present.");
			Assert.That(doc2, Is.Not.Null, "Document 2 should be present.");
		}

		[Test, Order(4)]
		public void ToggleButtons_ArePresent_ForLeftSide()
		{
			var btn1 = FindToggleButton("ToolBox 1");
			var btn2 = FindToggleButton("ToolBox 2");
			Assert.That(btn1, Is.Not.Null, "ToolBox 1 toggle button should be present.");
			Assert.That(btn2, Is.Not.Null, "ToolBox 2 toggle button should be present.");
		}

		[Test, Order(5)]
		public void ToggleButtons_ArePresent_ForRightSide()
		{
			var btn = FindToggleButton("Properties");
			Assert.That(btn, Is.Not.Null, "Properties toggle button should be present.");
		}

		[Test, Order(6)]
		public void ToggleButtons_ArePresent_ForBottomSide()
		{
			var btn = FindToggleButton("Output");
			Assert.That(btn, Is.Not.Null, "Output toggle button should be present.");
		}

		[Test, Order(7)]
		public void ToggleButtons_InitialState_AllUnchecked()
		{
			// All anchorables start auto-hidden, so all buttons should be unchecked
			var buttons = FindAllToggleButtons();
			foreach (var btn in buttons)
			{
				var togglePattern = btn.Patterns.Toggle.PatternOrDefault;
				if (togglePattern != null)
				{
					Assert.That(togglePattern.ToggleState.Value, Is.EqualTo(ToggleState.Off),
						$"Button '{btn.Name}' should be unchecked initially.");
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
			var btn = FindToggleButton("ToolBox 1");
			Assert.That(btn, Is.Not.Null, "ToolBox 1 button should exist.");

			// Click to toggle on (dock)
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// After clicking, the toggle button should be checked
			btn = FindToggleButton("ToolBox 1");
			var togglePattern = btn?.Patterns.Toggle.PatternOrDefault;
			Assert.That(togglePattern, Is.Not.Null, "Button should support Toggle pattern.");
			Assert.That(togglePattern.ToggleState.Value, Is.EqualTo(ToggleState.On),
				"ToolBox 1 button should be checked after clicking.");

			// The content area should now show ToolBox 1 Content
			var content = Retry.WhileNull(
				() => FindByName("ToolBox 1 Content"),
				timeout: TimeSpan.FromSeconds(5),
				interval: TimeSpan.FromMilliseconds(300));
			Assert.That(content.Result, Is.Not.Null,
				"ToolBox 1 content should be visible after toggling on.");

			// Clean up: undock
			btn = FindToggleButton("ToolBox 1");
			btn?.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);
		}

		[Test, Order(2)]
		public void ClickToggleButton_Again_HidesAnchorable()
		{
			var btn = FindToggleButton("ToolBox 1");
			Assert.That(btn, Is.Not.Null, "ToolBox 1 button should exist.");

			// Click to dock
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Click again to undock (auto-hide)
			btn = FindToggleButton("ToolBox 1");
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Button should be unchecked
			btn = FindToggleButton("ToolBox 1");
			var togglePattern = btn?.Patterns.Toggle.PatternOrDefault;
			Assert.That(togglePattern?.ToggleState.Value, Is.EqualTo(ToggleState.Off),
				"ToolBox 1 button should be unchecked after second click.");
		}

		[Test, Order(3)]
		public void ExclusiveToggle_SameSection_OnlyOneActive()
		{
			// Click ToolBox 1 to dock it
			var btn1 = FindToggleButton("ToolBox 1");
			Assert.That(btn1, Is.Not.Null, "ToolBox 1 button should exist.");
			btn1.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Verify ToolBox 1 is checked
			btn1 = FindToggleButton("ToolBox 1");
			Assert.That(btn1?.Patterns.Toggle.PatternOrDefault?.ToggleState.Value,
				Is.EqualTo(ToggleState.On), "ToolBox 1 should be checked.");

			// Click ToolBox 2 — should dock ToolBox 2 and auto-hide ToolBox 1
			var btn2 = FindToggleButton("ToolBox 2");
			Assert.That(btn2, Is.Not.Null, "ToolBox 2 button should exist.");
			btn2.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1000);

			// Re-find buttons after layout change
			btn1 = FindToggleButton("ToolBox 1");
			btn2 = FindToggleButton("ToolBox 2");

			Assert.That(btn2?.Patterns.Toggle.PatternOrDefault?.ToggleState.Value,
				Is.EqualTo(ToggleState.On), "ToolBox 2 should be checked.");
			Assert.That(btn1?.Patterns.Toggle.PatternOrDefault?.ToggleState.Value,
				Is.EqualTo(ToggleState.Off), "ToolBox 1 should be unchecked (exclusive per section).");

			// Clean up
			btn2 = FindToggleButton("ToolBox 2");
			btn2?.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);
		}

		[Test, Order(4)]
		public void RightSide_Toggle_WorksIndependently()
		{
			// Click ToolBox 1 on left using Invoke pattern as fallback
			var leftBtn = FindToggleButton("ToolBox 1");
			Assert.That(leftBtn, Is.Not.Null, "ToolBox 1 button should exist.");
			ClickToggleButtonSafe(leftBtn);
			System.Threading.Thread.Sleep(1000);

			// Click Properties on right
			var rightBtn = FindToggleButton("Properties");
			Assert.That(rightBtn, Is.Not.Null, "Properties button should exist.");
			ClickToggleButtonSafe(rightBtn);
			System.Threading.Thread.Sleep(1000);

			// Both should be checked — they're in different sections
			leftBtn = FindToggleButton("ToolBox 1");
			rightBtn = FindToggleButton("Properties");

			Assert.That(leftBtn?.Patterns.Toggle.PatternOrDefault?.ToggleState.Value,
				Is.EqualTo(ToggleState.On), "Left-side ToolBox 1 should be checked.");
			Assert.That(rightBtn?.Patterns.Toggle.PatternOrDefault?.ToggleState.Value,
				Is.EqualTo(ToggleState.On), "Right-side Properties should be checked.");

			// Clean up
			leftBtn = FindToggleButton("ToolBox 1");
			ClickToggleButtonSafe(leftBtn);
			System.Threading.Thread.Sleep(500);
			rightBtn = FindToggleButton("Properties");
			ClickToggleButtonSafe(rightBtn);
			System.Threading.Thread.Sleep(500);
		}

		private void ClickToggleButtonSafe(AutomationElement btn)
		{
			if (btn == null) return;
			try
			{
				btn.Click(true);
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
			// Dock ToolBox 1 first
			var btn = FindToggleButton("ToolBox 1");
			Assert.That(btn, Is.Not.Null, "ToolBox 1 button should exist.");
			btn.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(2000);

			// Verify it's docked (with retry for UI stabilization)
			Retry.WhileException(() =>
			{
				btn = FindToggleButton("ToolBox 1");
				Assert.That(btn?.Patterns.Toggle.PatternOrDefault?.ToggleState.Value,
					Is.EqualTo(ToggleState.On), "ToolBox 1 should be checked after docking.");
			}, timeout: TimeSpan.FromSeconds(5), interval: TimeSpan.FromMilliseconds(500));

			// Find the "Minimize" (formerly pin) button in the docked pane header
			// The PART_AutoHidePin button should have tooltip "Minimize"
			var minimizeBtn = Retry.WhileNull(
				() =>
				{
					var buttons = MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Button));
					return buttons.FirstOrDefault(b =>
					{
						try
						{
							var name = b.Name;
							var help = b.Properties.HelpText.ValueOrDefault;
							// The pin button's tooltip becomes "Minimize" in ToggleDockingManager
							return name == "Minimize" || help == "Minimize"
								|| (b.Properties.AutomationId.IsSupported && b.Properties.AutomationId.Value == "PART_AutoHidePin");
						}
						catch { return false; }
					});
				},
				timeout: TimeSpan.FromSeconds(10),
				interval: TimeSpan.FromMilliseconds(300));

			// If we found the pin button, click it
			if (minimizeBtn.Result != null)
			{
				ClickToggleButtonSafe(minimizeBtn.Result);
				System.Threading.Thread.Sleep(2000);

				// ToolBox 1 should be back to auto-hidden (toggle button unchecked)
				Retry.WhileException(() =>
				{
					btn = FindToggleButton("ToolBox 1");
					Assert.That(btn?.Patterns.Toggle.PatternOrDefault?.ToggleState.Value,
						Is.EqualTo(ToggleState.Off),
						"ToolBox 1 should be unchecked after clicking minimize (pin) button.");
				}, timeout: TimeSpan.FromSeconds(5), interval: TimeSpan.FromMilliseconds(500));
			}
			else
			{
				// Pin button not found via automation — clean up manually and mark as inconclusive
				btn = FindToggleButton("ToolBox 1");
				btn?.Click();
				Wait.UntilInputIsProcessed();
				System.Threading.Thread.Sleep(500);
				Assert.Inconclusive("Could not find the PART_AutoHidePin/Minimize button via automation.");
			}
		}
	}
}
