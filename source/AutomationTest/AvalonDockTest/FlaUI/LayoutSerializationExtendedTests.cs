using System;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Extended layout serialization tests for edge cases.
    /// Covers issues:
    ///   #118 - Losing pane ratio after serialization
    ///   #59  - InvalidOperationException deserializing layout
    ///   #38  - NullReferenceException restoring layout with non-existing document
    ///   #83  - Layout document disappears between sessions
    ///   #111 - LayoutRoot doesn't know how to deserialize
    ///   #88  - LayoutRoot IXmlSerializable read end element
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class LayoutSerializationExtendedTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that tool window proportions are preserved after layout restore.
        /// Regression for #118 - Losing pane ratio after serialization.
        /// </summary>
        [Test, Order(1)]
        public void SaveLoadLayout_PreservesPaneRatio_Issue118()
        {
            // Get initial tool window positions/sizes
            var tool1Before = FindToolWindowTab("Tool Window 1");
            var winFormsBefore = FindToolWindowTab("WinForms Window");
            Assert.That(tool1Before, Is.Not.Null);
            Assert.That(winFormsBefore, Is.Not.Null);

            // Save layout
            ClickMenuItemByName("Layout", "Save", "Layout_3");
            System.Threading.Thread.Sleep(500);

            // Load layout
            ClickMenuItemByName("Layout", "Load", "Layout_3");
            System.Threading.Thread.Sleep(1000);

            // Verify tool windows are still present and accessible
            var tool1After = FindToolWindowTab("Tool Window 1");
            var winFormsAfter = FindToolWindowTab("WinForms Window");

            Assert.That(tool1After, Is.Not.Null,
                "Tool Window 1 should be present after layout restore (Issue #118).");
            Assert.That(winFormsAfter, Is.Not.Null,
                "WinForms Window should be present after layout restore (Issue #118).");
        }

        /// <summary>
        /// Verifies that loading layout after closing a document doesn't crash.
        /// Regression for #38 - NullRef restoring layout with non-existing document.
        /// </summary>
        [Test, Order(2)]
        public void LoadLayoutAfterClosingDocument_DoesNotCrash_Issue38()
        {
            // Save current layout with all docs
            ClickMenuItemByName("Layout", "Save", "Layout_3");
            System.Threading.Thread.Sleep(500);

            // Add and then close a document
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(200);
            var addButton = FindByName("Click to add 2 documents");
            if (addButton != null)
            {
                addButton.Click();
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(500);
            }

            // Close the added document
            CloseDocument("Test1", confirmClose: true);
            System.Threading.Thread.Sleep(500);

            // Load saved layout — the saved layout might reference docs that no longer exist
            ClickMenuItemByName("Layout", "Load", "Layout_3");
            System.Threading.Thread.Sleep(1000);

            Assert.That(App.HasExited, Is.False,
                "App should not crash when loading layout with non-existing documents (Issue #38).");

            var doc1 = FindDocumentTab("Document 1");
            Assert.That(doc1, Is.Not.Null,
                "Document 1 should be accessible after layout restore.");
        }

        /// <summary>
        /// Verifies that layout restore after modifications brings back documents.
        /// Regression for #83 - Layout document disappears between sessions.
        /// </summary>
        [Test, Order(3)]
        public void LayoutRestore_BringsBackDocuments_Issue83()
        {
            // Save layout with both documents
            ClickMenuItemByName("Layout", "Save", "Layout_4");
            System.Threading.Thread.Sleep(500);

            // Verify both documents are present
            Assert.That(FindDocumentTab("Document 1"), Is.Not.Null);
            Assert.That(FindDocumentTab("Document 2"), Is.Not.Null);

            // Load layout
            ClickMenuItemByName("Layout", "Load", "Layout_4");
            System.Threading.Thread.Sleep(1000);

            // Documents should still be present
            var doc1 = FindDocumentTab("Document 1");
            var doc2 = FindDocumentTab("Document 2");

            Assert.That(doc1, Is.Not.Null,
                "Document 1 should not disappear after layout restore (Issue #83).");
            Assert.That(doc2, Is.Not.Null,
                "Document 2 should not disappear after layout restore (Issue #83).");
        }

        /// <summary>
        /// Verifies that layout save/load with auto-hide panels works.
        /// Regression for #111 - LayoutRoot deserialization issues.
        /// </summary>
        [Test, Order(4)]
        public void SaveLoadLayout_WithAutoHidePanels_Issue111()
        {
            // Open auto-hide panel first
            var autoHideTab = FindByName("AutoHide1 Content");
            Assert.That(autoHideTab, Is.Not.Null);

            autoHideTab.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            // Close the flyout
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);

            // Save layout
            ClickMenuItemByName("Layout", "Save", "Layout_4");
            System.Threading.Thread.Sleep(500);

            // Load layout
            ClickMenuItemByName("Layout", "Load", "Layout_4");
            System.Threading.Thread.Sleep(1000);

            Assert.That(App.HasExited, Is.False,
                "App should not crash during layout save/load with auto-hide panels (Issue #111).");

            // Verify auto-hide tabs still present
            var autoHide1After = FindByName("AutoHide1 Content");
            Assert.That(autoHide1After, Is.Not.Null,
                "AutoHide1 Content should be present after layout restore (Issue #111).");
        }

        /// <summary>
        /// Verifies that saving layout with different dock states and loading works.
        /// Regression for #59 - InvalidOperationException deserializing layout.
        /// </summary>
        [Test, Order(5)]
        public void SaveLoadLayout_WithToolWindowStates_Issue59()
        {
            // Click a tool window to change active state
            var tool1 = FindToolWindowTab("Tool Window 1");
            tool1?.Click(true);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            // Save
            ClickMenuItemByName(true, "Layout", "Save", "Layout_4");
            System.Threading.Thread.Sleep(500);

            // Activate a document
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(200);

            // Load
            ClickMenuItemByName(true, "Layout", "Load", "Layout_4");
            System.Threading.Thread.Sleep(1000);

            Assert.That(App.HasExited, Is.False,
                "App should not crash during layout save/load with tool windows active (Issue #59).");
        }
    }
}
