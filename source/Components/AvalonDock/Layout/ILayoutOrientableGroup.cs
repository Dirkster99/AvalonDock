using System.Windows.Controls;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout orientable group.
	/// </summary>
	public interface ILayoutOrientableGroup : ILayoutGroup
	{
		/// <summary>
		/// Gets or sets the orientation.
		/// </summary>
		Orientation Orientation { get; set; }
	}
}