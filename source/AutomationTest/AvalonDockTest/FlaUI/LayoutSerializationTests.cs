using System;
using System.Collections.Generic;
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

            FocusMainWindow();
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

            FocusMainWindow();
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

        /// <summary>Header of the context menu entry that moves an anchorable into a standalone window.</summary>
        private const string DetachToWindowHeader = "Window";

        /// <summary>Headers that only the anchorable context menu carries.</summary>
        /// <remarks>
        /// WPF publishes its menu bar as <see cref="ControlType.Menu"/> exactly like a context menu, so the
        /// control type alone cannot tell the two apart. An entry is therefore only accepted when one of
        /// these headers is on screen next to it. Getting this wrong is what made an unopened context menu
        /// look like a context menu holding the entries of the menu bar - Edit, Layout, Tools, Theme.
        /// </remarks>
        private static readonly string[] AnchorableMenuHeaders =
            { "Float", "Dock", "Dock as Tabbed Document", "Auto Hide" };

        /// <summary>
        /// Opens the context menu of a tool window and picks the entry that moves it into a standalone
        /// window.
        /// </summary>
        /// <param name="toolWindowName">Title of the tool window to detach.</param>
        /// <remarks>
        /// <para>
        /// The tool windows of the test application live in a pane that is 50 pixels wide, which leaves
        /// their titles trimmed down to a couple of pixels in both the pane title and the tab. Right
        /// clicking the centre of such an element is a coin flip: the point lands on the neighbouring
        /// button or tab just as easily as on the title, and a right click that misses opens nothing at
        /// all. The drop down button of the pane title is therefore tried first. It carries the very same
        /// <c>AnchorableContextMenu</c>, it has a fixed size, and it is driven through the toggle pattern,
        /// so no coordinate is involved. The right click is kept as a fallback, largest target first.
        /// </para>
        /// <para>
        /// Every attempt starts by dismissing open popups. A menu that is left standing swallows the next
        /// gesture, which then looks exactly like a menu that never opened - one missed detection used to
        /// poison every following candidate and fail the whole helper.
        /// </para>
        /// </remarks>
        private void DetachToolWindowViaContextMenu(string toolWindowName)
        {
            var seen = new List<string>();
            var menuOpened = false;

            ActivateToolWindow(toolWindowName);

            // Two passes, with the openers queried afresh on the second one: an earlier test in this
            // fixture restores a layout, which rebuilds the docking controls, so the first set of elements
            // can already point at controls that no longer exist.
            for (var attempt = 0; attempt < 2; attempt++)
            {
                foreach (var openMenu in GetMenuOpeners(toolWindowName))
                {
                    // The entry toggles, so a detach that has already happened must not be undone by a
                    // second pick.
                    if (FindDetachedWindow(toolWindowName) != null) return;

                    var outcome = TryDetachThroughMenu(toolWindowName, openMenu);
                    if (outcome == MenuAttempt.Detached) return;
                    if (outcome == MenuAttempt.NoMenu) continue;

                    menuOpened = true;
                    seen.AddRange(ListLiveMenuItems());
                }
            }

            CloseAnyOpenMenu();

            Assert.Fail(!menuOpened
                ? $"No anchorable context menu appeared for '{toolWindowName}' on any candidate target."
                : $"The context menu of '{toolWindowName}' offered no working '{DetachToWindowHeader}' entry. Items seen: {string.Join(", ", seen.Distinct())}.");
        }

        /// <summary>How far a single attempt at detaching through the context menu got.</summary>
        private enum MenuAttempt
        {
            /// <summary>The gesture raised no anchorable context menu.</summary>
            NoMenu,

            /// <summary>The menu was up, but picking its detach entry produced no window.</summary>
            NoWindow,

            /// <summary>The tool window is now hosted by a standalone window.</summary>
            Detached
        }

        /// <summary>Runs one gesture and picks the entry it is supposed to reveal.</summary>
        /// <param name="toolWindowName">Title of the tool window to detach.</param>
        /// <param name="openMenu">The gesture that raises the context menu.</param>
        /// <returns>How far the attempt got.</returns>
        private MenuAttempt TryDetachThroughMenu(string toolWindowName, Func<bool> openMenu)
        {
            // A popup that is still standing swallows the gesture, so every attempt starts from a closed
            // menu.
            CloseAnyOpenMenu();

            if (!openMenu()) return MenuAttempt.NoMenu;

            // Wait for the entry itself, so a menu that is merely slow is not mistaken for a menu that
            // never opened.
            var windowItem = Retry.WhileNull(
                () => FindAnchorableMenuItem(DetachToWindowHeader, requireEnabled: true),
                timeout: TimeSpan.FromSeconds(5),
                interval: TimeSpan.FromMilliseconds(200)).Result;

            if (windowItem == null)
                return IsAnchorableContextMenuOpen() ? MenuAttempt.NoWindow : MenuAttempt.NoMenu;

            InvokeMenuItem(windowItem);

            var detached = Retry.WhileNull(
                () => FindDetachedWindow(toolWindowName),
                timeout: TimeSpan.FromSeconds(15),
                interval: TimeSpan.FromMilliseconds(300)).Result;

            // A pick that does not take is what a stale popup looks like, so this is worth another route.
            return detached != null ? MenuAttempt.Detached : MenuAttempt.NoWindow;
        }

        /// <summary>Makes the tool window the selected content of its pane.</summary>
        /// <param name="toolWindowName">Title of the tool window.</param>
        /// <remarks>
        /// The pane title, and with it the menu this helper drives, always belongs to the anchorable that
        /// is selected in the pane. The Tools menu selects it through the model, which is worth a lot in a
        /// 50 pixel wide pane: the two tabs there are a few pixels apart, so clicking the right one is the
        /// kind of coin flip this helper exists to avoid.
        /// </remarks>
        private void ActivateToolWindow(string toolWindowName)
        {
            // The Tools menu carries an entry for the tool window this fixture detaches, and names it
            // without the space its title has.
            if (toolWindowName != "Tool Window 1") return;

            FocusMainWindow();
            ClickMenuItemByName("Tools", "Tool Window1");
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);
        }

        /// <summary>Gets the gestures worth trying to raise the anchorable context menu.</summary>
        /// <param name="toolWindowName">Title of the tool window.</param>
        /// <returns>Gestures, most reliable first, each reporting whether it was carried out.</returns>
        private IEnumerable<Func<bool>> GetMenuOpeners(string toolWindowName)
        {
            var openers = new List<Func<bool>>();

            var dropDown = FindPaneDropDownButton(toolWindowName);
            if (dropDown != null) openers.Add(() => TryOpenDropDown(dropDown));

            foreach (var target in GetContextMenuTargets(toolWindowName))
                openers.Add(() => TryRightClick(target));

            return openers;
        }

        /// <summary>Opens the menu of the pane title drop down button without using the mouse.</summary>
        /// <param name="button">The drop down button.</param>
        /// <returns><see langword="true"/> when the gesture was carried out.</returns>
        /// <remarks>
        /// <c>DropDownButton</c> opens its menu from <c>OnClick</c>, and the toggle pattern of a WPF
        /// <c>ToggleButton</c> routes straight into that, so the menu opens without a pixel being hit.
        /// </remarks>
        private bool TryOpenDropDown(AutomationElement button)
        {
            try
            {
                var toggle = button.Patterns.Toggle.PatternOrDefault;
                if (toggle != null)
                    toggle.Toggle();
                else
                    button.Click();

                Wait.UntilInputIsProcessed();
                return true;
            }
            catch
            {
                // The button can go away with the pane it belongs to.
                return false;
            }
        }

        /// <summary>Right clicks a candidate element.</summary>
        /// <param name="target">The element to right click.</param>
        /// <returns><see langword="true"/> when the gesture was carried out.</returns>
        private bool TryRightClick(AutomationElement target)
        {
            try
            {
                target.RightClick();
                Wait.UntilInputIsProcessed();
                return true;
            }
            catch
            {
                // No clickable point, or the element is gone.
                return false;
            }
        }

        /// <summary>Finds the drop down button of the pane title that shows the given tool window.</summary>
        /// <param name="toolWindowName">Title of the tool window.</param>
        /// <returns>The button, or <see langword="null"/> when it cannot be identified for sure.</returns>
        /// <remarks>
        /// The search walks up from an element carrying the title until it reaches an ancestor holding
        /// exactly one toggle button. Inside a single anchorable pane that button is unambiguous: the auto
        /// hide and close buttons beside it are plain buttons. An ancestor holding several of them spans
        /// more than one pane, and picking the wrong one would detach the wrong tool window, so the search
        /// gives up instead and lets the caller fall back to right clicking.
        /// </remarks>
        private AutomationElement FindPaneDropDownButton(string toolWindowName)
        {
            var current = FindToolWindowTab(toolWindowName);

            for (var level = 0; level < 8 && current != null; level++)
            {
                AutomationElement parent = null;
                try { parent = current.Parent; }
                catch { return null; }

                if (parent == null) return null;

                var toggleButtons = FindToggleButtons(parent);
                if (toggleButtons.Count == 1) return toggleButtons[0];
                if (toggleButtons.Count > 1) return null;

                current = parent;
            }

            return null;
        }

        /// <summary>Gets the toggle buttons below the given element that are on screen.</summary>
        /// <param name="root">The element to search.</param>
        /// <returns>The toggle buttons found.</returns>
        private List<AutomationElement> FindToggleButtons(AutomationElement root)
        {
            try
            {
                return root.FindAllDescendants(CF.ByControlType(ControlType.Button))
                    .Where(button =>
                    {
                        try { return IsOnScreen(button) && button.Patterns.Toggle.PatternOrDefault != null; }
                        catch { return false; }
                    })
                    .ToList();
            }
            catch
            {
                // Tree can change under us.
                return new List<AutomationElement>();
            }
        }

        /// <summary>Gets the elements worth right clicking to raise the anchorable context menu.</summary>
        /// <param name="toolWindowName">Title of the tool window.</param>
        /// <returns>Candidate elements, largest first.</returns>
        private IEnumerable<AutomationElement> GetContextMenuTargets(string toolWindowName)
        {
            var candidates = new List<AutomationElement>();

            // The title bar of the pane, which is what actually carries the context menu.
            try
            {
                candidates.AddRange(MainWindow.FindAllDescendants(CF.ByText(toolWindowName)).Where(IsOnScreen));
            }
            catch
            {
                // Tree can change under us.
            }

            var tab = FindToolWindowTab(toolWindowName);
            if (tab != null && !candidates.Contains(tab)) candidates.Add(tab);

            // Largest first: in a 50 pixel wide pane the titles are trimmed to a couple of pixels, and the
            // centre of such an element is as likely to sit on its neighbour as on the element itself.
            return candidates.OrderByDescending(ClickableArea).ToList();
        }

        /// <summary>Gets the area an element offers to the mouse.</summary>
        /// <param name="element">The element to measure.</param>
        /// <returns>The area in square pixels, or zero when it cannot be read.</returns>
        private static double ClickableArea(AutomationElement element)
        {
            try
            {
                var rectangle = element.BoundingRectangle;
                return (double)rectangle.Width * rectangle.Height;
            }
            catch
            {
                return 0d;
            }
        }

        /// <summary>Lists every menu entry currently on screen, for diagnostics.</summary>
        /// <returns>The entries found.</returns>
        private IEnumerable<string> ListLiveMenuItems()
        {
            var names = new List<string>();

            foreach (var root in GetPopupSearchRoots())
            {
                try
                {
                    // Deliberately unfiltered: an item with an empty header, or one that is present but
                    // collapsed, is exactly the kind of thing this diagnostic needs to surface.
                    names.AddRange(root.FindAllDescendants(CF.ByControlType(ControlType.MenuItem))
                        .Select(item =>
                        {
                            try
                            {
                                var label = string.IsNullOrEmpty(item.Name) ? "(empty)" : item.Name;
                                return $"{label}[enabled={item.IsEnabled},offscreen={item.IsOffscreen}]";
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

        /// <summary>Tells whether the anchorable context menu is currently up.</summary>
        /// <returns><see langword="true"/> when one of its entries is on screen.</returns>
        private bool IsAnchorableContextMenuOpen() =>
            AnchorableMenuHeaders.Any(header => FindAnchorableMenuItem(header, requireEnabled: false) != null);

        /// <summary>
        /// Finds an entry of the open anchorable context menu, ignoring the offscreen leftovers that
        /// dismissed WPF context menus leave behind in the automation tree.
        /// </summary>
        /// <param name="header">Header text of the wanted entry.</param>
        /// <param name="requireEnabled">Whether the entry has to be enabled to count as a match.</param>
        /// <returns>The menu item, or <see langword="null"/> when it is not on screen.</returns>
        /// <remarks>
        /// The menu is recognised by its own entries rather than by diffing the menus on screen against a
        /// snapshot: <c>AnchorableContextMenu</c> is one shared instance, so a stale snapshot taken while
        /// it happened to be up hid every later opening of it behind an already known element.
        /// </remarks>
        private AutomationElement FindAnchorableMenuItem(string header, bool requireEnabled)
        {
            foreach (var root in GetPopupSearchRoots())
            {
                try
                {
                    var items = root.FindAllDescendants(CF.ByControlType(ControlType.MenuItem))
                        .Where(IsOnScreen)
                        .ToList();

                    if (!items.Any(item => HasName(item, AnchorableMenuHeaders))) continue;

                    var match = items.FirstOrDefault(item =>
                        HasName(item, header) && (!requireEnabled || IsUsable(item)));

                    if (match != null) return match;
                }
                catch
                {
                    // A popup can vanish between enumeration and inspection.
                }
            }

            return null;
        }

        /// <summary>Picks a menu entry, preferring the invoke pattern over a click on its centre.</summary>
        /// <param name="item">The entry to pick.</param>
        private void InvokeMenuItem(AutomationElement item)
        {
            try
            {
                var invoke = item.Patterns.Invoke.PatternOrDefault;
                if (invoke != null)
                {
                    invoke.Invoke();
                    Wait.UntilInputIsProcessed();
                    return;
                }
            }
            catch
            {
                // Fall through to the click below.
            }

            try
            {
                item.Click();
                Wait.UntilInputIsProcessed();
            }
            catch
            {
                // The popup went away; the caller notices through the window that never appears.
            }
        }

        /// <summary>Dismisses an open menu so it cannot swallow the next gesture.</summary>
        private void CloseAnyOpenMenu()
        {
            for (var i = 0; i < 2; i++)
            {
                Keyboard.Press(VirtualKeyShort.ESCAPE);
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(150);
            }
        }

        /// <summary>Brings the main window back to the front.</summary>
        /// <remarks>
        /// The standalone window is an ordinary top level window: it takes the foreground when it opens and
        /// can cover the menu bar, so the menu driven steps around a detach have to raise the main window
        /// first.
        /// </remarks>
        private void FocusMainWindow()
        {
            try
            {
                MainWindow.SetForeground();
                Wait.UntilInputIsProcessed();
                System.Threading.Thread.Sleep(200);
            }
            catch
            {
                // Best effort - the menu helper retries on its own.
            }
        }

        /// <summary>Tells whether an element is on screen.</summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns><see langword="true"/> when it is on screen.</returns>
        private static bool IsOnScreen(AutomationElement element)
        {
            try { return !element.IsOffscreen; }
            catch { return false; }
        }

        /// <summary>Tells whether an element carries one of the given names.</summary>
        /// <param name="element">The element to inspect.</param>
        /// <param name="names">The names to accept.</param>
        /// <returns><see langword="true"/> on a match.</returns>
        private static bool HasName(AutomationElement element, params string[] names)
        {
            try { return names.Contains(element.Name); }
            catch { return false; }
        }

        /// <summary>Tells whether an element is enabled.</summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns><see langword="true"/> when it is enabled.</returns>
        private static bool IsUsable(AutomationElement element)
        {
            try { return element.IsEnabled; }
            catch { return false; }
        }

        /// <summary>Gets the elements that can hold menu popups.</summary>
        /// <returns>The roots to search.</returns>
        private IEnumerable<AutomationElement> GetPopupSearchRoots()
        {
            var roots = new List<AutomationElement>();
            if (MainWindow != null) roots.Add(MainWindow);

            try { roots.AddRange(App.GetAllTopLevelWindows(Automation)); }
            catch
            {
                // The application may be shutting down.
            }

            try
            {
                // WPF context menus live in their own popup windows, which are children of the desktop
                // rather than of the window they belong to.
                var processId = App.ProcessId;
                roots.AddRange(Automation.GetDesktop()
                    .FindAllChildren(CF.ByControlType(ControlType.Window))
                    .Where(window =>
                    {
                        try { return window.Properties.ProcessId.ValueOrDefault == processId; }
                        catch { return false; }
                    }));
            }
            catch
            {
                // Desktop enumeration is best effort.
            }

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


