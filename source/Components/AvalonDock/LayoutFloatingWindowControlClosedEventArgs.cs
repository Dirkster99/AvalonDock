using System;
using AvalonDock.Controls;

namespace AvalonDock
{
	public sealed class LayoutFloatingWindowControlClosedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutFloatingWindowControlClosedEventArgs"/> class.
		/// </summary>
		/// <param name="layoutFloatingWindowControl"></param>
		public LayoutFloatingWindowControlClosedEventArgs(LayoutFloatingWindowControl layoutFloatingWindowControl)
		{
			LayoutFloatingWindowControl = layoutFloatingWindowControl;
		}

		public LayoutFloatingWindowControl LayoutFloatingWindowControl { get; }
	}
}