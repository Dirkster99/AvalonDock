using System;
using AvalonDock.Controls;

namespace AvalonDock
{
	public sealed class LayoutFloatingWindowControlClosedEventArgs : EventArgs
	{
		public LayoutFloatingWindowControlClosedEventArgs(LayoutFloatingWindowControl layoutFloatingWindowControl)
		{
			LayoutFloatingWindowControl = layoutFloatingWindowControl;
		}

		public LayoutFloatingWindowControl LayoutFloatingWindowControl { get; }
	}

}
