using System;
using System.IO;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
	/// <summary>
	/// Base class for FlaUI tests that target the AvalonDockCodeApp.
	/// Launches AvalonDockCodeApp.exe before each test fixture.
	/// </summary>
	[TestFixture]
	[Category("FlaUI")]
	[Category("ToggleDock")]
	public abstract class ToggleDockFlaUITestBase
	{
		private const int MainWindowTimeoutSeconds = 30;

		protected Application App { get; private set; }
		protected UIA3Automation Automation { get; private set; }
		protected Window MainWindow { get; private set; }
		protected ConditionFactory CF { get; private set; }

		[OneTimeSetUp]
		public void BaseOneTimeSetUp()
		{
			var appPath = ResolveToggleTestAppPath();
			TestContext.Out.WriteLine($"[ToggleDockFlaUITestBase] Launching: {appPath}");

			Automation = new UIA3Automation();
			CF = new ConditionFactory(Automation.PropertyLibrary);
			App = Application.Launch(appPath);

			MainWindow = WaitForMainWindow();
			Assert.That(MainWindow, Is.Not.Null,
				"Main window did not appear within the timeout period.");

			MainWindow.SetForeground();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);

			WaitForAppReady();
			TestContext.Out.WriteLine("[ToggleDockFlaUITestBase] Application is ready.");
		}

		[OneTimeTearDown]
		public void BaseOneTimeTearDown()
		{
			if (App == null || App.HasExited)
			{
				Automation?.Dispose();
				return;
			}

			App.Close();

			for (int i = 0; i < 5; i++)
			{
				System.Threading.Thread.Sleep(500);
				if (App.HasExited) break;
			}

			if (!App.HasExited)
			{
				var closed = Retry.WhileFalse(
					() => App.HasExited,
					timeout: TimeSpan.FromSeconds(10),
					interval: TimeSpan.FromMilliseconds(500));
				if (!closed.Result) App.Kill();
			}

			Automation?.Dispose();
		}

		[SetUp]
		public void BaseSetUp()
		{
			if (App != null && !App.HasExited && MainWindow != null)
			{
				MainWindow.SetForeground();
				Wait.UntilInputIsProcessed();
				System.Threading.Thread.Sleep(200);
			}
		}

		[TearDown]
		public void BaseTearDown()
		{
			Keyboard.Press(VirtualKeyShort.ESCAPE);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(100);
		}

		// ===== Element Finders =====

		protected AutomationElement FindByAutomationId(string automationId)
			=> MainWindow.FindFirstDescendant(CF.ByAutomationId(automationId));

		protected AutomationElement FindByName(string name)
			=> MainWindow.FindFirstDescendant(CF.ByName(name));

		protected AutomationElement[] FindAllByName(string name)
			=> MainWindow.FindAllDescendants(CF.ByName(name));

		protected AutomationElement WaitForName(string name, int timeoutSeconds = 10)
		{
			var result = Retry.WhileNull(
				() => FindByName(name),
				timeout: TimeSpan.FromSeconds(timeoutSeconds),
				interval: TimeSpan.FromMilliseconds(200));
			Assert.That(result.Result, Is.Not.Null,
				$"Element '{name}' not found within {timeoutSeconds}s.");
			return result.Result;
		}

		/// <summary>
		/// Finds all toggle buttons by searching within the ToggleDockButtonBar containers.
		/// UIA wraps ToggleDockButton items as DataItem elements; the actual
		/// ToggleButton with Toggle pattern is a descendant of each DataItem.
		/// </summary>
		protected AutomationElement[] FindAllToggleButtons()
		{
			var barIds = new[]
			{
				"ToggleDockBar_LeftTop", "ToggleDockBar_LeftBottom",
				"ToggleDockBar_RightTop", "ToggleDockBar_RightBottom",
				"ToggleDockBar_BottomLeft", "ToggleDockBar_BottomRight"
			};

			var buttons = new System.Collections.Generic.List<AutomationElement>();
			foreach (var barId in barIds)
			{
				var bar = MainWindow.FindFirstDescendant(CF.ByAutomationId(barId));
				if (bar == null) continue;

				var dataItems = bar.FindAllDescendants()
					.Where(e => { try { return e.ControlType == ControlType.DataItem && !string.IsNullOrEmpty(e.Name); } catch { return false; } });

				foreach (var dataItem in dataItems)
				{
					// Look for the actual ToggleButton inside the DataItem
					AutomationElement toggleBtn = null;
					try
					{
						foreach (var child in dataItem.FindAllDescendants())
						{
							if (child.Patterns.Toggle.IsSupported)
							{
								toggleBtn = child;
								break;
							}
						}
					}
					catch { }

					// Use the ToggleButton if found, otherwise fall back to the DataItem
					buttons.Add(toggleBtn ?? dataItem);
				}
			}

			return buttons.ToArray();
		}

		/// <summary>
		/// Finds a toggle button by its name (matching the anchorable title).
		/// Searches DataItem elements by name, returns the inner ToggleButton if available.
		/// </summary>
		protected AutomationElement FindToggleButton(string name)
		{
			var barIds = new[]
			{
				"ToggleDockBar_LeftTop", "ToggleDockBar_LeftBottom",
				"ToggleDockBar_RightTop", "ToggleDockBar_RightBottom",
				"ToggleDockBar_BottomLeft", "ToggleDockBar_BottomRight"
			};

			foreach (var barId in barIds)
			{
				var bar = MainWindow.FindFirstDescendant(CF.ByAutomationId(barId));
				if (bar == null) continue;

				foreach (var item in bar.FindAllDescendants())
				{
					try
					{
						if (item.ControlType == ControlType.DataItem && item.Name == name)
						{
							// Return the inner ToggleButton if it exists
							foreach (var child in item.FindAllDescendants())
							{
								try { if (child.Patterns.Toggle.IsSupported) return child; }
								catch { }
							}

							return item;
						}
					}
					catch { }
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the toggle state of a toggle button element.
		/// Checks the element and its children for Toggle pattern,
		/// falls back to checking whether the tool window is docked.
		/// </summary>
		protected ToggleState GetToggleState(AutomationElement toggleBtn)
		{
			if (toggleBtn == null) return ToggleState.Off;

			var toggle = toggleBtn.Patterns.Toggle.PatternOrDefault;
			if (toggle != null) return toggle.ToggleState.Value;

			foreach (var child in toggleBtn.FindAllDescendants())
			{
				try
				{
					var childToggle = child.Patterns.Toggle.PatternOrDefault;
					if (childToggle != null) return childToggle.ToggleState.Value;
				}
				catch { }
			}

			return IsToolWindowDocked(toggleBtn.Name) ? ToggleState.On : ToggleState.Off;
		}

		/// <summary>
		/// Finds a document tab in the AvalonDock layout.
		/// </summary>
		protected AutomationElement FindDocumentTab(string documentName)
		{
			var deps = MainWindow.FindAllDescendants();
			foreach (var dep in deps)
			{
				try
				{
					var parentName = dep.Parent?.Properties.Name.IsSupported == true
						? dep.Parent?.Name
						: null;
					var depName = dep.Properties.Name.IsSupported ? dep.Name : null;

					if (parentName == "AvalonDock.Layout.LayoutDocument" &&
						depName != null &&
						documentName.Contains(depName.TrimStart('.')))
					{
						return dep.Parent;
					}
				}
				catch { }
			}
			return null;
		}

		/// <summary>
		/// Finds a tool window (LayoutAnchorable) by its title.
		/// </summary>
		protected AutomationElement FindToolWindowTab(string toolWindowName)
		{
			var elements = MainWindow.FindAllDescendants(CF.ByText(toolWindowName));
			foreach (var el in elements)
			{
				try
				{
					if (el.Parent?.Name == "AvalonDock.Layout.LayoutAnchorable")
						return el;
				}
				catch { }
			}
			return FindByName(toolWindowName);
		}

		protected bool IsToolWindowDocked(string toolWindowName)
		{
			try
			{
				var elements = MainWindow.FindAllDescendants(CF.ByText(toolWindowName));
				return elements.Any(el => el.Parent?.Name == "AvalonDock.Layout.LayoutAnchorable");
			}
			catch { return false; }
		}

		// ===== Infrastructure =====

		private string ResolveToggleTestAppPath()
		{
			var envPath = Environment.GetEnvironmentVariable("AVALONDOCK_TOGGLE_TESTAPP_PATH");
			if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath)) return envPath;

			var testDir = TestContext.CurrentContext.TestDirectory;
			var configs = new[] { "Debug", "Release" };
			var tfms = new[] { "net48", "net9.0-windows" };

			foreach (var config in configs)
			foreach (var tfm in tfms)
			{
				var candidate = Path.GetFullPath(Path.Combine(testDir,
					"..", "..", "..", "..", "..", "AvalonDockCodeApp", "bin", config, tfm, "AvalonDockCodeApp.exe"));
				if (File.Exists(candidate)) return candidate;
			}

			// Fallback search
			var dir = new DirectoryInfo(testDir);
			while (dir != null)
			{
				var matches = Directory.GetFiles(dir.FullName, "AvalonDockCodeApp.exe", SearchOption.AllDirectories);
				var match = matches.FirstOrDefault(m =>
					m.Contains(Path.DirectorySeparatorChar + "AvalonDockCodeApp" + Path.DirectorySeparatorChar) &&
					!m.Contains("AvalonDockTest"));
				if (match != null) return match;
				dir = dir.Parent;
				if (dir?.Name == "source" || dir?.Name == "AvalonDock") break;
			}

			Assert.Fail("AvalonDockCodeApp.exe not found. Build the solution first.");
			return null;
		}

		private Window WaitForMainWindow()
		{
			var result = Retry.WhileNull(
				() =>
				{
					try { return App.GetMainWindow(Automation, TimeSpan.FromSeconds(2)); }
					catch { return null; }
				},
				timeout: TimeSpan.FromSeconds(MainWindowTimeoutSeconds),
				interval: TimeSpan.FromSeconds(1));
			return result.Result;
		}

		private void WaitForAppReady()
		{
			// Wait for the main document to appear
			Retry.WhileNull(
				() => FindDocumentTab("Welcome") ?? FindByName("Layout"),
				timeout: TimeSpan.FromSeconds(15),
				interval: TimeSpan.FromMilliseconds(500));

			// Wait for toggle buttons to appear (they may arrive asynchronously via DockLayout binding)
			Retry.WhileTrue(
				() => FindAllToggleButtons().Length == 0,
				timeout: TimeSpan.FromSeconds(10),
				interval: TimeSpan.FromMilliseconds(500));

			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);
		}
	}
}
