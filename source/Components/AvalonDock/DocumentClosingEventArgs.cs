using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Provides data for the document Closing event.
	/// </summary>
	public class DocumentClosingEventArgs : CancelEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentClosingEventArgs"/> class.
		/// </summary>
		/// <param name="document">The document.</param>
		public DocumentClosingEventArgs(LayoutDocument document)
		{
			Document = document;
		}

		/// <summary>
		/// Gets the document.
		/// </summary>
		public LayoutDocument Document { get; private set; }
	}
}