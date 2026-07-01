using System.Windows;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout element that holds a WPF UI content element
	/// </summary>
	public interface ILayoutContentElement
	{
		/// <summary>
		/// Gets the root WPF FrameworkElement hosted by this layout element
		/// </summary>
		FrameworkElement Content { get; }
	}
}