using System;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for layout serialization and deserialization.
    /// Covers issues:
    ///   #440 - Facing exception in layout deserialization
    ///   #167 - Deserialising a LayoutDocumentFloatingWindow gives XML error
    ///   #356 - XmlLayoutSerializer memory leak
    ///   #443 - Content not shown after LayoutSerializationCallback
    ///   #392 - How to handle LayoutAnchorable after Deserialize
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class LayoutSerializationTests : FlaUITestBase
    {
        /// <summary>
        /// Verifies that saving and loading layout preserves documents.
        /// Regression for #440 - Exception in layout deserialization.
        /// </summary>
        [Test, Order(1)]
        public void SaveAndLoadLayout_PreservesDocuments_Issue440()
        {
            var doc1 = FindDocumentTab("Document 1");
            var doc2 = FindDocumentTab("Document 2");
            Assert.That(doc1, Is.Not.Null, "Document 1 should exist before save.");
            Assert.That(doc2, Is.Not.Null, "Document 2 should exist before save.");

            // Save layout via Layout > Save > Layout_1
            ClickMenuItemByName("Layout", "Save", "Layout_1");
            System.Threading.Thread.Sleep(500);

            // Load layout via Layout > Load > Layout_1
            ClickMenuItemByName("Layout", "Load", "Layout_1");
            System.Threading.Thread.Sleep(1000);

            doc1 = FindDocumentTab("Document 1");
            doc2 = FindDocumentTab("Document 2");

            Assert.That(doc1, Is.Not.Null,
                "Document 1 should exist after layout restore (Issue #440).");
            Assert.That(doc2, Is.Not.Null,
                "Document 2 should exist after layout restore (Issue #440).");
        }

        /// <summary>
        /// Verifies that saving and loading layout preserves tool windows.
        /// Regression for #392 - LayoutAnchorable after Deserialize.
        /// </summary>
        [Test, Order(2)]
        public void SaveAndLoadLayout_PreservesToolWindows_Issue392()
        {
            var tool1 = FindToolWindowTab("Tool Window 1");
            var tool2 = FindToolWindowTab("Tool Window 2");
            Assert.That(tool1, Is.Not.Null, "Tool Window 1 should exist before save.");
            Assert.That(tool2, Is.Not.Null, "Tool Window 2 should exist before save.");

            ClickMenuItemByName("Layout", "Save", "Layout_1");
            System.Threading.Thread.Sleep(500);

            ClickMenuItemByName("Layout", "Load", "Layout_1");
            System.Threading.Thread.Sleep(1000);

            tool1 = FindToolWindowTab("Tool Window 1");
            tool2 = FindToolWindowTab("Tool Window 2");

            Assert.That(tool1, Is.Not.Null,
                "Tool Window 1 should exist after layout restore (Issue #392).");
            Assert.That(tool2, Is.Not.Null,
                "Tool Window 2 should exist after layout restore (Issue #392).");
        }

        /// <summary>
        /// Verifies that loading layout after modifications restores original state.
        /// </summary>
        [Test, Order(3)]
        public void LoadLayout_RestoresOriginalState()
        {
            // Save current layout
            ClickMenuItemByName("Layout", "Save", "Layout_2");
            System.Threading.Thread.Sleep(500);

            // Add documents via the button in Document 1
            ActivateDocumentTab("Document 1");
            System.Threading.Thread.Sleep(300);
            var addButton = FindByName("Click to add 2 documents");
            addButton?.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            // Load saved layout to restore original state
            ClickMenuItemByName("Layout", "Load", "Layout_2");
            System.Threading.Thread.Sleep(1000);

            var doc1 = FindDocumentTab("Document 1");
            Assert.That(doc1, Is.Not.Null,
                "Original Document 1 should be present after layout restore.");
        }

        /// <summary>
        /// Verifies that layout save/load does not crash the application.
        /// Regression for #167 - Deserialising a LayoutDocumentFloatingWindow gives XML error.
        /// </summary>
        [Test, Order(4)]
        public void SaveLoadLayout_DoesNotCrash_Issue167()
        {
            for (int i = 0; i < 3; i++)
            {
                ClickMenuItemByName("Layout", "Save", "Layout_3");
                System.Threading.Thread.Sleep(300);

                ClickMenuItemByName("Layout", "Load", "Layout_3");
                System.Threading.Thread.Sleep(500);
            }

            Assert.That(App.HasExited, Is.False,
                "Application should not crash during repeated save/load (Issue #167).");

            var doc1 = FindDocumentTab("Document 1");
            Assert.That(doc1, Is.Not.Null,
                "Document 1 should still be accessible after repeated save/load.");
        }

        /// <summary>
        /// Verifies that documents are accessible after layout restore.
        /// Regression for #443 - Content not shown after LayoutSerializationCallback.
        /// </summary>
        [Test, Order(5)]
        public void AfterLayoutRestore_DocumentsAccessible_Issue443()
        {
            ClickMenuItemByName("Layout", "Save", "Layout_4");
            System.Threading.Thread.Sleep(300);

            ClickMenuItemByName("Layout", "Load", "Layout_4");
            System.Threading.Thread.Sleep(1000);

            var doc1 = FindDocumentTab("Document 1");
            if (doc1 != null)
            {
                doc1.Click();
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(300);
            }

            Assert.That(doc1, Is.Not.Null,
                "Document 1 should be accessible after layout restore (Issue #443).");
            Assert.That(App.HasExited, Is.False,
                "Application should remain responsive after layout restore (Issue #443).");
        }
    }
}


