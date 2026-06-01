using System.ComponentModel;
using System.Windows;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the drop Target Base.
	/// </summary>
	internal abstract class DropTargetBase : DependencyObject
	{
		/// <summary>
		/// IsDraggingOver attached dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDraggingOverProperty = DependencyProperty.RegisterAttached("IsDraggingOver", typeof(bool), typeof(DropTargetBase),
				new FrameworkPropertyMetadata((bool)false));

		/// <summary>
		/// Gets the get Is Dragging Over.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		[Bindable(true)]
		[Description("Gets wether the user is dragging a window over the target element.")]
		[Category("Other")]
		public static bool GetIsDraggingOver(DependencyObject d)
		{
			return (bool)d.GetValue(IsDraggingOverProperty);
		}

		/// <summary>
		/// Sets the set Is Dragging Over.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="value">The value.</param>
		public static void SetIsDraggingOver(DependencyObject d, bool value)
		{
			d.SetValue(IsDraggingOverProperty, value);
		}
	}
}