﻿/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AvalonDock
{
	internal static class WindowHelper
	{
		public static bool IsAttachedToPresentationSource(this Visual element)
		{
			return PresentationSource.FromVisual(element) != null;
		}

		public static void SetParentToMainWindowOf(this Window window, Visual element)
		{
			var wndParent = Window.GetWindow(element);
			if (wndParent != null)
				window.Owner = wndParent;
			else
			{
				if (GetParentWindowHandle(element, out IntPtr parentHwnd))
					Win32Helper.SetOwner(new WindowInteropHelper(window).Handle, parentHwnd);
			}
		}

		public static IntPtr GetParentWindowHandle(this Window window)
		{
			if (window.Owner != null)
				return new WindowInteropHelper(window.Owner).Handle;
			else
				return Win32Helper.GetOwner(new WindowInteropHelper(window).Handle);
		}

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

		public static void SetParentWindowToNull(this Window window)
		{
			if (window.Owner != null)
				window.Owner = null;
			else
			{
				Win32Helper.SetOwner(new WindowInteropHelper(window).Handle, IntPtr.Zero);
			}
		}
	}
}