/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Threading;

	/// <summary>
	/// Represents the message Window.
	/// </summary>
	internal sealed class MessageWindow : DispatcherObject, IDisposable
	{
		// Alias this to a static so the wrapper doesn't get GC'd

		/// <summary>
		/// The s Wnd Proc field.
		/// </summary>
		private static readonly WndProc s_WndProc = new WndProc(_WndProc);

		/// <summary>
		/// The s window Lookup field.
		/// </summary>
		private static readonly Dictionary<IntPtr, MessageWindow> s_windowLookup = new Dictionary<IntPtr, MessageWindow>();

		/// <summary>
		/// The wnd Proc Callback field.
		/// </summary>
		private WndProc _wndProcCallback;

		/// <summary>
		/// The class Name field.
		/// </summary>
		private string _className;

		/// <summary>
		/// The is Disposed field.
		/// </summary>
		private bool _isDisposed;

		/// <summary>
		/// Gets the handle.
		/// </summary>
		public IntPtr Handle
		{
			get; private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageWindow"/> class.
		/// </summary>
		/// <param name="classStyle">The class Style.</param>
		/// <param name="style">The style.</param>
		/// <param name="exStyle">The ex Style.</param>
		/// <param name="location">The location.</param>
		/// <param name="name">The name.</param>
		/// <param name="callback">The callback.</param>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public MessageWindow(CS classStyle, WS style, WS_EX exStyle, Rect location, string name, WndProc callback)
		{
			// A null callback means just use DefWindowProc.
			_wndProcCallback = callback;
			_className = "MessageWindowClass+" + Guid.NewGuid().ToString();

			var wc = new WNDCLASSEX
			{
				cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
				style = classStyle,
				lpfnWndProc = s_WndProc,
				hInstance = NativeMethods.GetModuleHandle(null),
				hbrBackground = NativeMethods.GetStockObject(StockObject.NULL_BRUSH),
				lpszMenuName = string.Empty,
				lpszClassName = _className,
			};

			NativeMethods.RegisterClassEx(ref wc);

			var gcHandle = default(GCHandle);
			try
			{
				gcHandle = GCHandle.Alloc(this);
				var pinnedThisPtr = (IntPtr)gcHandle;

				Handle = NativeMethods.CreateWindowEx(
					exStyle,
					_className,
					name,
					style,
					(int)location.X,
					(int)location.Y,
					(int)location.Width,
					(int)location.Height,
					IntPtr.Zero,
					IntPtr.Zero,
					IntPtr.Zero,
					pinnedThisPtr);
			}
			finally
			{
				gcHandle.Free();
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="MessageWindow"/> class.
		/// </summary>
		~MessageWindow()
		{
			_Dispose(false, false);
		}

		/// <summary>
		/// Executes the dispose operation.
		/// </summary>
		public void Dispose()
		{
			_Dispose(true, false);
			GC.SuppressFinalize(this);
		}

		// This isn't right if the Dispatcher has already started shutting down.
		// It will wind up leaking the class ATOM...

		/// <summary>
		/// Executes the dispose operation.
		/// </summary>
		/// <param name="disposing">The disposing.</param>
		/// <param name="isHwndBeingDestroyed">The is Hwnd Being Destroyed.</param>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
		private void _Dispose(bool disposing, bool isHwndBeingDestroyed)
		{
			// Block against reentrancy.
			if (_isDisposed) return;
			_isDisposed = true;
			var hwnd = Handle;
			var className = _className;

			if (isHwndBeingDestroyed)
			{
				Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)(arg => _DestroyWindow(IntPtr.Zero, className)));
			}
			else if (Handle != IntPtr.Zero)
			{
				if (CheckAccess())
					_DestroyWindow(hwnd, className);
				else
					Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)(arg => _DestroyWindow(hwnd, className)));
			}

			s_windowLookup.Remove(hwnd);
			_className = null;
			Handle = IntPtr.Zero;
		}

		/// <summary>
		/// Executes the wnd Proc operation.
		/// </summary>
		/// <param name="hwnd">The hwnd.</param>
		/// <param name="msg">The msg.</param>
		/// <param name="wParam">The w Param.</param>
		/// <param name="lParam">The l Param.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		private static IntPtr _WndProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam)
		{
			var ret = IntPtr.Zero;
			MessageWindow hwndWrapper = null;

			if (msg == WM.CREATE)
			{
				var createStruct = (CREATESTRUCT)Marshal.PtrToStructure(lParam, typeof(CREATESTRUCT));
				var gcHandle = GCHandle.FromIntPtr(createStruct.lpCreateParams);
				hwndWrapper = (MessageWindow)gcHandle.Target;
				s_windowLookup.Add(hwnd, hwndWrapper);
			}
			else
			{
				if (!s_windowLookup.TryGetValue(hwnd, out hwndWrapper))
					return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
			}

			Assert.IsNotNull(hwndWrapper);

			var callback = hwndWrapper._wndProcCallback;
			if (callback != null)
				ret = callback(hwnd, msg, wParam, lParam);
			else
				ret = NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);

			if (msg == WM.NCDESTROY)
			{
				hwndWrapper._Dispose(true, true);
				GC.SuppressFinalize(hwndWrapper);
			}

			return ret;
		}

		/// <summary>
		/// Executes the destroy Window operation.
		/// </summary>
		/// <param name="hwnd">The hwnd.</param>
		/// <param name="className">The class Name.</param>
		/// <returns>The result of the operation.</returns>
		private static object _DestroyWindow(IntPtr hwnd, string className)
		{
			Utility.SafeDestroyWindow(ref hwnd);
			NativeMethods.UnregisterClass(className, NativeMethods.GetModuleHandle(null));
			return null;
		}
	}
}