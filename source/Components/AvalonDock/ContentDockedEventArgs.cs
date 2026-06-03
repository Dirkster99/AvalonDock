using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Provides data for the content Docking event.
	/// </summary>
	public class ContentDockingEventArgs : CancelEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentDockingEventArgs"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		public ContentDockingEventArgs(LayoutContent content)
		{
			Content = content;
		}

		/// <summary>
		/// Gets the content.
		/// </summary>
		public LayoutContent Content { get; }
	}

	/// <summary>
	/// Provides data for the content Docked event.
	/// </summary>
	public class ContentDockedEventArgs : System.EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentDockedEventArgs"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		public ContentDockedEventArgs(LayoutContent content)
		{
			Content = content;
		}

		/// <summary>
		/// Gets the content.
		/// </summary>
		public LayoutContent Content { get; }
	}
}