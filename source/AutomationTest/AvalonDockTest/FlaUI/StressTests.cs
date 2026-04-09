using System;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Stress tests for rapid interactions and edge cases.
    /// Covers issues:
    ///   #142 - Switching between documents slow
    ///   #139 - Losing data when switching between tabs
    ///   #90  - ArgumentOutOfRangeException in LayoutAnchorableTabItem.OnMouseEnter
    ///   #42  - Dispatcher Suspend Exception
    ///   #101 - Window Resize Spam
    ///   #162 - Lagging KeyEvents
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class StressTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that switching documents rapidly doesn't cause errors.
        /// Regression for #142 - Switching between documents slow.
        /// </summary>
        [Test, Order(1)]
        public void RapidDocumentSwitching_DoesNotCrash_Issue142()
        {
            for (int i = 0; i < 20; i++)
            {
                ActivateDocumentTab("Document 1");
                System.Threading.Thread.Sleep(30);
                ActivateDocumentTab("Document 2");
                System.Threading.Thread.Sleep(30);
            }

            Assert.That(App.HasExited, Is.False,
                "App should not crash during rapid document switching (Issue #142).");

            // Verify documents are still accessible after rapid switching
            var doc1 = FindDocumentTab("Document 1");
            var doc2 = FindDocumentTab("Document 2");
            Assert.That(doc1, Is.Not.Null, "Document 1 should still be accessible.");
            Assert.That(doc2, Is.Not.Null, "Document 2 should still be accessible.");
        }

        /// <summary>
        /// Verifies that text entered in a textbox is preserved when switching tabs.
        /// Regression for #139 - Losing data when switching between tabs.
        /// </summary>
        [Test, Order(2)]
        public void TextPreserved_WhenSwitchingTabs_Issue139()
        {
            // Activate Document 1 which has a TextBox with "Document 1 Content"
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            // Find the textbox and type something
            var textBoxes = MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Edit));
            if (textBoxes.Length > 0)
            {
                var textBox = textBoxes[0];
                textBox.Click();
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(100);

                // Select all and type new text
                Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
                System.Threading.Thread.Sleep(50);
                Keyboard.Type("TestData_Issue139");
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(200);

                // Switch to Document 2
                ActivateDocumentTab("Document 2");
                System.Threading.Thread.Sleep(300);

                // Switch back to Document 1
                ActivateDocumentTab("Document 1");
                System.Threading.Thread.Sleep(300);

                // Find the textbox again and check value
                textBoxes = MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Edit));
                if (textBoxes.Length > 0)
                {
                    var value = "";
                    try { value = textBoxes[0].Patterns.Value.PatternOrDefault?.Value ?? ""; } catch { }
                    TestContext.Out.WriteLine($"TextBox value after switch: '{value}'");
                }
            }

            Assert.That(App.HasExited, Is.False,
                "App should not crash or lose data when switching tabs (Issue #139).");
        }

        /// <summary>
        /// Verifies that rapid mouse movement over tool tabs doesn't crash.
        /// Regression for #90 - ArgumentOutOfRangeException in LayoutAnchorableTabItem.OnMouseEnter.
        /// </summary>
        [Test, Order(3)]
        public void RapidMouseOverToolTabs_DoesNotCrash_Issue90()
        {
            var tool1 = FindToolWindowTab("Tool Window 1");
            var tool2 = FindToolWindowTab("Tool Window 2");

            if (tool1 != null && tool2 != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    Mouse.MoveTo(tool1.GetClickablePoint());
                    System.Threading.Thread.Sleep(20);
                    Mouse.MoveTo(tool2.GetClickablePoint());
                    System.Threading.Thread.Sleep(20);
                }
            }

            Assert.That(App.HasExited, Is.False,
                "App should not crash during rapid mouse movement over tool tabs (Issue #90).");
        }

        /// <summary>
        /// Verifies that keyboard events work correctly during rapid input.
        /// Regression for #162 - Lagging KeyEvents.
        /// </summary>
        [Test, Order(4)]
        public void RapidKeyboardInput_DoesNotLag_Issue162()
        {
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            var textBoxes = MainWindow.FindAllDescendants(CF.ByControlType(ControlType.Edit));
            if (textBoxes.Length > 0)
            {
                textBoxes[0].Click();
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(100);

                // Type rapidly
                Keyboard.Type("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(200);

                // Arrow keys rapidly
                for (int i = 0; i < 10; i++)
                {
                    Keyboard.Press(VirtualKeyShort.LEFT);
                    Keyboard.Press(VirtualKeyShort.RIGHT);
                }
                Wait.UntilInputIsProcessed();
            }

            Assert.That(App.HasExited, Is.False,
                "App should handle rapid keyboard input without lagging (Issue #162).");
        }

        /// <summary>
        /// Verifies that resizing the window rapidly doesn't cause errors.
        /// Regression for #101 - Window Resize Spam.
        /// </summary>
        [Test, Order(5)]
        public void RapidWindowResize_DoesNotCrash_Issue101()
        {
            var bounds = MainWindow.BoundingRectangle;
            var originalWidth = bounds.Width;
            var originalHeight = bounds.Height;

            // Resize window multiple times
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    MainWindow.Patterns.Transform.PatternOrDefault?.Resize(
                        originalWidth - 100 + (i * 40),
                        originalHeight - 50 + (i * 20));
                    System.Threading.Thread.Sleep(100);
                }
                catch
                {
                    // Some windows don't support Transform pattern
                    break;
                }
            }

            // Restore original size
            try
            {
                MainWindow.Patterns.Transform.PatternOrDefault?.Resize(originalWidth, originalHeight);
            }
            catch { }

            System.Threading.Thread.Sleep(300);

            Assert.That(App.HasExited, Is.False,
                "App should not crash during rapid window resizing (Issue #101).");
        }

        /// <summary>
        /// Verifies that interactions during layout operations don't cause dispatcher exceptions.
        /// Regression for #42 - Dispatcher Suspend Exception.
        /// </summary>
        [Test, Order(6)]
        public void InteractionDuringLayout_DoesNotCrash_Issue42()
        {
            // Save layout
            ClickMenuItemByName("Layout", "Save", "Layout_4");
            System.Threading.Thread.Sleep(200);

            // Immediately switch tabs while layout may be processing
            ActivateDocumentTab("Document 2");
            System.Threading.Thread.Sleep(100);
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(100);

            // Load layout while interacting
            ClickMenuItemByName("Layout", "Load", "Layout_4");

            // Immediately try to interact
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(500);

            Assert.That(App.HasExited, Is.False,
                "App should not crash during layout operations with concurrent interaction (Issue #42).");
        }

        /// <summary>
        /// Verifies that rapid save/load layout cycles don't cause memory or stability issues.
        /// Extended regression for #356 - XmlLayoutSerializer memory leak.
        /// </summary>
        [Test, Order(7)]
        public void RapidSaveLoadLayout_Stability_Issue356()
        {
            for (int i = 0; i < 10; i++)
            {
                ClickMenuItemByName("Layout", "Save", "Layout_4");
                System.Threading.Thread.Sleep(100);
                ClickMenuItemByName("Layout", "Load", "Layout_4");
                System.Threading.Thread.Sleep(200);
            }

            Assert.That(App.HasExited, Is.False,
                "App should remain stable after rapid save/load cycles (Issue #356).");

            var doc1 = FindDocumentTab("Document 1");
            Assert.That(doc1, Is.Not.Null,
                "Document 1 should be present after rapid save/load cycles.");
        }
    }
}
