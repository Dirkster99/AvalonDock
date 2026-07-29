using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AvalonDock
{
	/// <summary>
	/// Provides helper members for window Helper.
	/// </summary>
	internal static class WindowHelper
	{
		/// <summary>
		/// Executes the is Attached To Presentation Source operation.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		public static bool IsAttachedToPresentationSource(this Visual element)
		{
			return PresentationSource.FromVisual(element) != null;
		}

		/// <summary>
		/// Sets the set Parent To Main Window Of.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="element">The element.</param>
		/// <returns>
		/// true if the ownership could be established; false if the window hosting <paramref name="element"/>
		/// has not been shown yet and the ownership has to be established at a later point in time.
		/// </returns>
		public static bool SetParentToMainWindowOf(this Window window, Visual element)
		{
			var wndParent = Window.GetWindow(element);
			if (wndParent != null)
			{
				// WPF refuses to own a window by a window that has never been shown and throws
				// InvalidOperationException("Cannot set Owner property to a Window that has not been shown
				// previously.") instead. This happens whenever a layout element is floated before the window
				// hosting the DockingManager is shown for the first time (issue #618).
				if (!wndParent.IsWindowHandleCreated())
					return false;

				window.Owner = wndParent;
				return true;
			}

			if (GetParentWindowHandle(element, out IntPtr parentHwnd))
			{
				Win32Helper.SetOwner(new WindowInteropHelper(window).Handle, parentHwnd);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether the native window handle of <paramref name="window"/> has already been created,
		/// which is the case as soon as the window has been shown once and until it is closed.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <returns>true if the window owns a native window handle; otherwise, false.</returns>
		public static bool IsWindowHandleCreated(this Window window)
		{
			return window != null && new WindowInteropHelper(window).Handle != IntPtr.Zero;
		}

		/// <summary>
		/// Gets the get Parent Window Handle.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <returns>The requested value.</returns>
		public static IntPtr GetParentWindowHandle(this Window window)
		{
			if (window.Owner != null)
				return new WindowInteropHelper(window.Owner).Handle;
			else
				return Win32Helper.GetOwner(new WindowInteropHelper(window).Handle);
		}

		/// <summary>
		/// Gets the get Parent Window Handle.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="hwnd">The hwnd.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		public static bool GetParentWindowHandle(this Visual element, out IntPtr hwnd)
		{
			hwnd = IntPtr.Zero;

			if (!(PresentationSource.FromVisual(element) is HwndSource wpfHandle))
				return false;

			hwnd = Win32Helper.GetParent(wpfHandle.Handle);
			if (hwnd == IntPtr.Zero)
				hwnd = wpfHandle.Handle;
			return true;
		}

		/// <summary>
		/// Sets the set Parent Window To Null.
		/// </summary>
		/// <param name="window">The window.</param>
		public static void SetParentWindowToNull(this Window window)
		{
			if (window.Owner != null)
			{
				window.Owner = null;
			}
			else
			{
				Win32Helper.SetOwner(new WindowInteropHelper(window).Handle, IntPtr.Zero);
			}
		}
	}
}