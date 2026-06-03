using System;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Provides data for the window Activate event.
	/// </summary>
	internal class WindowActivateEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WindowActivateEventArgs"/> class.
		/// </summary>
		/// <param name="hwndActivating">The hwnd Activating.</param>
		public WindowActivateEventArgs(IntPtr hwndActivating)
		{
			HwndActivating = hwndActivating;
		}

		/// <summary>
		/// Gets the hwnd Activating.
		/// </summary>
		public IntPtr HwndActivating { get; private set; }
	}
}