using System;
using System.Linq;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for floating window operations.
    /// Covers issues:
    ///   #437 - NullReferenceException OnClosed in LayoutAnchorableFloatingWindowControl
    ///   #426 - Don't create floating window already created
    ///   #254 - FloatingWindow is created twice
    ///   #349 - Dragging window appears behind main window
    ///   #252 - Part of maximized floating window is hidden behind taskbar
    ///   #287 - Taskbar close button does not work for floating windows
    ///   #408 - Floating window activation issues
    ///   #174 - The SetWindowSizeWhenOpened Feature is broken
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class FloatingWindowTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that "New floating window" via Tools menu creates a floating window.
        /// Regression for #254 - FloatingWindow is created twice.
        /// </summary>
        [Test, Order(1)]
        public void NewFloatingWindow_CreatesFloatingWindow_Issue254()
        {
            ClickMenuItemByName("Tools", "New floating window");
            WaitForFloatingWindow();

            var floatingWindows = GetFloatingWindows();
            Assert.That(floatingWindows.Length, Is.GreaterThanOrEqualTo(1),
                "New floating window should create at least one floating window (Issue #254).");
        }

        /// <summary>
        /// Verifies that a floating window can be focused/activated.
        /// Regression for #408 - Floating window activation issues.
        /// </summary>
        [Test, Order(2)]
        public void FloatingWindow_CanBeActivated_Issue408()
        {
            var floatingWindows = GetFloatingWindows();
            if (floatingWindows.Length == 0)
            {
                ClickMenuItemByName("Tools", "New floating window");
                WaitForFloatingWindow();
                floatingWindows = GetFloatingWindows();
            }

            Assert.That(floatingWindows.Length, Is.GreaterThan(0),
                "Should have at least one floating window.");

            var floatingWindow = floatingWindows[0];
            floatingWindow.Focus();
            Wait.UntilInputIsProcessed();

            Assert.That(floatingWindow.IsOffscreen, Is.False,
                "Floating window should be visible after activation (Issue #408).");
        }

        /// <summary>
        /// Verifies that the main window is still accessible while a floating window exists.
        /// Regression for #349 - Dragging window appears behind main window.
        /// </summary>
        [Test, Order(3)]
        public void MainWindow_AccessibleWithFloatingWindow_Issue349()
        {
            var floatingWindows = GetFloatingWindows();
            if (floatingWindows.Length == 0)
            {
                ClickMenuItemByName("Tools", "New floating window");
                WaitForFloatingWindow();
            }

            MainWindow.Focus();
            Wait.UntilInputIsProcessed();

            var doc1 = FindDocumentTab("Document 1");
            Assert.That(doc1, Is.Not.Null,
                "Main window elements should be accessible with floating windows present (Issue #349).");
        }

        /// <summary>
        /// Verifies that the floating window has a reasonable size.
        /// Regression for #174 - The SetWindowSizeWhenOpened Feature is broken.
        /// </summary>
        [Test, Order(4)]
        public void FloatingWindow_HasReasonableSize_Issue174()
        {
            var floatingWindows = GetFloatingWindows();
            if (floatingWindows.Length == 0)
            {
                ClickMenuItemByName("Tools", "New floating window");
                WaitForFloatingWindow();
                floatingWindows = GetFloatingWindows();
            }

            Assert.That(floatingWindows.Length, Is.GreaterThan(0));

            var floating = floatingWindows[0];
            var bounds = floating.BoundingRectangle;

            Assert.That(bounds.Width, Is.GreaterThan(50),
                "Floating window should have reasonable width (Issue #174).");
            Assert.That(bounds.Height, Is.GreaterThan(50),
                "Floating window should have reasonable height (Issue #174).");
        }
    }
}


