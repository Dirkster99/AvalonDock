using System.ComponentModel;
using System.Windows;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Defines the drop Area Type values.
	/// </summary>
	public enum DropAreaType
	{
		/// <summary>
		/// The docking Manager option.
		/// </summary>
		DockingManager,

		/// <summary>
		/// The document Pane option.
		/// </summary>
		DocumentPane,

		/// <summary>
		/// The document Pane Group option.
		/// </summary>
		DocumentPaneGroup,

		/// <summary>
		/// The anchorable Pane option.
		/// </summary>
		AnchorablePane,
	}

	/// <summary>
	/// Defines the contract for drop Area.
	/// </summary>
	public interface IDropArea
	{
		/// <summary>
		/// Gets the detection Rect.
		/// </summary>
		Rect DetectionRect { get; }

		/// <summary>
		/// Gets the type.
		/// </summary>
		DropAreaType Type { get; }

		/// <summary>
		/// Executes the transform To Device DPI operation.
		/// </summary>
		/// <param name="dragPosition">The drag Position.</param>
		/// <returns>The result of the operation.</returns>
		Point TransformToDeviceDPI(Point dragPosition);
	}

	/// <summary>
	/// Represents the drop Area.
	/// </summary>
	/// <typeparam name="T">The t type.</typeparam>
	public class DropArea<T> : IDropArea
		where T : FrameworkElement
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DropArea{T}"/> class.
		/// </summary>
		/// <param name="areaElement">The area Element.</param>
		/// <param name="type">The type.</param>
		internal DropArea(T areaElement, DropAreaType type)
		{
			AreaElement = areaElement;
			DetectionRect = areaElement.GetScreenArea();
			Type = type;
		}

		/// <summary>
		/// Gets the detection Rect.
		/// </summary>
		public Rect DetectionRect { get; }

		/// <summary>
		/// Gets the type.
		/// </summary>
		public DropAreaType Type { get; }

		/// <summary>
		/// Executes the transform To Device DPI operation.
		/// </summary>
		/// <param name="dragPosition">The drag Position.</param>
		/// <returns>The result of the operation.</returns>
		public Point TransformToDeviceDPI(Point dragPosition)
		{
			return AreaElement.TransformToDeviceDPI(dragPosition);
		}

		/// <summary>
		/// Gets the area Element.
		/// </summary>
		[Bindable(false)]
		[Description("Gets the FrameworkElement that implements a drop target for a drag & drop (dock) operation.")]
		[Category("Other")]
		public T AreaElement { get; }
	}
}