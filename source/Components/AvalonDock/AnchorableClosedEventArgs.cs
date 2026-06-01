using System;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Provides data for the anchorable Closed event.
	/// </summary>
	public class AnchorableClosedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorableClosedEventArgs"/> class.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		public AnchorableClosedEventArgs(LayoutAnchorable anchorable)
		{
			Anchorable = anchorable;
		}

		/// <summary>
		/// Gets the anchorable.
		/// </summary>
		public LayoutAnchorable Anchorable { get; private set; }
	}
}