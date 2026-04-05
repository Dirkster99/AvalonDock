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
	/// Base class for all FlaUI-based AvalonDock UI automation tests.
	/// Launches the TestApp before each test fixture and shuts it down afterwards.
	/// Element discovery follows the same patterns as Alpine.UITests.Wpf.TestBase:
	/// AvalonDock exposes LayoutDocument parents with Name=="AvalonDock.Layout.LayoutDocument"
	/// and LayoutAnchorable parents with Name=="AvalonDock.Layout.LayoutAnchorable".
	/// </summary>
	[TestFixture]
	[Category("FlaUI")]
	public abstract class FlaUITestBase
	{
		private const int MainWindowTimeoutSeconds = 30;

		protected Application App { get; private set; }
		protected UIA3Automation Automation { get; private set; }
		protected Window MainWindow { get; private set; }
		protected ConditionFactory CF { get; private set; }

		[OneTimeSetUp]
		public void BaseOneTimeSetUp()
		{
			var appPath = ResolveTestAppPath();
			TestContext.Out.WriteLine($"[FlaUITestBase] Launching: {appPath}");

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
			TestContext.Out.WriteLine("[FlaUITestBase] Application is ready.");
		}

		[OneTimeTearDown]
		public void BaseOneTimeTearDown()
		{
			if (App == null || App.HasExited)
			{
				Automation?.Dispose();
				return;
			}

			DismissAnyDialogIfPresent("No", "Nein", "Yes", "OK", "Close");
			App.Close();

			for (int i = 0; i < 5; i++)
			{
				System.Threading.Thread.Sleep(500);
				if (App.HasExited) break;
				DismissAnyDialogIfPresent("No", "Nein", "Yes", "OK", "Close");
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
			// Close any open menus or popups
			Keyboard.Press(VirtualKeyShort.ESCAPE);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(100);
			DismissAnyDialogIfPresent("OK", "Close", "Yes", "No");
		}

		// ===== Generic Element Finders =====

		protected AutomationElement FindByAutomationId(string automationId)
			=> MainWindow.FindFirstDescendant(CF.ByAutomationId(automationId));

		protected AutomationElement FindByName(string name)
			=> MainWindow.FindFirstDescendant(CF.ByName(name));

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

		protected AutomationElement[] FindAllByName(string name)
			=> MainWindow.FindAllDescendants(CF.ByName(name));

		protected AutomationElement[] FindAllByType(AutomationElement parent, ControlType controlType)
			=> parent.FindAllDescendants(CF.ByControlType(controlType));

		// ===== AvalonDock Document Helpers (from Alpine TestBase pattern) =====

		/// <summary>
		/// Gets a document tab by matching its display name.
		/// AvalonDock exposes document tabs as elements whose parent has
		/// Name == "AvalonDock.Layout.LayoutDocument".
		/// </summary>
		protected AutomationElement GetDocument(string documentName)
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
				catch
				{
				}
			}

			return null;
		}

		protected AutomationElement WaitForDocumentTab(string documentName, int timeoutSeconds = 10)
		{
			var result = Retry.WhileNull(
				() => GetDocument(documentName),
				timeout: TimeSpan.FromSeconds(timeoutSeconds),
				interval: TimeSpan.FromSeconds(1));
			Assert.That(result.Result, Is.Not.Null,
				$"Document tab '{documentName}' not found within {timeoutSeconds}s.");
			return result.Result;
		}

		protected AutomationElement FindDocumentTab(string title) => GetDocument(title);

		protected void ActivateDocumentTab(string title)
		{
			var doc = WaitForDocumentTab(title);
			var clickTarget = doc.FindFirstDescendant(CF.ByControlType(ControlType.Text)) ?? doc;
			clickTarget.Click(true);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(200);
		}

		/// <summary>
		/// Closes a document by clicking its close button.
		/// Automatically dismisses the TestApp's "Are you sure?" MessageBox.
		/// </summary>
		protected void CloseDocument(string documentName, bool confirmClose = true)
		{
			var doc = GetDocument(documentName);
			if (doc == null) return;

			var clickTarget = doc.FindFirstDescendant(CF.ByControlType(ControlType.Text)) ?? doc;
			clickTarget.Click(true);
			Wait.UntilInputIsProcessed();

			var closeButton = doc.FindFirstDescendant(CF.ByControlType(ControlType.Button));
			if (closeButton != null)
				closeButton.Click();
			else
				Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_W);

			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);

			if (confirmClose)
			{
				DismissAnyDialogIfPresent("Yes");
			}
		}

		// ===== AvalonDock Tool Window Helpers (from Alpine TestBase pattern) =====

		/// <summary>
		/// Finds a tool window (LayoutAnchorable) by its title.
		/// AvalonDock exposes anchorable tabs as elements whose parent has
		/// Name == "AvalonDock.Layout.LayoutAnchorable".
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
				catch
				{
				}
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
			catch
			{
				return false;
			}
		}

		// ===== Menu Helpers =====

		protected void ClickMenuItemByName(params string[] menuPath)
		{
			ClickMenuItemByName(false, menuPath);
		}
		
		protected void ClickMenuItemByName(bool moveMouse, params string[] menuPath)
		{
			for (int i = 0; i < menuPath.Length; i++)
			{
				var menuName = menuPath[i];
				var isTopLevel = (i == 0);

				var result = Retry.WhileNull(
					() =>
					{
						// For top-level items, search within the MenuBar to avoid
						// matching tool window tabs that share the same name.
						if (isTopLevel)
						{
							var menuBar = MainWindow.FindFirstDescendant(CF.ByControlType(ControlType.MenuBar));
							if (menuBar != null)
							{
								var item = menuBar.FindFirstDescendant(CF.ByName(menuName));
								if (item != null) return item;
							}
						}

						// For sub-items, prefer MenuItem control type
						var menuItems = MainWindow.FindAllDescendants(
							CF.ByControlType(ControlType.MenuItem).And(CF.ByName(menuName)));
						if (menuItems.Length > 0) return menuItems[0];

						// Fallback: any element with the name (for nested sub-menus)
						return MainWindow.FindFirstDescendant(CF.ByName(menuName));
					},
					timeout: TimeSpan.FromSeconds(5),
					interval: TimeSpan.FromMilliseconds(200));

				Assert.That(result.Result, Is.Not.Null, $"Menu item '{menuName}' not found.");
				result.Result.Click(moveMouse);
				Wait.UntilInputIsProcessed();
				System.Threading.Thread.Sleep(300);
			}
		}

		// ===== Floating Window Helpers =====

		/// <summary>
		/// Gets floating windows. AvalonDock floating windows can appear as:
		/// 1. Separate top-level windows (owned by the app)
		/// 2. Child Window elements inside MainWindow
		/// We check both strategies.
		/// </summary>
		protected AutomationElement[] GetFloatingWindows()
		{
			var results = new System.Collections.Generic.List<AutomationElement>();
			var mainHandle = MainWindow.Properties.NativeWindowHandle.ValueOrDefault;

			// Strategy 1: Top-level windows (different from MainWindow)
			try
			{
				foreach (var w in App.GetAllTopLevelWindows(Automation))
				{
					try
					{
						if (w.Properties.NativeWindowHandle.ValueOrDefault != mainHandle)
							results.Add(w);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			// Strategy 2: Child Window controls inside MainWindow (AvalonDock floating panels)
			try
			{
				var childWindows = MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Window));
				foreach (var cw in childWindows)
				{
					try
					{
						if (!cw.IsOffscreen)
							results.Add(cw);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			return results.ToArray();
		}

		protected AutomationElement WaitForFloatingWindow(int timeoutSeconds = 10)
		{
			var result = Retry.WhileNull(
				() =>
				{
					var w = GetFloatingWindows();
					return w.Length > 0 ? w[0] : null;
				},
				timeout: TimeSpan.FromSeconds(timeoutSeconds),
				interval: TimeSpan.FromMilliseconds(300));
			Assert.That(result.Result, Is.Not.Null, $"No floating window within {timeoutSeconds}s.");
			return result.Result;
		}

		// ===== Dialog Helpers (multi-strategy, from Alpine TestBase) =====

		protected AutomationElement FindAnyDialog()
		{
			var knownButtons = new[] { "OK", "Close", "Cancel", "Abort", "Yes", "No", "Retry", "Ignore" };
			var mainHandle = MainWindow.Properties.NativeWindowHandle.ValueOrDefault;

			// Strategy 1: Top-level windows
			try
			{
				foreach (var w in App.GetAllTopLevelWindows(Automation))
				{
					try
					{
						if (w.Properties.NativeWindowHandle.ValueOrDefault == mainHandle || w.IsOffscreen) continue;
						var btns = w.FindAllDescendants(CF.ByControlType(ControlType.Button));
						if (btns.Any(b => knownButtons.Contains(b.Name))) return w;
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			// Strategy 2: Child Window elements
			try
			{
				foreach (var w in MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Window)))
				{
					try
					{
						if (!w.IsEnabled && w.IsOffscreen) continue;
						var btns = w.FindAllDescendants(CF.ByControlType(ControlType.Button));
						if (btns.Any(b => knownButtons.Contains(b.Name))) return w;
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			// Strategy 3: Desktop search
			try
			{
				var desktop = Automation.GetDesktop();
				var processId = App.ProcessId;
				foreach (var w in desktop.FindAllChildren(CF.ByControlType(ControlType.Window)))
				{
					try
					{
						if (w.Properties.ProcessId.ValueOrDefault != processId) continue;
						if (w.Properties.NativeWindowHandle.ValueOrDefault == mainHandle || w.IsOffscreen) continue;
						var btns = w.FindAllDescendants(CF.ByControlType(ControlType.Button));
						if (btns.Any(b => knownButtons.Contains(b.Name))) return w;
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			return null;
		}

		protected string DismissAnyDialogIfPresent(params string[] preferredButtonNames)
		{
			var dialog = FindAnyDialog();
			if (dialog == null)
			{
				return null;
			}

			var title = "";
			try
			{
				title = dialog.Name;
			}
			catch
			{
			}

			DismissDialog(dialog, preferredButtonNames);
			return $"Dismissed: '{title}'";
		}

		protected void DismissDialog(AutomationElement dialog, params string[] preferredButtonNames)
		{
			try
			{
				dialog.Focus();
				System.Threading.Thread.Sleep(100);
			}
			catch
			{
			}

			Keyboard.Press(VirtualKeyShort.RETURN);
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(300);
			if (!IsDialogStillOpen(dialog)) return;

			var buttons = dialog.FindAllDescendants(CF.ByControlType(ControlType.Button));
			AutomationElement dismissBtn = null;
			foreach (var name in preferredButtonNames)
			{
				dismissBtn = buttons.FirstOrDefault(b =>
				{
					try
					{
						return b.Name == name;
					}
					catch
					{
						return false;
					}
				});
				if (dismissBtn != null) break;
			}

			if (dismissBtn != null)
			{
				try
				{
					dismissBtn.Click(true);
					Wait.UntilInputIsProcessed();
					System.Threading.Thread.Sleep(300);
				}
				catch
				{
				}

				if (!IsDialogStillOpen(dialog)) return;
				try
				{
					dismissBtn.Patterns.Invoke.PatternOrDefault?.Invoke();
					Wait.UntilInputIsProcessed();
					System.Threading.Thread.Sleep(300);
				}
				catch
				{
				}

				if (!IsDialogStillOpen(dialog)) return;
			}

			try
			{
				dialog.Focus();
			}
			catch
			{
			}

			Keyboard.Press(VirtualKeyShort.ESCAPE);
			Wait.UntilInputIsProcessed();
		}

		private bool IsDialogStillOpen(AutomationElement dialog)
		{
			try
			{
				return dialog.Properties.ProcessId.IsSupported && !dialog.IsOffscreen;
			}
			catch
			{
				return false;
			}
		}

		// ===== Infrastructure =====

		private string ResolveTestAppPath()
		{
			var envPath = Environment.GetEnvironmentVariable("AVALONDOCK_TESTAPP_PATH");
			if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath)) return envPath;

			var testDir = TestContext.CurrentContext.TestDirectory;
			var configs = new[] { "Debug", "Release" };
			var tfms = new[] { "net48", "net9.0-windows" };

			foreach (var config in configs)
			foreach (var tfm in tfms)
			{
				var candidate = Path.GetFullPath(Path.Combine(testDir,
					"..", "..", "..", "..", "..", "TestApp", "bin", config, tfm, "TestApp.exe"));
				if (File.Exists(candidate)) return candidate;
			}

			// Fallback search
			var dir = new DirectoryInfo(testDir);
			while (dir != null)
			{
				var matches = Directory.GetFiles(dir.FullName, "TestApp.exe", SearchOption.AllDirectories);
				var match = matches.FirstOrDefault(m =>
					m.Contains(Path.DirectorySeparatorChar + "TestApp" + Path.DirectorySeparatorChar) &&
					!m.Contains("AvalonDockTest"));
				if (match != null) return match;
				dir = dir.Parent;
				if (dir?.Name == "source" || dir?.Name == "AvalonDock") break;
			}

			Assert.Fail("TestApp.exe not found. Build the solution first.");
			return null;
		}

		private Window WaitForMainWindow()
		{
			var result = Retry.WhileNull(
				() =>
				{
					try
					{
						return App.GetMainWindow(Automation, TimeSpan.FromSeconds(2));
					}
					catch
					{
						return null;
					}
				},
				timeout: TimeSpan.FromSeconds(MainWindowTimeoutSeconds),
				interval: TimeSpan.FromSeconds(1));
			return result.Result;
		}

		private void WaitForAppReady()
		{
			var result = Retry.WhileNull(
				() => GetDocument("Document 1") ?? FindByName("Edit") ?? FindByName("Layout"),
				timeout: TimeSpan.FromSeconds(15),
				interval: TimeSpan.FromMilliseconds(500));

			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(500);
		}
	}
}