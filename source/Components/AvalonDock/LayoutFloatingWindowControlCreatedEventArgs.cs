using AvalonDock.Controls;
using System;

namespace AvalonDock
{
	public sealed class LayoutFloatingWindowControlCreatedEventArgs : EventArgs
	{
		public LayoutFloatingWindowControlCreatedEventArgs(LayoutFloatingWindowControl layoutFloatingWindowControl)
		{
			LayoutFloatingWindowControl = layoutFloatingWindowControl;
		}

		public LayoutFloatingWindowControl LayoutFloatingWindowControl { get; }
	}

}
