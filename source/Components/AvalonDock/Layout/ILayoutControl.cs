namespace AvalonDock.Layout
{
	/// <summary>Defines a control class that hosts a <see cref="ILayoutElement"/> as its model</summary>
	public interface ILayoutControl
	{
		/// <summary>Gets the <see cref="ILayoutElement"/> model for this control.</summary>
		ILayoutElement Model
		{
			get;
		}
	}
}