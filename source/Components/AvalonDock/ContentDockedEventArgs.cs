using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Cancelable event raised before a layout content is docked (re-docked from floating).
	/// Set <see cref="CancelEventArgs.Cancel"/> to true to prevent docking.
	/// </summary>
	public class ContentDockingEventArgs : CancelEventArgs
	{
		/// <summary>Class constructor.</summary>
		/// <param name="content">The content that is about to be docked.</param>
		public ContentDockingEventArgs(LayoutContent content)
		{
			Content = content;
		}

		/// <summary>Gets the layout content that is about to be docked.</summary>
		public LayoutContent Content { get; }
	}

	/// <summary>
	/// Event raised after a layout content has been docked.
	/// </summary>
	public class ContentDockedEventArgs : System.EventArgs
	{
		/// <summary>Class constructor.</summary>
		/// <param name="content">The content that was docked.</param>
		public ContentDockedEventArgs(LayoutContent content)
		{
			Content = content;
		}

		/// <summary>Gets the layout content that was docked.</summary>
		public LayoutContent Content { get; }
	}
}