using System;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Smoke tests for basic AvalonDock UI functionality.
    /// Ensures the application starts correctly and all major UI elements are present.
    /// Acts as a baseline verification before more specific regression tests.
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class SmokeTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that the test host application starts and the main window appears.
        /// </summary>
        [Test, Order(1)]
        public void Application_Starts_MainWindowAppears()
        {
            Assert.That(App, Is.Not.Null, "Application should be launched.");
            Assert.That(App.HasExited, Is.False, "Application should still be running.");
            Assert.That(MainWindow, Is.Not.Null, "Main window should be present.");
        }

        /// <summary>
        /// Verifies that the main window title is correct.
        /// </summary>
        [Test, Order(2)]
        public void MainWindow_HasCorrectTitle()
        {
            Assert.That(MainWindow.Title, Does.Contain("MainWindow"),
                "Main window should have the expected title.");
        }

        /// <summary>
        /// Verifies that the DockingManager is present by looking for its visual elements.
        /// Note: DockingManager is a custom WPF control and may not expose an AutomationId.
        /// We verify it by checking for its child layout elements (document pane, tool panes).
        /// </summary>
        [Test, Order(3)]
        public void DockingManager_IsPresent()
        {
            // The DockingManager hosts document and tool panes.
            // If we can find documents and tools, the DockingManager is working.
            var doc1 = FindDocumentTab("Document 1");
            var tool1 = FindToolWindowTab("Tool Window 1");

            Assert.That(doc1 != null || tool1 != null,
                "DockingManager should be present - expected to find at least documents or tool windows.");
        }

        /// <summary>
        /// Verifies that the main menu items are accessible by header name.
        /// </summary>
        [Test, Order(4)]
        public void MainMenu_IsAccessible()
        {
            var editMenu = FindByName("Edit");
            var layoutMenu = FindByName("Layout");
            var toolsMenu = FindByName("Tools");

            Assert.That(editMenu, Is.Not.Null, "Edit menu should be accessible.");
            Assert.That(layoutMenu, Is.Not.Null, "Layout menu should be accessible.");
            Assert.That(toolsMenu, Is.Not.Null, "Tools menu should be accessible.");
        }

        /// <summary>
        /// Verifies that both initial document tabs are present.
        /// </summary>
        [Test, Order(5)]
        public void InitialDocuments_AllPresent()
        {
            var doc1 = FindDocumentTab("Document 1");
            var doc2 = FindDocumentTab("Document 2");

            Assert.That(doc1, Is.Not.Null, "Document 1 should be present.");
            Assert.That(doc2, Is.Not.Null, "Document 2 should be present.");
        }

        /// <summary>
        /// Verifies that all tool windows are present.
        /// </summary>
        [Test, Order(6)]
        public void InitialToolWindows_AllPresent()
        {
            var winForms = FindToolWindowTab("WinForms Window");
            var tool1 = FindToolWindowTab("Tool Window 1");
            var tool2 = FindToolWindowTab("Tool Window 2");

            Assert.That(winForms, Is.Not.Null, "WinForms Window should be present.");
            Assert.That(tool1, Is.Not.Null, "Tool Window 1 should be present.");
            Assert.That(tool2, Is.Not.Null, "Tool Window 2 should be present.");
        }

        /// <summary>
        /// Verifies that auto-hide panels are present.
        /// </summary>
        [Test, Order(7)]
        public void AutoHidePanels_ArePresent()
        {
            var autoHide1 = FindByName("AutoHide1 Content");
            var autoHide2 = FindByName("AutoHide2 Content");

            Assert.That(autoHide1, Is.Not.Null, "AutoHide1 Content tab should be present.");
            Assert.That(autoHide2, Is.Not.Null, "AutoHide2 Content tab should be present.");
        }

        /// <summary>
        /// Verifies that the main window has a reasonable size.
        /// </summary>
        [Test, Order(8)]
        public void MainWindow_HasReasonableSize()
        {
            var bounds = MainWindow.BoundingRectangle;
            Assert.That(bounds.Width, Is.GreaterThan(400),
                "Main window should have a reasonable width.");
            Assert.That(bounds.Height, Is.GreaterThan(300),
                "Main window should have a reasonable height.");
        }
    }
}


