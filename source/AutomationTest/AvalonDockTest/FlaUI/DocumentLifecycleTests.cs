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
    /// Regression tests for document close/open lifecycle.
    /// Covers issues:
    ///   #204 - DockingManager not updating state when closing tab
    ///   #201 - Closing active document selects first instead of previous
    ///   #184 - All documents disappear if close cancelled
    ///   #232 - Closing behavior difference LayoutAnchorable vs LayoutDocument
    ///   #196 - CanClose false but showing close button
    ///   #315 - Closing documents via middle click
    ///   #7   - Click header quickly causes exception
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class DocumentLifecycleTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that cancelling a document close keeps the document present.
        /// TestApp shows a "Are you sure?" MessageBox on DocumentClosing.
        /// Regression for #184 - All documents disappear if close cancelled.
        /// </summary>
        [Test, Order(5)]
        public void CancelDocumentClose_DocumentRemains_Issue184()
        {
            var doc1Before = FindDocumentTab("Document 1");
            Assert.That(doc1Before, Is.Not.Null, "Document 1 should exist before close attempt.");

            // Try to close Document 1 but cancel
            CloseDocument("Document 1", confirmClose: false);

            // The MessageBox should appear; dismiss with "No" to cancel close
            System.Threading.Thread.Sleep(500);
            DismissAnyDialogIfPresent("No");

            // Verify document is still present
            var doc1After = FindDocumentTab("Document 1");
            Assert.That(doc1After, Is.Not.Null,
                "Document 1 should still be present after cancelling close (Issue #184).");
        }

        /// <summary>
        /// Verifies that after closing a document, remaining documents are still accessible.
        /// Regression for #204 - DockingManager not updating state when closing tab.
        /// </summary>
        [Test, Order(2)]
        public void CloseDocument_RemainingDocsAccessible_Issue204()
        {
            // First add extra documents so we have something to work with
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);
            var addButton = FindByName("Click to add 2 documents");
            if (addButton != null)
            {
                addButton.Click();
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(500);
            }

            // Close Test1 (confirm with Yes)
            CloseDocument("Test1", confirmClose: true);
            System.Threading.Thread.Sleep(500);

            // Verify Document 1 and Document 2 still exist
            var doc1 = FindDocumentTab("Document 1");
            var doc2 = FindDocumentTab("Document 2");

            Assert.That(doc1, Is.Not.Null,
                "Document 1 should still be accessible after closing Test1 (Issue #204).");
            Assert.That(doc2, Is.Not.Null,
                "Document 2 should still be accessible after closing Test1 (Issue #204).");
        }

        /// <summary>
        /// Verifies that rapidly clicking document tabs doesn't crash the application.
        /// Regression for #7 - Click header quickly causes exception.
        /// </summary>
        [Test, Order(1)]
        public void RapidTabClicking_DoesNotCrash_Issue7()
        {
            for (int i = 0; i < 10; i++)
            {
                ActivateDocumentTab("Document 1");
                System.Threading.Thread.Sleep(50);
                ActivateDocumentTab("Document 2");
                System.Threading.Thread.Sleep(50);
            }

            Assert.That(App.HasExited, Is.False,
                "Rapidly clicking between document tabs should not crash (Issue #7).");
        }

        /// <summary>
        /// Verifies that WinForms Window (CanClose=False) does not show a close button
        /// or at least cannot be closed.
        /// Regression for #196 - CanClose false but showing close button.
        /// </summary>
        [Test, Order(3)]
        public void CanCloseFalse_CannotCloseWindow_Issue196()
        {
            var winForms = FindToolWindowTab("WinForms Window");
            Assert.That(winForms, Is.Not.Null, "WinForms Window should be present.");

            // Click to activate it
            winForms.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            // Try Ctrl+W — should not close because CanClose=False
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_W);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            // Dismiss any dialog that may appear
            DismissAnyDialogIfPresent("No", "Cancel");

            // Verify WinForms Window is still present
            var winFormsAfter = FindToolWindowTab("WinForms Window");
            Assert.That(winFormsAfter, Is.Not.Null,
                "WinForms Window with CanClose=False should not be closed (Issue #196).");
        }

        /// <summary>
        /// Verifies that closing and re-adding documents works correctly.
        /// Regression for #232 - Closing behavior difference between LayoutAnchorable and LayoutDocument.
        /// </summary>
        [Test, Order(2)]
        public void CloseAndReAddDocuments_Works_Issue232()
        {
            // Add documents
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(200);
            var addButton = FindByName("Click to add 2 documents");
            addButton?.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            var test1 = FindDocumentTab("Test1");
            Assert.That(test1, Is.Not.Null, "Test1 should have been added.");

            // Close Test1
            CloseDocument("Test1", confirmClose: true);
            System.Threading.Thread.Sleep(500);

            // Add documents again
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(200);
            addButton = FindByName("Click to add 2 documents");
            addButton?.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            Assert.That(App.HasExited, Is.False,
                "App should not crash when re-adding documents after closing (Issue #232).");
        }
    }
}
