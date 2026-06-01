namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout previous container.
	/// </summary>
	public interface ILayoutPreviousContainer
	{
		/// <summary>
		/// Gets or sets the previous container.
		/// </summary>
		ILayoutContainer PreviousContainer { get; set; }

		/// <summary>
		/// Gets or sets the previous container id.
		/// </summary>
		string PreviousContainerId { get; set; }
	}
}