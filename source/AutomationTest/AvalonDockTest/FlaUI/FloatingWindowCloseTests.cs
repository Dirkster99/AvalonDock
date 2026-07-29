using System;
using System.Linq;
using System.Runtime.InteropServices;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
    /// <summary>
    /// Regression tests for closing floating windows via the OS (Alt+F4, taskbar close button,
    /// system menu) instead of the AvalonDock UI.
    /// Covers issues:
    ///   #368 - LayoutAnchorable floating window can not be reopened after Ctrl+F4.
    /// </summary>
    [TestFixture]
    [Category("FlaUI")]
    public class FloatingWindowCloseTests : FlaUITestBase
    {
        private const uint WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;
        private const uint GA_ROOT = 2;
        private const string MessageBoxClassName = "#32770";

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        /// <summary>
        /// Verifies that a system close (Alt+F4/taskbar/system menu) hides the floating tool
        /// window through the normal Hiding lifecycle, and that it can be shown again afterwards.
        /// Regression for #368 - the window used to be destroyed instead, losing it for good.
        /// </summary>
        [Test]
        public void FloatingToolWindow_SystemClose_CanBeReshown_Issue368()
        {
            ClickMenuItemByName("Tools", "Float Tool Window1");

            var floatingWindow = Retry.WhileNull(
                () => GetTopLevelFloatingWindow(),
                timeout: TimeSpan.FromSeconds(10),
                interval: TimeSpan.FromMilliseconds(300));
            Assert.That(floatingWindow.Result, Is.Not.Null, "Floating the tool window should create a floating window.");

            // UIA may expose the handle of a child hwnd (HwndHost content); resolve the top level window first.
            var floatingHandle = GetAncestor(floatingWindow.Result.Properties.NativeWindowHandle.Value, GA_ROOT);

            // Same message the window receives from Alt+F4, the taskbar close button and the system menu.
            PostMessage(floatingHandle, WM_SYSCOMMAND, new IntPtr(SC_CLOSE), IntPtr.Zero);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(500);

            Assert.That(App.HasExited, Is.False, "Closing a floating tool window must not exit the application.");

            // A system close must raise the anchorable's Hiding event; TestApp shows a confirmation dialog for it.
            var messageBox = Retry.WhileNull(
                () => FindMessageBox(),
                timeout: TimeSpan.FromSeconds(5),
                interval: TimeSpan.FromMilliseconds(300));
            Assert.That(messageBox.Result, Is.Not.Null, "A system close must raise the Hiding event of its anchorables.");

            // Confirm the hide; the default button is "Yes" but captions are localized, so match by class instead.
            messageBox.Result.Focus();
            Wait.UntilInputIsProcessed();
            Keyboard.Press(VirtualKeyShort.RETURN);
            Wait.UntilInputIsProcessed();
            System.Threading.Thread.Sleep(300);

            var hidden = Retry.WhileFalse(
                () => GetTopLevelFloatingWindow() == null,
                timeout: TimeSpan.FromSeconds(5),
                interval: TimeSpan.FromMilliseconds(300));
            Assert.That(hidden.Result, Is.True, "The floating window should be hidden after the system close.");

            MainWindow.SetForeground();
            Wait.UntilInputIsProcessed();
            ClickMenuItemByName("Tools", "Tool Window1");

            var reshown = Retry.WhileFalse(
                () => GetTopLevelFloatingWindow() != null,
                timeout: TimeSpan.FromSeconds(10),
                interval: TimeSpan.FromMilliseconds(300));
            Assert.That(reshown.Result, Is.True, "The tool window must be shown again after the system close hid it.");
        }

        /// <summary>
        /// Gets the first visible floating window with its own native window handle,
        /// excluding message boxes. Returns null if there is none.
        /// </summary>
        private AutomationElement GetTopLevelFloatingWindow()
        {
            try
            {
                var mainHandle = MainWindow.Properties.NativeWindowHandle.ValueOrDefault;
                return GetFloatingWindows().FirstOrDefault(w =>
                {
                    try
                    {
                        var handle = w.Properties.NativeWindowHandle.ValueOrDefault;
                        return handle != IntPtr.Zero
                            && handle != mainHandle
                            && w.ClassName != MessageBoxClassName
                            && !w.IsOffscreen;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Finds a native MessageBox belonging to the TestApp. Button captions are localized,
        /// so the dialog is matched by window class instead.
        /// </summary>
        private AutomationElement FindMessageBox()
        {
            try
            {
                var dialog = MainWindow.FindFirstDescendant(CF.ByClassName(MessageBoxClassName));
                if (dialog != null) return dialog;

                var desktop = Automation.GetDesktop();
                return desktop.FindAllChildren(CF.ByClassName(MessageBoxClassName))
                    .FirstOrDefault(w =>
                    {
                        try
                        {
                            return w.Properties.ProcessId.ValueOrDefault == App.ProcessId;
                        }
                        catch
                        {
                            return false;
                        }
                    });
            }
            catch
            {
                return null;
            }
        }
    }
}
