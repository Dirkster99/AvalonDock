/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// Implements a control like <see cref="System.Windows.Controls.GridSplitter"/> that can be used to resize areas
	/// horizontally or vertically (only one of these but never both) in a grid layout.
	/// </summary>
	/// <seealso cref="Thumb"/>
	public class LayoutGridResizerControl : Thumb
	{
		#region Constructors

		static LayoutGridResizerControl()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(typeof(LayoutGridResizerControl)));
			HorizontalAlignmentProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
			VerticalAlignmentProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
			BackgroundProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(Brushes.Transparent));
			IsHitTestVisibleProperty.OverrideMetadata(typeof(LayoutGridResizerControl), new FrameworkPropertyMetadata(true, null));
		}

		#endregion Constructors

		#region Properties

		#region BackgroundWhileDragging

		/// <summary><see cref="BackgroundWhileDragging"/> dependency property.</summary>
		public static readonly DependencyProperty BackgroundWhileDraggingProperty = DependencyProperty.Register(nameof(BackgroundWhileDragging), typeof(Brush), typeof(LayoutGridResizerControl),
				new FrameworkPropertyMetadata(Brushes.Black));

		/// <summary>
		/// Gets or sets the <see cref="BackgroundWhileDragging"/> property.
		/// This dependency property indicates the background while the control is being dragged.
		/// </summary>
		public Brush BackgroundWhileDragging
		{
			get => (Brush)GetValue(BackgroundWhileDraggingProperty);
			set => SetValue(BackgroundWhileDraggingProperty, value);
		}

		#endregion BackgroundWhileDragging

		#region OpacityWhileDragging

		/// <summary><see cref="OpacityWhileDragging"/> dependency property.</summary>
		public static readonly DependencyProperty OpacityWhileDraggingProperty = DependencyProperty.Register(nameof(OpacityWhileDragging), typeof(double), typeof(LayoutGridResizerControl),
				new FrameworkPropertyMetadata(0.5));

		/// <summary>
		/// Gets or sets the <see cref="OpacityWhileDragging"/> property.
		/// This dependency property indicates opacity while the control is being dragged.
		/// </summary>
		public double OpacityWhileDragging
		{
			get => (double)GetValue(OpacityWhileDraggingProperty);
			set => SetValue(OpacityWhileDraggingProperty, value);
		}

		#endregion OpacityWhileDragging

		#endregion Properties
	}
}
