using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Grid Resizer Control.
	/// </summary>
	public class LayoutGridResizerControl : Thumb
	{
		/// <summary>
		/// Initializes static members of the <see cref="LayoutGridResizerControl"/> class.
		/// </summary>
		static LayoutGridResizerControl()
		{
			// This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			// This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(typeof(LayoutGridResizerControl)));
			HorizontalAlignmentProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
			VerticalAlignmentProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
			BackgroundProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(Brushes.Transparent));
			IsHitTestVisibleProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(true, null));
		}

		/// <summary>
		/// <see cref="BackgroundWhileDragging"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty BackgroundWhileDraggingProperty = DependencyProperty.Register(nameof(BackgroundWhileDragging), typeof(Brush), typeof(LayoutGridResizerControl),
				new FrameworkPropertyMetadata(Brushes.Black));

		/// <summary>
		/// Gets or sets the background While Dragging.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the background brush of the control being dragged.")]
		[Category("Other")]
		public Brush BackgroundWhileDragging
		{
			get => (Brush)GetValue(BackgroundWhileDraggingProperty);
			set => SetValue(BackgroundWhileDraggingProperty, value);
		}

		/// <summary>
		/// <see cref="OpacityWhileDragging"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty OpacityWhileDraggingProperty = DependencyProperty.Register(nameof(OpacityWhileDragging), typeof(double), typeof(LayoutGridResizerControl),
				new FrameworkPropertyMetadata(0.5));

		/// <summary>
		/// Gets or sets the opacity While Dragging.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the opacity while the control is being dragged.")]
		[Category("Other")]
		public double OpacityWhileDragging
		{
			get => (double)GetValue(OpacityWhileDraggingProperty);
			set => SetValue(OpacityWhileDraggingProperty, value);
		}
	}
}