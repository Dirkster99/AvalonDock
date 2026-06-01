using System.Windows;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for adjustable size layout.
	/// </summary>
	public interface IAdjustableSizeLayout
	{
		/// <summary>
		/// Executes the adjust fixed children panel sizes operation.
		/// </summary>
		/// <param name="parentSize">The parent size.</param>
		void AdjustFixedChildrenPanelSizes(Size? parentSize = null);
	}
}