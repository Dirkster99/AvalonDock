namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout panel element.
	/// </summary>
	public interface ILayoutPanelElement : ILayoutElement
	{
		/// <summary>
		/// Gets a value indicating whether this instance is visible.
		/// </summary>
		bool IsVisible { get; }
	}
}