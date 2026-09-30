using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// macOS implementation of ICursorService.
	/// Uses CoreGraphics and Objective-C P/Invoke for cursor operations.
	/// </summary>
	internal class MacOSCursorService : ICursorService
	{
		private const string ObjC = "/usr/lib/libobjc.dylib";
		private const string CG = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

		// CoreGraphics imports
		[DllImport(CG)]
		private static extern IntPtr CGEventCreate(IntPtr source);

		[DllImport(CG)]
		private static extern NSPoint CGEventGetLocation(IntPtr e);

		[DllImport(CG)]
		private static extern void CFRelease(IntPtr cf);

		[DllImport(CG)]
		private static extern bool CGEventSourceButtonState(int stateID, uint button);

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
		private static extern NSPoint MsgSend_retNSPoint(IntPtr rcv, IntPtr sel);

		// Selectors
		private static readonly IntPtr _selMouseLocation = Sel("mouseLocation");

		// Classes
		private static readonly IntPtr _clsNSEvent = GetClass("NSEvent");

		private const int CombinedSession = 0;
		private const uint LeftButton = 0;

		[StructLayout(LayoutKind.Sequential)]
		private struct NSPoint { public double X; public double Y; }

		public (double X, double Y) GetCursorPosition()
		{
			return GetCursorLocationQuartz();
		}

		public bool IsLeftButtonDown()
		{
			return CGEventSourceButtonState(CombinedSession, LeftButton);
		}

		public (double X, double Y) GetCursorLocationQuartz()
		{
			var evt = CGEventCreate(IntPtr.Zero);
			if (evt == IntPtr.Zero) return (0, 0);
			try
			{
				var pt = CGEventGetLocation(evt);
				return (pt.X, pt.Y);
			}
			catch { return (0, 0); }
			finally { CFRelease(evt); }
		}

		public (double X, double Y) GetCursorLocationCocoa()
		{
			try
			{
				var pt = MsgSend_retNSPoint(_clsNSEvent, _selMouseLocation);
				return (pt.X, pt.Y);
			}
			catch { return (0, 0); }
		}
	}
}