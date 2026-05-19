using System;
using AvalonDock.Controls;

namespace AvalonDock
{
	public sealed class LayoutFloatingWindowControlCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutFloatingWindowControlCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="layoutFloatingWindowControl"></param>
		public LayoutFloatingWindowControlCreatedEventArgs(LayoutFloatingWindowControl layoutFloatingWindowControl)
		{
			LayoutFloatingWindowControl = layoutFloatingWindowControl;
		}

		public LayoutFloatingWindowControl LayoutFloatingWindowControl { get; }
	}
}