using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AvalonDock
{
	/// <summary>Represents the Win32Helper class.</summary>
	internal static class Win32Helper
	{
		/// <summary>The WS_CHILD value.</summary>
		internal const int WS_CHILD = 0x40000000;

		/// <summary>The WS_VISIBLE value.</summary>
		internal const int WS_VISIBLE = 0x10000000;

		/// <summary>The WS_VSCROLL value.</summary>
		internal const int WS_VSCROLL = 0x00200000;

		/// <summary>The WS_BORDER value.</summary>
		internal const int WS_BORDER = 0x00800000;

		/// <summary>The WS_CLIPSIBLINGS value.</summary>
		internal const int WS_CLIPSIBLINGS = 0x04000000;

		/// <summary>The WS_CLIPCHILDREN value.</summary>
		internal const int WS_CLIPCHILDREN = 0x02000000;

		/// <summary>The WS_TABSTOP value.</summary>
		internal const int WS_TABSTOP = 0x00010000;

		/// <summary>The WS_GROUP value.</summary>
		internal const int WS_GROUP = 0x00020000;

		/// <summary>Defines SetWindowPosFlags values.</summary>
		[Flags]
		internal enum SetWindowPosFlags : uint
		{
			/// <summary>The SynchronousWindowPosition value.</summary>
			SynchronousWindowPosition = 0x4000,

			/// <summary>The DeferErase value.</summary>
			DeferErase = 0x2000,

			/// <summary>The DrawFrame value.</summary>
			DrawFrame = 0x0020,

			/// <summary>The FrameChanged value.</summary>
			FrameChanged = 0x0020,

			/// <summary>The HideWindow value.</summary>
			HideWindow = 0x0080,

			/// <summary>The DoNotActivate value.</summary>
			DoNotActivate = 0x0010,

			/// <summary>The DoNotCopyBits value.</summary>
			DoNotCopyBits = 0x0100,

			/// <summary>The IgnoreMove value.</summary>
			IgnoreMove = 0x0002,

			/// <summary>The DoNotChangeOwnerZOrder value.</summary>
			DoNotChangeOwnerZOrder = 0x0200,

			/// <summary>The DoNotRedraw value.</summary>
			DoNotRedraw = 0x0008,

			/// <summary>The DoNotReposition value.</summary>
			DoNotReposition = 0x0200,

			/// <summary>The DoNotSendChangingEvent value.</summary>
			DoNotSendChangingEvent = 0x0400,

			/// <summary>The IgnoreResize value.</summary>
			IgnoreResize = 0x0001,

			/// <summary>The IgnoreZOrder value.</summary>
			IgnoreZOrder = 0x0004,

			/// <summary>The ShowWindow value.</summary>
			ShowWindow = 0x0040,
		}

		/// <summary>The HWND_TOPMOST value.</summary>
		internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

		/// <summary>The HWND_NOTOPMOST value.</summary>
		internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

		/// <summary>The HWND_TOP value.</summary>
		internal static readonly IntPtr HWND_TOP = new IntPtr(0);

		/// <summary>The HWND_BOTTOM value.</summary>
		internal static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		/// <summary>Represents the WINDOWPOS class.</summary>
		[StructLayout(LayoutKind.Sequential)]
		internal class WINDOWPOS
		{
			/// <summary>The Hwnd value.</summary>
			public IntPtr Hwnd;

			/// <summary>The HwndInsertAfter value.</summary>
			public IntPtr HwndInsertAfter;

			/// <summary>The X value.</summary>
			public int X;

			/// <summary>The Y value.</summary>
			public int Y;

			/// <summary>The Cx value.</summary>
			public int Cx;

			/// <summary>The Cy value.</summary>
			public int Cy;

			/// <summary>The Flags value.</summary>
			public int Flags;
		}

		/// <summary>Performs the SetWindowPos operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="hWndInsertAfter">The hWndInsertAfter value.</param>
		/// <param name="X">The X value.</param>
		/// <param name="Y">The Y value.</param>
		/// <param name="cx">The cx value.</param>
		/// <param name="cy">The cy value.</param>
		/// <param name="uFlags">The uFlags value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		/// <summary>Performs the IsChild operation.</summary>
		/// <param name="hWndParent">The hWndParent value.</param>
		/// <param name="hwnd">The hwnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern bool IsChild(IntPtr hWndParent, IntPtr hwnd);

		/// <summary>Performs the SetFocus operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		internal static extern IntPtr SetFocus(IntPtr hWnd);

		/// <summary>The WM_WINDOWPOSCHANGED value.</summary>
		internal const int WM_WINDOWPOSCHANGED = 0x0047;

		/// <summary>The WM_WINDOWPOSCHANGING value.</summary>
		internal const int WM_WINDOWPOSCHANGING = 0x0046;

		/// <summary>The WM_NCMOUSEMOVE value.</summary>
		internal const int WM_NCMOUSEMOVE = 0xa0;

		/// <summary>The WM_NCLBUTTONDOWN value.</summary>
		internal const int WM_NCLBUTTONDOWN = 0xA1;

		/// <summary>The WM_NCLBUTTONUP value.</summary>
		internal const int WM_NCLBUTTONUP = 0xA2;

		/// <summary>The WM_NCLBUTTONDBLCLK value.</summary>
		internal const int WM_NCLBUTTONDBLCLK = 0xA3;

		/// <summary>The WM_NCRBUTTONDOWN value.</summary>
		internal const int WM_NCRBUTTONDOWN = 0xA4;

		/// <summary>The WM_NCRBUTTONUP value.</summary>
		internal const int WM_NCRBUTTONUP = 0xA5;

		/// <summary>The WM_CAPTURECHANGED value.</summary>
		internal const int WM_CAPTURECHANGED = 0x0215;

		/// <summary>The WM_EXITSIZEMOVE value.</summary>
		internal const int WM_EXITSIZEMOVE = 0x0232;

		/// <summary>The WM_ENTERSIZEMOVE value.</summary>
		internal const int WM_ENTERSIZEMOVE = 0x0231;

		/// <summary>The WM_MOVE value.</summary>
		internal const int WM_MOVE = 0x0003;

		/// <summary>The WM_MOVING value.</summary>
		internal const int WM_MOVING = 0x0216;

		/// <summary>The WM_KILLFOCUS value.</summary>
		internal const int WM_KILLFOCUS = 0x0008;

		/// <summary>The WM_SETFOCUS value.</summary>
		internal const int WM_SETFOCUS = 0x0007;

		/// <summary>The WM_ACTIVATE value.</summary>
		internal const int WM_ACTIVATE = 0x0006;

		/// <summary>The WM_NCHITTEST value.</summary>
		internal const int WM_NCHITTEST = 0x0084;

		/// <summary>The WM_INITMENUPOPUP value.</summary>
		internal const int WM_INITMENUPOPUP = 0x0117;

		/// <summary>The WM_KEYDOWN value.</summary>
		internal const int WM_KEYDOWN = 0x0100;

		/// <summary>The WM_KEYUP value.</summary>
		internal const int WM_KEYUP = 0x0101;

		/// <summary>The WM_CLOSE value.</summary>
		internal const int WM_CLOSE = 0x10;

		/// <summary>The WA_INACTIVE value.</summary>
		internal const int WA_INACTIVE = 0x0000;

		/// <summary>The WM_SYSCOMMAND value.</summary>
		internal const int WM_SYSCOMMAND = 0x0112;

		// These are the wParam of WM_SYSCOMMAND

		/// <summary>The SC_MAXIMIZE value.</summary>
		internal const int SC_MAXIMIZE = 0xF030;

		/// <summary>The SC_RESTORE value.</summary>
		internal const int SC_RESTORE = 0xF120;

		/// <summary>The WM_CREATE value.</summary>
		internal const int
			WM_CREATE = 0x0001;

		/// <summary>Performs the SetActiveWindow operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetActiveWindow(IntPtr hWnd);

		/// <summary>Performs the DestroyWindow operation.</summary>
		/// <param name="hwnd">The hwnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
		internal static extern bool DestroyWindow(IntPtr hwnd);

		/// <summary>The HT_CAPTION value.</summary>
		internal const int HT_CAPTION = 0x2;

		/// <summary>Performs the SendMessage operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="Msg">The Msg value.</param>
		/// <param name="wParam">The wParam value.</param>
		/// <param name="lParam">The lParam value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImportAttribute("user32.dll")]
		internal static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>Performs the PostMessage operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="Msg">The Msg value.</param>
		/// <param name="wParam">The wParam value.</param>
		/// <param name="lParam">The lParam value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImportAttribute("user32.dll")]
		internal static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>Performs the GetClientRect operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="lpRect">The lpRect value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		/// <summary>Performs the GetWindowRect operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="lpRect">The lpRect value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		// Hook Types

		/// <summary>Defines HookType values.</summary>
		public enum HookType : int
		{
			/// <summary>The WH_JOURNALRECORD value.</summary>
			WH_JOURNALRECORD = 0,

			/// <summary>The WH_JOURNALPLAYBACK value.</summary>
			WH_JOURNALPLAYBACK = 1,

			/// <summary>The WH_KEYBOARD value.</summary>
			WH_KEYBOARD = 2,

			/// <summary>The WH_GETMESSAGE value.</summary>
			WH_GETMESSAGE = 3,

			/// <summary>The WH_CALLWNDPROC value.</summary>
			WH_CALLWNDPROC = 4,

			/// <summary>The WH_CBT value.</summary>
			WH_CBT = 5,

			/// <summary>The WH_SYSMSGFILTER value.</summary>
			WH_SYSMSGFILTER = 6,

			/// <summary>The WH_MOUSE value.</summary>
			WH_MOUSE = 7,

			/// <summary>The WH_HARDWARE value.</summary>
			WH_HARDWARE = 8,

			/// <summary>The WH_DEBUG value.</summary>
			WH_DEBUG = 9,

			/// <summary>The WH_SHELL value.</summary>
			WH_SHELL = 10,

			/// <summary>The WH_FOREGROUNDIDLE value.</summary>
			WH_FOREGROUNDIDLE = 11,

			/// <summary>The WH_CALLWNDPROCRET value.</summary>
			WH_CALLWNDPROCRET = 12,

			/// <summary>The WH_KEYBOARD_LL value.</summary>
			WH_KEYBOARD_LL = 13,

			/// <summary>The WH_MOUSE_LL value.</summary>
			WH_MOUSE_LL = 14
		}

		/// <summary>The HCBT_SETFOCUS value.</summary>
		public const int HCBT_SETFOCUS = 9;

		/// <summary>The HCBT_ACTIVATE value.</summary>
		public const int HCBT_ACTIVATE = 5;

		/// <summary>Performs the GetCurrentThreadId operation.</summary>
		/// <returns>The result of the operation.</returns>
		[DllImport("kernel32.dll")]
		public static extern uint GetCurrentThreadId();

		/// <summary>Represents the HookProc callback.</summary>
		/// <param name="code">The code value.</param>
		/// <param name="wParam">The wParam value.</param>
		/// <param name="lParam">The lParam value.</param>
		/// <returns>The result of the operation.</returns>
		public delegate int HookProc(int code, IntPtr wParam,
			IntPtr lParam);

		/// <summary>Performs the SetWindowsHookEx operation.</summary>
		/// <param name="code">The code value.</param>
		/// <param name="func">The func value.</param>
		/// <param name="hInstance">The hInstance value.</param>
		/// <param name="threadID">The threadID value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		public static extern IntPtr SetWindowsHookEx(
			HookType code,
			HookProc func,
			IntPtr hInstance,
			int threadID);

		/// <summary>Performs the UnhookWindowsHookEx operation.</summary>
		/// <param name="hhook">The hhook value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		public static extern int UnhookWindowsHookEx(IntPtr hhook);

		/// <summary>Performs the CallNextHookEx operation.</summary>
		/// <param name="hhook">The hhook value.</param>
		/// <param name="code">The code value.</param>
		/// <param name="wParam">The wParam value.</param>
		/// <param name="lParam">The lParam value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		public static extern int CallNextHookEx(
			IntPtr hhook,
			int code, IntPtr wParam, IntPtr lParam);

		/// <summary>Represents the RECT structure.</summary>
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT
		{
			/// <summary>The Left value.</summary>
			public int Left;

			/// <summary>The Top value.</summary>
			public int Top;

			/// <summary>The Right value.</summary>
			public int Right;

			/// <summary>The Bottom value.</summary>
			public int Bottom;

			/// <summary>Initializes a new instance of the <see cref="RECT"/> struct.</summary>
			/// <param name="left_">The left_ value.</param>
			/// <param name="top_">The top_ value.</param>
			/// <param name="right_">The right_ value.</param>
			/// <param name="bottom_">The bottom_ value.</param>
			public RECT(int left_, int top_, int right_, int bottom_)
			{
				Left = left_;
				Top = top_;
				Right = right_;
				Bottom = bottom_;
			}

			/// <summary>Gets the Height value.</summary>
			public int Height
			{
				get
				{
					return Bottom - Top;
				}
			}

			/// <summary>Gets the Width value.</summary>
			public int Width
			{
				get
				{
					return Right - Left;
				}
			}

			/// <summary>Gets the Size value.</summary>
			public Size Size
			{
				get
				{
					return new Size(Width, Height);
				}
			}

			/// <summary>Gets the Location value.</summary>
			public Point Location
			{
				get
				{
					return new Point(Left, Top);
				}
			}

			// Handy method for converting to a System.Drawing.Rectangle

			/// <summary>Performs the ToRectangle operation.</summary>
			/// <returns>The result of the operation.</returns>
			public Rect ToRectangle()
			{
				return new Rect(Left, Top, Right, Bottom);
			}

			/// <summary>Performs the FromRectangle operation.</summary>
			/// <param name="rectangle">The rectangle value.</param>
			/// <returns>The result of the operation.</returns>
			public static RECT FromRectangle(Rect rectangle)
			{
				return new Rect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
			}

			/// <summary>Performs the GetHashCode operation.</summary>
			/// <returns>The result of the operation.</returns>
			public override int GetHashCode()
			{
				return Left ^ ((Top << 13) | (Top >> 0x13)) ^ ((Width << 0x1a) | (Width >> 6)) ^ ((Height << 7) | (Height >> 0x19));
			}

			/// <summary>Converts the value.</summary>
			/// <param name="rect">The rect value.</param>
			/// <returns>The result of the operation.</returns>
			public static implicit operator Rect(RECT rect)
			{
				return rect.ToRectangle();
			}

			/// <summary>Converts the value.</summary>
			/// <param name="rect">The rect value.</param>
			/// <returns>The result of the operation.</returns>
			public static implicit operator RECT(Rect rect)
			{
				return FromRectangle(rect);
			}
		}

		/// <summary>Performs the GetClientRect operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		internal static RECT GetClientRect(IntPtr hWnd)
		{
			GetClientRect(hWnd, out RECT result);
			return result;
		}

		/// <summary>Performs the GetWindowRect operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		internal static RECT GetWindowRect(IntPtr hWnd)
		{
			GetWindowRect(hWnd, out RECT result);
			return result;
		}

		/// <summary>Performs the GetTopWindow operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		internal static extern IntPtr GetTopWindow(IntPtr hWnd);

		/// <summary>Performs the GetWindow operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="uCmd">The uCmd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		/// <summary>Defines GetWindow_Cmd values.</summary>
		internal enum GetWindow_Cmd : uint
		{
			/// <summary>The GW_HWNDFIRST value.</summary>
			GW_HWNDFIRST = 0,

			/// <summary>The GW_HWNDLAST value.</summary>
			GW_HWNDLAST = 1,

			/// <summary>The GW_HWNDNEXT value.</summary>
			GW_HWNDNEXT = 2,

			/// <summary>The GW_HWNDPREV value.</summary>
			GW_HWNDPREV = 3,

			/// <summary>The GW_OWNER value.</summary>
			GW_OWNER = 4,

			/// <summary>The GW_CHILD value.</summary>
			GW_CHILD = 5,

			/// <summary>The GW_ENABLEDPOPUP value.</summary>
			GW_ENABLEDPOPUP = 6
		}

		/// <summary>Performs the GetWindowZOrder operation.</summary>
		/// <param name="hwnd">The hwnd value.</param>
		/// <param name="zOrder">The zOrder value.</param>
		/// <returns>The result of the operation.</returns>
		public static bool GetWindowZOrder(IntPtr hwnd, out int zOrder)
		{
			var lowestHwnd = GetWindow(hwnd, (uint)GetWindow_Cmd.GW_HWNDLAST);

			var z = 0;
			var hwndTmp = lowestHwnd;
			while (hwndTmp != IntPtr.Zero)
			{
				if (hwnd == hwndTmp)
				{
					zOrder = z;
					return true;
				}

				hwndTmp = GetWindow(hwndTmp, (uint)GetWindow_Cmd.GW_HWNDPREV);
				z++;
			}

			zOrder = int.MinValue;
			return false;
		}

		/// <summary>Performs the MakeLParam operation.</summary>
		/// <param name="LoWord">The LoWord value.</param>
		/// <param name="HiWord">The HiWord value.</param>
		/// <returns>The result of the operation.</returns>
		internal static int MakeLParam(int LoWord, int HiWord)
		{
			return (int)((HiWord << 16) | (LoWord & 0xffff));
		}

		/// <summary>The WM_MOUSEMOVE value.</summary>
		internal const int WM_MOUSEMOVE = 0x200;

		/// <summary>The WM_LBUTTONDOWN value.</summary>
		internal const int WM_LBUTTONDOWN = 0x201;

		/// <summary>The WM_LBUTTONUP value.</summary>
		internal const int WM_LBUTTONUP = 0x202;

		/// <summary>The WM_LBUTTONDBLCLK value.</summary>
		internal const int WM_LBUTTONDBLCLK = 0x203;

		/// <summary>The WM_RBUTTONDOWN value.</summary>
		internal const int WM_RBUTTONDOWN = 0x204;

		/// <summary>The WM_RBUTTONUP value.</summary>
		internal const int WM_RBUTTONUP = 0x205;

		/// <summary>The WM_RBUTTONDBLCLK value.</summary>
		internal const int WM_RBUTTONDBLCLK = 0x206;

		/// <summary>The WM_MBUTTONDOWN value.</summary>
		internal const int WM_MBUTTONDOWN = 0x207;

		/// <summary>The WM_MBUTTONUP value.</summary>
		internal const int WM_MBUTTONUP = 0x208;

		/// <summary>The WM_MBUTTONDBLCLK value.</summary>
		internal const int WM_MBUTTONDBLCLK = 0x209;

		/// <summary>The WM_MOUSEWHEEL value.</summary>
		internal const int WM_MOUSEWHEEL = 0x20A;

		/// <summary>The WM_MOUSEHWHEEL value.</summary>
		internal const int WM_MOUSEHWHEEL = 0x20E;

		/// <summary>Performs the GetCursorPos operation.</summary>
		/// <param name="pt">The pt value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		/// <summary>Represents the Win32Point structure.</summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct Win32Point
		{
			/// <summary>The X value.</summary>
			public int X;

			/// <summary>The Y value.</summary>
			public int Y;
		}

		/// <summary>Performs the GetMousePosition operation.</summary>
		/// <returns>The result of the operation.</returns>
		internal static Point GetMousePosition()
		{
			Win32Point w32Mouse = new Win32Point();
			GetCursorPos(ref w32Mouse);
			return new Point(w32Mouse.X, w32Mouse.Y);
		}

		/// <summary>Performs the IsWindowVisible operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindowVisible(IntPtr hWnd);

		/// <summary>Performs the IsWindowEnabled operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindowEnabled(IntPtr hWnd);

		/// <summary>Performs the GetFocus operation.</summary>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		internal static extern IntPtr GetFocus();

		/// <summary>Performs the BringWindowToTop operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool BringWindowToTop(IntPtr hWnd);

		/// <summary>Performs the SetParent operation.</summary>
		/// <param name="hWndChild">The hWndChild value.</param>
		/// <param name="hWndNewParent">The hWndNewParent value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		/// <summary>Performs the GetParent operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern IntPtr GetParent(IntPtr hWnd);

		/// <summary>Performs the GetWindowLong32 operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="nIndex">The nIndex value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

		/// <summary>Performs the GetWindowLongPtr64 operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="nIndex">The nIndex value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
		private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

		/// <summary>Performs the GetWindowLongPtr operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="nIndex">The nIndex value.</param>
		/// <returns>The result of the operation.</returns>
		internal static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) 
		{
			return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));
		}

		/// <summary>Performs the SetWindowLong32 operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="nIndex">The nIndex value.</param>
		/// <param name="dwNewLong">The dwNewLong value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

		/// <summary>Performs the SetWindowLongPtr64 operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="nIndex">The nIndex value.</param>
		/// <param name="dwNewLong">The dwNewLong value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		/// <summary>Performs the SetWindowLongPtr operation.</summary>
		/// <param name="hWnd">The hWnd value.</param>
		/// <param name="nIndex">The nIndex value.</param>
		/// <param name="dwNewLong">The dwNewLong value.</param>
		/// <returns>The result of the operation.</returns>
		internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) 
		{
			return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
		}

		/// <summary>Performs the SetOwner operation.</summary>
		/// <param name="childHandle">The childHandle value.</param>
		/// <param name="ownerHandle">The ownerHandle value.</param>
		public static void SetOwner(IntPtr childHandle, IntPtr ownerHandle)
		{
			SetWindowLongPtr(
				childHandle,
				-8, // GWL_HWNDPARENT
				ownerHandle);
		}

		/// <summary>Performs the GetOwner operation.</summary>
		/// <param name="childHandle">The childHandle value.</param>
		/// <returns>The result of the operation.</returns>
		public static IntPtr GetOwner(IntPtr childHandle)
		{
			return GetWindowLongPtr(childHandle, -8);
		}

		// Monitor Patch #13440

		/// <summary>Performs the MonitorFromRect operation.</summary>
		/// <param name="lprc">The lprc value.</param>
		/// <param name="dwFlags">The dwFlags value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

		/// <summary>Performs the MonitorFromWindow operation.</summary>
		/// <param name="hwnd">The hwnd value.</param>
		/// <param name="dwFlags">The dwFlags value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		/// <summary>Represents the MonitorInfo class.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public class MonitorInfo
		{
			/// <summary>The Size value.</summary>
			public int Size = Marshal.SizeOf(typeof(MonitorInfo));

			/// <summary>The Monitor value.</summary>
			public RECT Monitor;

			/// <summary>The Work value.</summary>
			public RECT Work;

			/// <summary>The Flags value.</summary>
			public uint Flags;
		}

		/// <summary>Performs the GetMonitorInfo operation.</summary>
		/// <param name="hMonitor">The hMonitor value.</param>
		/// <param name="lpmi">The lpmi value.</param>
		/// <returns>The result of the operation.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out] MonitorInfo lpmi);
	}
}