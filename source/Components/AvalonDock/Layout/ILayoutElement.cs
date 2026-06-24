using System.ComponentModel;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout elements that participate in the AvalonDock layout tree.
	/// </summary>
	public interface ILayoutElement : INotifyPropertyChanged, INotifyPropertyChanging
	{
		/// <summary>Gets the parent <see cref="ILayoutContainer"/> for this layout element.</summary>
		ILayoutContainer Parent { get; }

		/// <summary>Gets the <see cref="LayoutRoot"/> for this layout element.</summary>
		ILayoutRoot Root { get; }
	}
}