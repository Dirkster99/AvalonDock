using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Provides data for the anchorable Hiding event.
	/// </summary>
	public class AnchorableHidingEventArgs : CancelEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorableHidingEventArgs"/> class.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		public AnchorableHidingEventArgs(LayoutAnchorable anchorable)
		{
			Anchorable = anchorable;
		}

		/// <summary>
		/// Gets the anchorable.
		/// </summary>
		public LayoutAnchorable Anchorable { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether close Instead Of Hide.
		/// </summary>
		public bool CloseInsteadOfHide { get; set; }
	}
}