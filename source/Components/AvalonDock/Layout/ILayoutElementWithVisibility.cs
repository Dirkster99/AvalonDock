namespace AvalonDock.Layout
{
	/// <summary>Interface definition for a layout element that can update its visibility (IsVisible) property.</summary>
	public interface ILayoutElementWithVisibility
	{
		/// <summary>Invoke this to update the visibility (IsVisible) property of this layout element.</summary>
		void ComputeVisibility();
	}
}