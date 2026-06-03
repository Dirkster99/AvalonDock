using System;
using AvalonDock.Controls;

namespace AvalonDock
{
	/// <summary>
	/// Provides data for the layout Floating Window Control Created event.
	/// </summary>
	public sealed class LayoutFloatingWindowControlCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutFloatingWindowControlCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="layoutFloatingWindowControl">The layout Floating Window Control.</param>
		public LayoutFloatingWindowControlCreatedEventArgs(LayoutFloatingWindowControl layoutFloatingWindowControl)
		{
			LayoutFloatingWindowControl = layoutFloatingWindowControl;
		}

		/// <summary>
		/// Gets the layout Floating Window Control.
		/// </summary>
		public LayoutFloatingWindowControl LayoutFloatingWindowControl { get; }
	}
}