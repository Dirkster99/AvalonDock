using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// macOS implementation of INativeWindowService.
	/// Uses Objective-C P/Invoke for NSWindow operations.
	/// </summary>
	internal class MacOSNativeWindowService : INativeWindowService
	{
		private const string ObjC = "/usr/lib/libobjc.dylib";
		private const string CG = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

		// Objective-C runtime imports
		[DllImport(ObjC, EntryPoint = "objc_getClass")]
		private static extern IntPtr GetClass(string name);

		[DllImport(ObjC, EntryPoint = "sel_registerName")]
		private static extern IntPtr Sel(string name);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr MsgSend(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr MsgSend(IntPtr rcv, IntPtr sel, IntPtr arg);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern void MsgSend_nint(IntPtr rcv, IntPtr sel, nint value);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern void MsgSend_double(IntPtr rcv, IntPtr sel, double value);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern void MsgSend_NSPoint(IntPtr rcv, IntPtr sel, NSPoint pt);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern NSPoint MsgSend_retNSPoint(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern double MsgSend_retDouble(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern NSRect MsgSend_retNSRect(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern NSRect MsgSend_NSRect_retNSRect(IntPtr rcv, IntPtr sel, NSRect arg);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern NSRect MsgSend_NSRect_IntPtr_retNSRect(IntPtr rcv, IntPtr sel, NSRect arg, IntPtr view);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern nuint MsgSend_retNUInt(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern nint MsgSend_retNInt(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern byte MsgSend_retBool(IntPtr rcv, IntPtr sel);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr MsgSend_nuint_retIntPtr(IntPtr rcv, IntPtr sel, nuint idx);

		// CoreGraphics imports
		[DllImport(CG)]
		private static extern uint CGMainDisplayID();

		[DllImport(CG)]
		private static extern nuint CGDisplayPixelsHigh(uint display);

		[DllImport(CG)]
		private static extern IntPtr CGEventCreate(IntPtr source);

		[DllImport(CG)]
		private static extern NSPoint CGEventGetLocation(IntPtr e);

		[DllImport(CG)]
		private static extern void CFRelease(IntPtr cf);

		// Selectors
		private static readonly IntPtr _selSharedApp = Sel("sharedApplication");
		private static readonly IntPtr _selWindows = Sel("windows");
		private static readonly IntPtr _selLastObject = Sel("lastObject");
		private static readonly IntPtr _selTabbingMode = Sel("setTabbingMode:");
		private static readonly IntPtr _selFrameTopLeft = Sel("setFrameTopLeftPoint:");
		private static readonly IntPtr _selFrame = Sel("frame");
		private static readonly IntPtr _selContentView = Sel("contentView");
		private static readonly IntPtr _selBounds = Sel("bounds");
		private static readonly IntPtr _selConvertRectToScreen = Sel("convertRectToScreen:");
		private static readonly IntPtr _selConvertRectToView = Sel("convertRect:toView:");
		private static readonly IntPtr _selSetLevel = Sel("setLevel:");
		private static readonly IntPtr _selSetAlphaValue = Sel("setAlphaValue:");
		private static readonly IntPtr _selClose = Sel("close");
		private static readonly IntPtr _selOrderFront = Sel("orderFront:");
		private static readonly IntPtr _selOrderOut = Sel("orderOut:");
		private static readonly IntPtr _selStyleMask = Sel("styleMask");
		private static readonly IntPtr _selIsVisible = Sel("isVisible");
		private static readonly IntPtr _selBackingScaleFactor = Sel("backingScaleFactor");
		private static readonly IntPtr _selScreen = Sel("screen");

		// Classes
		private static readonly IntPtr _clsNSApp = GetClass("NSApplication");
		private static readonly IntPtr _clsNSEvent = GetClass("NSEvent");
		private static readonly IntPtr _clsNSScreen = GetClass("NSScreen");

		private const nint NSWindowTabbingModeDisallowed = 2;
		private const nuint NSWindowStyleMaskTitled = 1;

		[StructLayout(LayoutKind.Sequential)]
		private struct NSPoint { public double X; public double Y; }

		[StructLayout(LayoutKind.Sequential)]
		private struct NSRect { public NSPoint Origin; public NSPoint Size; }

		public (double X, double Y) GetWindowPosition(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0);

			try
			{
				var frame = MsgSend_retNSRect(windowHandle, _selFrame);
				var screenH = (double)(long)CGDisplayPixelsHigh(CGMainDisplayID());
				var quartzY = screenH - (frame.Origin.Y + frame.Size.Y);
				return (frame.Origin.X, quartzY);
			}
			catch { return (0, 0); }
		}

		public void SetWindowPosition(IntPtr windowHandle, double x, double y)
		{
			if (windowHandle == IntPtr.Zero) return;

			try
			{
				var screenH = (double)(long)CGDisplayPixelsHigh(CGMainDisplayID());
				var cocoaY = screenH - y;
				MsgSend_NSPoint(windowHandle, _selFrameTopLeft, new NSPoint { X = x, Y = cocoaY });
			}
			catch { }
		}

		public (double X, double Y, double Width, double Height) GetWindowFrame(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0, 0, 0);

			try
			{
				var frame = MsgSend_retNSRect(windowHandle, _selFrame);
				var screenH = (double)(long)CGDisplayPixelsHigh(CGMainDisplayID());
				var quartzY = screenH - (frame.Origin.Y + frame.Size.Y);
				return (frame.Origin.X, quartzY, frame.Size.X, frame.Size.Y);
			}
			catch { return (0, 0, 0, 0); }
		}

		public void BringToFront(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return;

			try
			{
				MsgSend(windowHandle, _selOrderFront, IntPtr.Zero);
			}
			catch { }
		}

		public void CloseWindow(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return;

			try
			{
				MsgSend(windowHandle, _selClose);
			}
			catch { }
		}

		public void DisableWindowTabbing(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return;

			try
			{
				MsgSend_nint(windowHandle, _selTabbingMode, NSWindowTabbingModeDisallowed);
			}
			catch { }
		}

		public void SetWindowAlpha(IntPtr windowHandle, double alpha)
		{
			if (windowHandle == IntPtr.Zero) return;

			try
			{
				if (alpha < 0.0) alpha = 0.0;
				else if (alpha > 1.0) alpha = 1.0;
				MsgSend_double(windowHandle, _selSetAlphaValue, alpha);
			}
			catch { }
		}

		public void SetWindowLevel(IntPtr windowHandle, int level)
		{
			if (windowHandle == IntPtr.Zero) return;

			try
			{
				MsgSend_nint(windowHandle, _selSetLevel, level);
			}
			catch { }
		}

		public (double X, double Y, double Width, double Height) GetContentViewFrame(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0, 0, 0);

			try
			{
				var contentView = MsgSend(windowHandle, _selContentView);
				if (contentView == IntPtr.Zero) return (0, 0, 0, 0);
				var frame = MsgSend_retNSRect(contentView, _selFrame);
				return (frame.Origin.X, frame.Origin.Y, frame.Size.X, frame.Size.Y);
			}
			catch { return (0, 0, 0, 0); }
		}

		public (double X, double Y) GetWindowContentOrigin(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0);

			try
			{
				var contentView = MsgSend(windowHandle, _selContentView);
				if (contentView == IntPtr.Zero) return (0, 0);

				var bounds = MsgSend_retNSRect(contentView, _selBounds);
				var inWindow = MsgSend_NSRect_IntPtr_retNSRect(contentView, _selConvertRectToView, bounds, IntPtr.Zero);
				var onScreen = MsgSend_NSRect_retNSRect(windowHandle, _selConvertRectToScreen, inWindow);

				var screenH = (double)(long)CGDisplayPixelsHigh(CGMainDisplayID());
				var quartzTopY = screenH - (onScreen.Origin.Y + onScreen.Size.Y);
				return (onScreen.Origin.X, quartzTopY);
			}
			catch { return (0, 0); }
		}
	}
}