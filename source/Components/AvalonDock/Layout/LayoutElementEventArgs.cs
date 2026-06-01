using System;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides event data for layout element operations.
	/// </summary>
	public class LayoutElementEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutElementEventArgs"/> class.
		/// </summary>
		/// <param name="element">The layout element.</param>
		public LayoutElementEventArgs(LayoutElement element)
		{
			Element = element;
		}

		/// <summary>
		/// Gets the element.
		/// </summary>
		public LayoutElement Element { get; private set; }
	}
}