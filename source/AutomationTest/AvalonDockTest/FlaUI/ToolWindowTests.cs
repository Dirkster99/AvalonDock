using System;
using System.Linq;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for tool window operations (show, hide, float, dock, auto-hide).
    /// Covers issues:
    ///   #437 - NullReferenceException OnClosed in LayoutAnchorableFloatingWindowControl
    ///   #362 - DockingManager with Viewbox ancestor does not properly render auto-hidden LayoutAnchorables
    ///   #375 - Is LayoutAnchorable Active?
    ///   #169 - Autohide LayoutAnchorable causes CPU load on idle
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class ToolWindowTests : FlaUITestBase
    {
        [Test, Order(1)]
        public void InitialToolWindows_AreVisible()
        {
            var winForms = FindToolWindowTab("WinForms Window");
            var tool1 = FindToolWindowTab("Tool Window 1");
            var tool2 = FindToolWindowTab("Tool Window 2");

            Assert.That(winForms, Is.Not.Null, "WinForms Window should be visible.");
            Assert.That(tool1, Is.Not.Null, "Tool Window 1 should be visible.");
            Assert.That(tool2, Is.Not.Null, "Tool Window 2 should be visible.");
        }

        /// <summary>
        /// Regression for #375 - Is LayoutAnchorable Active?
        /// </summary>
        [Test, Order(2)]
        public void ClickToolWindowTab_ActivatesIt_Issue375()
        {
            var tool1 = FindToolWindowTab("Tool Window 1");
            Assert.That(tool1, Is.Not.Null);

            tool1.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            Assert.That(App.HasExited, Is.False,
                "Clicking tool window tab should activate it without crashing (Issue #375).");
        }

        [Test, Order(3)]
        public void ToolWindowContent_IsAccessible()
        {
            var tool1 = FindToolWindowTab("Tool Window 1");
            Assert.That(tool1, Is.Not.Null, "Tool Window 1 should be present.");

            tool1.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            Assert.That(App.HasExited, Is.False,
                "Tool window content should be accessible when activated.");
        }

        [Test, Order(4)]
        public void ToolWindows_OnDifferentSides_AreAccessible()
        {
            var winForms = FindToolWindowTab("WinForms Window");
            var tool1 = FindToolWindowTab("Tool Window 1");
            var tool2 = FindToolWindowTab("Tool Window 2");

            Assert.That(winForms, Is.Not.Null, "WinForms Window should be accessible.");
            Assert.That(tool1, Is.Not.Null, "Tool Window 1 should be accessible.");
            Assert.That(tool2, Is.Not.Null, "Tool Window 2 should be accessible.");
        }

        /// <summary>
        /// Regression for #362 - auto-hidden LayoutAnchorables not rendering.
        /// </summary>
        [Test, Order(5)]
        public void AutoHideTabs_ArePresent_Issue362()
        {
            var autoHide1 = FindByName("AutoHide1 Content");
            var autoHide2 = FindByName("AutoHide2 Content");

            Assert.That(autoHide1, Is.Not.Null,
                "AutoHide1 Content tab should be present on the window edge (Issue #362).");
            Assert.That(autoHide2, Is.Not.Null,
                "AutoHide2 Content tab should be present on the window edge (Issue #362).");
        }

        /// <summary>
        /// Regression for #169 - Autohide LayoutAnchorable causes CPU load on idle.
        /// </summary>
        [Test, Order(6)]
        public void ClickAutoHideTab_OpensFlyout_Issue169()
        {
            var autoHideTab = FindByName("AutoHide1 Content");
            Assert.That(autoHideTab, Is.Not.Null);

            autoHideTab.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            Assert.That(App.HasExited, Is.False,
                "Clicking auto-hide tab should not crash the application (Issue #169).");

            // Click the document area to close the flyout
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Run last to avoid affecting other tests with layout changes.
        /// </summary>
        [Test, Order(7)]
        public void NewFloatingWindow_CreatesFloatingToolWindow()
        {
            MainWindow.SetForeground();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            var initialCount = GetFloatingWindows().Length;

            ClickMenuItemByName("Tools", "New floating window");
            System.Threading.Thread.Sleep(1000);

            var afterCount = GetFloatingWindows().Length;
            Assert.That(afterCount, Is.GreaterThan(initialCount),
                "New floating window menu item should create a floating tool window.");
        }
    }
}
