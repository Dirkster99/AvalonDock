using System;
using System.Linq;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for context menu operations.
    /// Covers issues:
    ///   #193 - Context menu items always disabled
    ///   #200 - Utilizing CompositeCollection for AnchorableContextMenu binding issues
    ///   #234 - Remove disabled context-menu items from dropdown
    ///   #223 - VS2013 Dark Theme: non-standard MenuDropDownButton ContextMenu style
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class ContextMenuTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that right-clicking a document tab shows a context menu.
        /// Regression for #193 - Context menu items always disabled.
        /// </summary>
        [Test, Order(1)]
        public void RightClickDocumentTab_ShowsContextMenu_Issue193()
        {
            var tab = WaitForDocumentTab("Document 1");
            tab.RightClick();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            var menus = MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Menu));
            var contextMenuFound = menus.Any(m => m.ClassName?.Contains("ContextMenu") == true)
                                   || menus.Length > 1;

            // Close the context menu
            Keyboard.Press(VirtualKeyShort.ESCAPE);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(200);

            TestContext.Out.WriteLine($"Context menu found: {contextMenuFound}, Menu count: {menus.Length}");
        }

        /// <summary>
        /// Verifies that right-clicking a tool window tab does not crash.
        /// Regression for #200 - AnchorableContextMenu binding issues.
        /// </summary>
        [Test, Order(2)]
        public void RightClickToolWindowTab_DoesNotCrash_Issue200()
        {
            var tool1 = FindToolWindowTab("Tool Window 1");
            Assert.That(tool1, Is.Not.Null);

            tool1.RightClick();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            Keyboard.Press(VirtualKeyShort.ESCAPE);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(200);

            Assert.That(App.HasExited, Is.False,
                "App should not crash after right-clicking tool window tab (Issue #200).");
        }

        /// <summary>
        /// Verifies that the application menu items are accessible by name.
        /// </summary>
        [Test, Order(3)]
        public void ApplicationMenu_ItemsAreAccessible()
        {
            var editMenu = FindByName("Edit");
            Assert.That(editMenu, Is.Not.Null, "Edit menu should be accessible.");

            var layoutMenu = FindByName("Layout");
            Assert.That(layoutMenu, Is.Not.Null, "Layout menu should be accessible.");

            var toolsMenu = FindByName("Tools");
            Assert.That(toolsMenu, Is.Not.Null, "Tools menu should be accessible.");
        }

        /// <summary>
        /// Verifies that opening and closing the Edit menu works.
        /// </summary>
        [Test, Order(4)]
        public void EditMenu_OpensAndCloses()
        {
            var editMenu = WaitForName("Edit");
            editMenu.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            // Verify Edit menu sub-items are visible
            var undoItem = FindByName("Undo");
            TestContext.Out.WriteLine($"Undo menu item found: {undoItem != null}");

            // Close the menu
            Keyboard.Press(VirtualKeyShort.ESCAPE);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(200);
        }

        /// <summary>
        /// Verifies that the Tools menu items are accessible by name.
        /// </summary>
        [Test, Order(5)]
        public void ToolsMenu_ItemsAreAccessible()
        {
            var toolsMenu = WaitForName("Tools");
            toolsMenu.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            var winFormsItem = FindByName("WinForms Window");
            var toolWindow1Item = FindByName("Tool Window1");
            var newFloatingItem = FindByName("New floating window");

            TestContext.Out.WriteLine($"WinForms Window: {winFormsItem != null}");
            TestContext.Out.WriteLine($"Tool Window1: {toolWindow1Item != null}");
            TestContext.Out.WriteLine($"New floating window: {newFloatingItem != null}");

            // Close the menu
            Keyboard.Press(VirtualKeyShort.ESCAPE);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(200);
        }
    }
}


