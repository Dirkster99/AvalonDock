using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Cancelable event raised before a layout content is floated.
	/// Set <see cref="CancelEventArgs.Cancel"/> to true to prevent floating.
	/// </summary>
	public class ContentFloatingEventArgs : CancelEventArgs
	{
		/// <summary>Class constructor.</summary>
		/// <param name="content">The content that is about to be floated.</param>
		public ContentFloatingEventArgs(LayoutContent content)
		{
			Content = content;
		}

		/// <summary>Gets the layout content that is about to be floated.</summary>
		public LayoutContent Content { get; }
	}

	/// <summary>
	/// Event raised after a layout content has been floated.
	/// </summary>
	public class ContentFloatedEventArgs : System.EventArgs
	{
		/// <summary>Class constructor.</summary>
		/// <param name="content">The content that was floated.</param>
		public ContentFloatedEventArgs(LayoutContent content)
		{
			Content = content;
		}

		/// <summary>Gets the layout content that was floated.</summary>
		public LayoutContent Content { get; }
	}
}