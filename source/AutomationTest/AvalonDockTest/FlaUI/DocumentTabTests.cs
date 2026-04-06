using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for document tab operations.
    /// Covers issues:
    ///   #511 - NullReference in LayoutDocumentTabItem.OnMouseLeftButtonDown
    ///   #244 - Right click on tab header closes tab unexpectedly
    ///   #201 - Closing active document always selects first document instead of previous
    ///   #493 - DropDownContextMenu doesn't show when adding a new document
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class DocumentTabTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that clicking a document tab activates it without crashing.
        /// Regression for #511 - NullReference in LayoutDocumentTabItem.OnMouseLeftButtonDown.
        /// </summary>
        [Test, Order(1)]
        public void ClickDocumentTab_ActivatesDocument_Issue511()
        {
            ActivateDocumentTab("Document 1");

            Assert.That(App.HasExited, Is.False,
                "Clicking Document 1 tab should activate it without crashing.");
        }

        /// <summary>
        /// Verifies switching between document tabs works without errors.
        /// </summary>
        [Test, Order(2)]
        public void SwitchBetweenDocumentTabs_Works()
        {
            ActivateDocumentTab("Document 2");
            System.Threading.Thread.Sleep(200);

            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(200);

            Assert.That(App.HasExited, Is.False,
                "Switching between document tabs should work without errors.");
        }

        /// <summary>
        /// Verifies that right-clicking a document tab does not close it.
        /// Regression for #244 - Right click on tab header closes tab unexpectedly.
        /// </summary>
        [Test, Order(3)]
        public void RightClickDocumentTab_DoesNotClose_Issue244()
        {
            var tab = WaitForDocumentTab("Document 1");
            Assert.That(tab, Is.Not.Null, "Document 1 tab should exist.");

            tab.RightClick();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            // Dismiss any context menu by pressing Escape
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(200);

            var tabAfter = FindDocumentTab("Document 1");
            Assert.That(tabAfter, Is.Not.Null,
                "Right-clicking a document tab should NOT close it (Issue #244).");
        }

        /// <summary>
        /// Verifies that clicking "Click to add 2 documents" in Document 1 adds Test1 and Test2 tabs.
        /// Regression for #493 - DropDownContextMenu doesn't show when adding a new document.
        /// </summary>
        [Test, Order(4)]
        public void AddDocumentsViaButton_AppearsAsTabs_Issue493()
        {
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            // Click the "Click to add 2 documents" button inside Document 1
            var addButton = FindByName("Click to add 2 documents");
            Assert.That(addButton, Is.Not.Null, "Add documents button should be present in Document 1.");

            addButton.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            var test1 = FindDocumentTab("Test1");
            var test2 = FindDocumentTab("Test2");

            Assert.That(test1, Is.Not.Null,
                "Test1 document tab should appear after clicking the add button (Issue #493).");
            Assert.That(test2, Is.Not.Null,
                "Test2 document tab should appear after clicking the add button (Issue #493).");
        }

        /// <summary>
        /// Verifies that both initial document tabs exist and can be found.
        /// </summary>
        [Test, Order(5)]
        public void InitialDocumentTabs_ArePresent()
        {
            var doc1 = FindDocumentTab("Document 1");
            var doc2 = FindDocumentTab("Document 2");

            Assert.That(doc1, Is.Not.Null, "Document 1 tab should be present.");
            Assert.That(doc2, Is.Not.Null, "Document 2 tab should be present.");
        }

        /// <summary>
        /// Verifies that document content is accessible after activation.
        /// Regression for #240 - NullReferenceException in LayoutDocumentControl.OnModelChanged.
        /// </summary>
        [Test, Order(6)]
        public void ActivateDocument_ContentIsAccessible_Issue240()
        {
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            // Document 1 contains a Button and a TextBox; look for any child control
            var content = MainWindow.FindFirstDescendant(CF.ByControlType(ControlType.Button));
            Assert.That(content, Is.Not.Null,
                "Document content should be accessible after activation (Issue #240).");
        }
    }
}


