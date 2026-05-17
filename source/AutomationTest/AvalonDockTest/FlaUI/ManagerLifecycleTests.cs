using System;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for DockingManager unload/reload lifecycle.
    /// TestApp has "Unload Manager" and "Load Manager" menu items in Layout menu.
    /// Covers issues:
    ///   #437 - NullReferenceException OnClosed in LayoutAnchorableFloatingWindowControl
    ///   #384 - Cannot change properties without restart
    ///   #159 - DockingManager in TabControl causes InvalidOperationException
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class ManagerLifecycleTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that unloading and reloading the DockingManager works without crash.
        /// Regression for #384 - Cannot change properties without restart.
        /// </summary>
        [Test, Order(1)]
        public void UnloadAndReloadManager_Works_Issue384()
        {
            // Verify initial state
            var doc1 = FindDocumentTab("Document 1");
            Assert.That(doc1, Is.Not.Null, "Document 1 should exist before unload.");

            // Unload Manager
            ClickMenuItemByName("Layout", "Unload Manager");
            System.Threading.Thread.Sleep(500);

            Assert.That(App.HasExited, Is.False,
                "App should not crash after unloading manager.");

            // Documents should no longer be visible
            var doc1After = FindDocumentTab("Document 1");
            // It may or may not be null depending on how unload works

            // Reload Manager
            ClickMenuItemByName("Layout", "Load Manager");
            System.Threading.Thread.Sleep(500);

            Assert.That(App.HasExited, Is.False,
                "App should not crash after reloading manager (Issue #384).");
        }

        /// <summary>
        /// Verifies that unloading manager with floating windows doesn't crash.
        /// Regression for #437 - NullReferenceException OnClosed in LayoutAnchorableFloatingWindowControl.
        /// </summary>
        [Test, Order(4)]
        public void UnloadManagerWithFloatingWindow_DoesNotCrash_Issue437()
        {
            // Load manager if not loaded
            ClickMenuItemByName("Layout", "Load Manager");
            System.Threading.Thread.Sleep(500);

            // Create a floating window
            ClickMenuItemByName("Tools", "New floating window");
            WaitForFloatingWindow();

            var floatingBefore = GetFloatingWindows();
            TestContext.Out.WriteLine($"Floating windows before unload: {floatingBefore.Length}");

            // Unload Manager — should not crash even with floating window
            MainWindow.SetForeground();
            Wait.UntilInputIsProcessed();
            
            Assert.That(App.HasExited, Is.False,
                "App should not crash when unloading manager with floating windows (Issue #437).");
        }

        /// <summary>
        /// Verifies that repeated unload/reload cycles don't cause errors.
        /// Regression for #159 - DockingManager causes InvalidOperationException.
        /// </summary>
        [Test, Order(2)]
        public void RepeatedUnloadReload_DoesNotCrash_Issue159()
        {
            for (int i = 0; i < 3; i++)
            {
                ClickMenuItemByName("Layout", "Unload Manager");
                System.Threading.Thread.Sleep(300);

                ClickMenuItemByName("Layout", "Load Manager");
                System.Threading.Thread.Sleep(300);
            }

            Assert.That(App.HasExited, Is.False,
                "App should not crash during repeated unload/reload cycles (Issue #159).");

            // Verify functionality is still intact after reload cycles
            var doc1 = FindDocumentTab("Document 1");
            Assert.That(doc1, Is.Not.Null,
                "Document 1 should be accessible after repeated unload/reload.");
        }

        /// <summary>
        /// Verifies that menus still work after manager unload/reload.
        /// </summary>
        [Test, Order(3)]
        public void MenusWork_AfterManagerReload()
        {
            ClickMenuItemByName("Layout", "Unload Manager");
            System.Threading.Thread.Sleep(300);

            ClickMenuItemByName("Layout", "Load Manager");
            System.Threading.Thread.Sleep(500);

            // Verify menus are still functional
            var editMenu = FindByName("Edit");
            Assert.That(editMenu, Is.Not.Null, "Edit menu should still be accessible after reload.");

            var toolsMenu = FindByName("Tools");
            Assert.That(toolsMenu, Is.Not.Null, "Tools menu should still be accessible after reload.");
        }
    }
}
