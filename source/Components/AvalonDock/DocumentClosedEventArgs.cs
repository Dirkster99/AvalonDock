using System;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Provides data for the document Closed event.
	/// </summary>
	public class DocumentClosedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentClosedEventArgs"/> class.
		/// </summary>
		/// <param name="document">The document.</param>
		public DocumentClosedEventArgs(LayoutDocument document)
		{
			Document = document;
		}

		/// <summary>
		/// Gets the document.
		/// </summary>
		public LayoutDocument Document { get; private set; }
	}
}