using System;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the focus change event args.
	/// </summary>
	internal class FocusChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FocusChangeEventArgs"/> class.
		/// </summary>
		/// <param name="gotFocusWinHandle">The got focus win handle.</param>
		/// <param name="lostFocusWinHandle">The lost focus win handle.</param>
		public FocusChangeEventArgs(IntPtr gotFocusWinHandle, IntPtr lostFocusWinHandle)
		{
			GotFocusWinHandle = gotFocusWinHandle;
			LostFocusWinHandle = lostFocusWinHandle;
		}

		/// <summary>
		/// Gets the got focus win handle.
		/// </summary>
		public IntPtr GotFocusWinHandle { get; private set; }

		/// <summary>
		/// Gets the lost focus win handle.
		/// </summary>
		public IntPtr LostFocusWinHandle { get; private set; }
	}

	/// <summary>
	/// Represents the window hook handler.
	/// </summary>
	internal class WindowHookHandler
	{
		private IntPtr _windowHook;
		private Win32Helper.HookProc _hookProc;
		private ReentrantFlag _insideActivateEvent = new ReentrantFlag();

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowHookHandler"/> class.
		/// </summary>
		public WindowHookHandler()
		{
		}

		/// <summary>
		/// Occurs when focus changed.
		/// </summary>
		public event EventHandler<FocusChangeEventArgs> FocusChanged;

		/// <summary>
		/// Attaches the handler.
		/// </summary>
		public void Attach()
		{
			_hookProc = new Win32Helper.HookProc(this.HookProc);
			_windowHook = Win32Helper.SetWindowsHookEx(
				Win32Helper.HookType.WH_CBT,
				_hookProc,
				IntPtr.Zero,
				(int)Win32Helper.GetCurrentThreadId());
		}

		/// <summary>
		/// Detaches the handler.
		/// </summary>
		public void Detach()
		{
			Win32Helper.UnhookWindowsHookEx(_windowHook);
		}

		/// <summary>
		/// Hook proc.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="wParam">The w param.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns>The hook proc.</returns>
		public int HookProc(int code, IntPtr wParam, IntPtr lParam)
		{
			if (code == Win32Helper.HCBT_SETFOCUS)
			{
				if (FocusChanged != null)
					FocusChanged(this, new FocusChangeEventArgs(wParam, lParam));
			}
			else if (code == Win32Helper.HCBT_ACTIVATE)
			{
				if (_insideActivateEvent.CanEnter)
				{
					using (_insideActivateEvent.Enter())
					{
						// if (Activate != null)
						//    Activate(this, new WindowActivateEventArgs(wParam));
					}
				}
			}

			return Win32Helper.CallNextHookEx(_windowHook, code, wParam, lParam);
		}
	}
}