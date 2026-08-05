using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
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
            System.Threading.Thread.Sleep(300);

            // Load layout via Layout > Load > Layout_1
            ClickMenuItemByName("Layout", "Load", "Layout_1");
            WaitForLayoutSettled();

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
            System.Threading.Thread.Sleep(300);

            ClickMenuItemByName("Layout", "Load", "Layout_1");
            WaitForLayoutSettled();

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
            System.Threading.Thread.Sleep(300);

            // Add documents via the button in Document 1
            ActivateDocumentTab("Document 1");
            var addButton = FindByName("Click to add 2 documents");
            addButton?.Click();
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            // Load saved layout to restore original state
            ClickMenuItemByName("Layout", "Load", "Layout_2");
            WaitForLayoutSettled();

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
                System.Threading.Thread.Sleep(200);

                ClickMenuItemByName("Layout", "Load", "Layout_3");
                System.Threading.Thread.Sleep(300);
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
            System.Threading.Thread.Sleep(200);

            ClickMenuItemByName("Layout", "Load", "Layout_4");
            WaitForLayoutSettled();

            var doc1 = FindDocumentTab("Document 1");
            if (doc1 != null)
            {
                doc1.Click();
                Wait.UntilInputIsProcessed();
            }

            Assert.That(doc1, Is.Not.Null,
                "Document 1 should be accessible after layout restore (Issue #443).");
            Assert.That(App.HasExited, Is.False,
                "Application should remain responsive after layout restore (Issue #443).");
        }

        /// <summary>
        /// Verifies that a tool window detached into a standalone window comes back detached after a
        /// layout round trip. This is the end to end counterpart of the DTO level tests: it drives the
        /// entry in the default anchorable context menu of a plain DockingManager, saves, puts the tool
        /// window back, and expects the restore to recreate the window.
        /// </summary>
        [Test, Order(6)]
        public void SaveAndLoadLayout_RestoresDetachedWindow()
        {
            const string toolWindow = "Tool Window 1";

            DetachToolWindowViaContextMenu(toolWindow);

            var detached = WaitForDetachedWindow(toolWindow);
            Assert.That(detached, Is.Not.Null,
                $"'{toolWindow}' should be hosted by a standalone window before saving.");

            ClickMenuItemByName("Layout", "Save", "Layout_1");
            System.Threading.Thread.Sleep(500);

            // Put the tool window back, so a restore that ignored the detached state would be visible
            // as the absence of the window rather than as a leftover from before the save.
            detached.AsWindow()?.Close();
            var closed = Retry.WhileFalse(
                () => FindDetachedWindow(toolWindow) == null,
                timeout: TimeSpan.FromSeconds(10),
                interval: TimeSpan.FromMilliseconds(300));
            Assert.That(closed.Result, Is.True, "The standalone window should close before the restore.");

            ClickMenuItemByName("Layout", "Load", "Layout_1");
            WaitForLayoutSettled();

            var restored = Retry.WhileNull(
                () => FindDetachedWindow(toolWindow),
                timeout: TimeSpan.FromSeconds(20),
                interval: TimeSpan.FromMilliseconds(400)).Result;

            Assert.That(restored, Is.Not.Null,
                $"'{toolWindow}' was detached when the layout was saved, so restoring it should recreate the standalone window.");
            Assert.That(App.HasExited, Is.False,
                "The application should survive restoring a layout that contains a detached tool window.");

            // Leave the app docked again for any test that follows.
            restored.AsWindow()?.Close();
            Retry.WhileFalse(
                () => FindDetachedWindow(toolWindow) == null,
                timeout: TimeSpan.FromSeconds(10),
                interval: TimeSpan.FromMilliseconds(300));
        }

        /// <summary>
        /// Opens the context menu of a tool window and picks the entry that moves it into a standalone
        /// window.
        /// </summary>
        /// <param name="toolWindowName">Title of the tool window to detach.</param>
        /// <remarks>
        /// The context menu hangs off the pane title, which is not necessarily the element that
        /// <c>FindToolWindowTab</c> returns, so a few plausible targets are tried. When none of them
        /// produces the entry the failure lists what the menu actually offered, which distinguishes a
        /// missing menu from a missing entry.
        /// </remarks>
        private void DetachToolWindowViaContextMenu(string toolWindowName)
        {
            var seen = new System.Collections.Generic.List<string>();

            foreach (var target in GetContextMenuTargets(toolWindowName))
            {
                try
                {
                    target.RightClick();
                    Wait.UntilInputIsProcessed();
                    System.Threading.Thread.Sleep(500);
                }
                catch
                {
                    continue;
                }

                var windowItem = FindLiveMenuItem("Window");
                if (windowItem != null)
                {
                    windowItem.Click();
                    Wait.UntilInputIsProcessed();
                    System.Threading.Thread.Sleep(800);
                    return;
                }

                seen.AddRange(ListLiveMenuItems());
                Keyboard.Press(VirtualKeyShort.ESCAPE);
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(200);
            }

            Assert.Fail(seen.Count == 0
                ? $"Right clicking '{toolWindowName}' opened no context menu."
                : $"The context menu of '{toolWindowName}' offered no 'Window' entry. Items seen: {string.Join(", ", seen.Distinct())}.");
        }

        /// <summary>Gets the elements worth right clicking to raise the anchorable context menu.</summary>
        /// <param name="toolWindowName">Title of the tool window.</param>
        /// <returns>Candidate elements, most likely first.</returns>
        private System.Collections.Generic.IEnumerable<AutomationElement> GetContextMenuTargets(string toolWindowName)
        {
            var candidates = new System.Collections.Generic.List<AutomationElement>();

            // The title bar of the pane, which is what actually carries the context menu.
            try
            {
                candidates.AddRange(MainWindow.FindAllDescendants(CF.ByText(toolWindowName))
                    .Where(e =>
                    {
                        try { return !e.IsOffscreen; }
                        catch { return false; }
                    }));
            }
            catch
            {
                // Tree can change under us.
            }

            var tab = FindToolWindowTab(toolWindowName);
            if (tab != null && !candidates.Contains(tab)) candidates.Add(tab);

            return candidates;
        }

        /// <summary>Lists the headers of every menu item currently on screen, for diagnostics.</summary>
        /// <returns>The headers found.</returns>
        private System.Collections.Generic.IEnumerable<string> ListLiveMenuItems()
        {
            var names = new System.Collections.Generic.List<string>();

            foreach (var root in GetPopupSearchRoots())
            {
                try
                {
                    // Deliberately unfiltered: an item with an empty header, or one that is present but
                    // collapsed, is exactly the kind of thing this diagnostic needs to surface.
                    names.AddRange(root.FindAllDescendants(CF.ByControlType(ControlType.MenuItem))
                        .Select(i =>
                        {
                            try
                            {
                                var label = string.IsNullOrEmpty(i.Name) ? "(empty)" : i.Name;
                                return $"{label}[enabled={i.IsEnabled},offscreen={i.IsOffscreen}]";
                            }
                            catch
                            {
                                return "(unreadable)";
                            }
                        }));
                }
                catch
                {
                    // Popup vanished.
                }
            }

            return names;
        }

        /// <summary>
        /// Finds a menu item by header, ignoring the offscreen leftovers that dismissed WPF context
        /// menus leave behind in the automation tree.
        /// </summary>
        /// <param name="header">Header text of the wanted item.</param>
        /// <returns>The menu item, or <see langword="null"/> when it is not on screen.</returns>
        private AutomationElement FindLiveMenuItem(string header)
        {
            foreach (var root in GetPopupSearchRoots())
            {
                try
                {
                    var match = root.FindAllDescendants(CF.ByControlType(ControlType.MenuItem))
                        .FirstOrDefault(item =>
                        {
                            try { return item.Name == header && !item.IsOffscreen && item.IsEnabled; }
                            catch { return false; }
                        });

                    if (match != null) return match;
                }
                catch
                {
                    // A popup can vanish between enumeration and inspection.
                }
            }

            return null;
        }

        /// <summary>Gets the elements that can hold menu popups.</summary>
        /// <returns>The roots to search.</returns>
        private System.Collections.Generic.IEnumerable<AutomationElement> GetPopupSearchRoots()
        {
            var roots = new System.Collections.Generic.List<AutomationElement>();
            if (MainWindow != null) roots.Add(MainWindow);

            try { roots.AddRange(App.GetAllTopLevelWindows(Automation)); }
            catch { }

            return roots;
        }

        /// <summary>Finds the standalone window hosting the given tool window.</summary>
        /// <param name="toolWindowName">Title of the tool window.</param>
        /// <returns>The window, or <see langword="null"/> when nothing is detached.</returns>
        private AutomationElement FindDetachedWindow(string toolWindowName)
        {
            var mainHandle = MainWindow?.Properties.NativeWindowHandle.ValueOrDefault ?? IntPtr.Zero;

            try
            {
                foreach (var window in App.GetAllTopLevelWindows(Automation))
                {
                    try
                    {
                        if (window.Properties.NativeWindowHandle.ValueOrDefault == mainHandle) continue;
                        if (window.Title == toolWindowName) return window;
                    }
                    catch
                    {
                        // Window closed while being inspected.
                    }
                }
            }
            catch
            {
                // The application may be shutting down.
            }

            return null;
        }

        /// <summary>Waits until the standalone window is on screen.</summary>
        /// <param name="toolWindowName">Title of the tool window.</param>
        /// <returns>The window.</returns>
        private AutomationElement WaitForDetachedWindow(string toolWindowName)
        {
            return Retry.WhileNull(
                () => FindDetachedWindow(toolWindowName),
                timeout: TimeSpan.FromSeconds(25),
                interval: TimeSpan.FromMilliseconds(400)).Result;
        }
    }
}


