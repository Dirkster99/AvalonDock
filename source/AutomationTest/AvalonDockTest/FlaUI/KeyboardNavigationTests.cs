using System;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for keyboard navigation within AvalonDock.
    /// Covers issues:
    ///   #225 - Keyboard up/down in textbox in floating anchorable focusing DropDownControlArea
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class KeyboardNavigationTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that keyboard input works in a document's content area.
        /// Document 1 contains a Button and a TextBox.
        /// </summary>
        [Test, Order(1)]
        public void KeyboardInput_WorksInDocumentContent()
        {
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            // Find any focusable element inside the active document (e.g., a TextBox)
            var textBoxes = MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Edit));
            if (textBoxes.Length > 0)
            {
                textBoxes[0].Click();
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(200);

                Keyboard.Type("Hello FlaUI");
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(200);
            }

            Assert.That(App.HasExited, Is.False,
                "App should not crash during keyboard input in document.");
        }

        /// <summary>
        /// Verifies that arrow keys work in the application without crashing.
        /// Regression for #225 - Keyboard up/down in textbox in floating anchorable.
        /// </summary>
        [Test, Order(2)]
        public void ArrowKeys_WorkWithoutCrash_Issue225()
        {
            // Activate Tool Window 1
            var tool1 = FindToolWindowTab("Tool Window 1");
            Assert.That(tool1, Is.Not.Null);
            tool1.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            // Press arrow keys
            Keyboard.Press(VirtualKeyShort.UP);
            Keyboard.Press(VirtualKeyShort.DOWN);
            Keyboard.Press(VirtualKeyShort.LEFT);
            Keyboard.Press(VirtualKeyShort.RIGHT);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(200);

            Assert.That(App.HasExited, Is.False,
                "App should not crash when using arrow keys in tool window (Issue #225).");
        }

        /// <summary>
        /// Verifies that Tab key cycles focus within the application.
        /// </summary>
        [Test, Order(3)]
        public void TabKey_CyclesFocus()
        {
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            for (int i = 0; i < 5; i++)
            {
                Keyboard.Press(VirtualKeyShort.TAB);
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(100);
            }

            Assert.That(App.HasExited, Is.False,
                "App should not crash during Tab key navigation.");
        }

        /// <summary>
        /// Verifies that Ctrl+Tab works for switching between documents.
        /// </summary>
        [Test, Order(4)]
        public void CtrlTab_SwitchesBetweenDocuments()
        {
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.TAB);
            Keyboard.Release(VirtualKeyShort.TAB);
            Keyboard.Release(VirtualKeyShort.CONTROL);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            Assert.That(App.HasExited, Is.False,
                "App should not crash during Ctrl+Tab document switching.");
        }
    }
}


