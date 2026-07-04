using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// macOS implementation of IDpiService.
	/// Uses CoreGraphics and Objective-C P/Invoke for DPI operations.
	/// </summary>
	internal class MacOSDpiService : IDpiService
	{
		private const string ObjC = "/usr/lib/libobjc.dylib";
		private const string CG = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

		// CoreGraphics imports
		[DllImport(CG)]
		private static extern uint CGMainDisplayID();

		[DllImport(CG)]
		private static extern nuint CGDisplayPixelsHigh(uint display);

		// Objective-C runtime imports
		[DllImport(ObjC, EntryPoint = "objc_getClass")]
		private static extern IntPtr GetClass(string name);

		[DllImport(ObjC, EntryPoint = "sel_registerName")]
		private static extern IntPtr Sel(string name);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern double MsgSend_retDouble(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern NSRect MsgSend_retNSRect(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr MsgSend(IntPtr rcv, IntPtr sel);

		// Selectors
		private static readonly IntPtr _selMainScreen = Sel("mainScreen");
		private static readonly IntPtr _selBackingScaleFactor = Sel("backingScaleFactor");
		private static readonly IntPtr _selFrame = Sel("frame");
		private static readonly IntPtr _selVisibleFrame = Sel("visibleFrame");

		// Classes
		private static readonly IntPtr _clsNSScreen = GetClass("NSScreen");

		[StructLayout(LayoutKind.Sequential)]
		private struct NSPoint { public double X; public double Y; }

		[StructLayout(LayoutKind.Sequential)]
		private struct NSRect { public NSPoint Origin; public NSPoint Size; }

		public double GetPrimaryMonitorDpi()
		{
			try
			{
				var mainScreen = MsgSend(_clsNSScreen, _selMainScreen);
				if (mainScreen == IntPtr.Zero) return 1.0;

				var scale = MsgSend_retDouble(mainScreen, _selBackingScaleFactor);
				return scale; // macOS uses scale factor directly (2.0 for Retina)
			}
			catch { return 1.0; }
		}

		public double GetMonitorDpi(IntPtr windowHandle)
		{
			// On macOS, all displays typically have the same scale factor
			// For simplicity, return the primary monitor DPI
			return GetPrimaryMonitorDpi();
		}

		public Rect GetPrimaryMonitorWorkArea()
		{
			try
			{
				var mainScreen = MsgSend(_clsNSScreen, _selMainScreen);
				if (mainScreen == IntPtr.Zero) return new Rect(0, 0, 1920, 1080);

				var frame = MsgSend_retNSRect(mainScreen, _selVisibleFrame);
				// Convert Cocoa coordinates (Y-up) to WPF coordinates (Y-down)
				var screenH = (double)(long)CGDisplayPixelsHigh(CGMainDisplayID());
				var wpfY = screenH - (frame.Origin.Y + frame.Size.Y);
				return new Rect(frame.Origin.X, wpfY, frame.Size.X, frame.Size.Y);
			}
			catch { return new Rect(0, 0, 1920, 1080); }
		}

		public Rect GetMonitorWorkArea(IntPtr windowHandle)
		{
			// On macOS, return the primary monitor work area
			// A more sophisticated implementation would find the monitor containing the window
			return GetPrimaryMonitorWorkArea();
		}
	}
}