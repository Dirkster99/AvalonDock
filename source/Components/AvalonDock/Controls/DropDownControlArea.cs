using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the drop Down Control Area.
	/// </summary>
	public class DropDownControlArea : ContentControl
	{
		/// <summary>
		/// Initializes static members of the <see cref="DropDownControlArea"/> class.
		/// </summary>
		static DropDownControlArea()
		{
			// Fixing issue with Keyboard up/down in textbox in floating anchorable focusing DropDownControlArea
			// https://github.com/Dirkster99/AvalonDock/issues/225
			FocusableProperty.OverrideMetadata(typeof(DropDownControlArea), new FrameworkPropertyMetadata(false));
			
			// See PreviewMouseRightButtonUpCallback for details.
			EventManager.RegisterClassHandler(
				typeof(DropDownControlArea),
				PreviewMouseRightButtonUpEvent,
				new MouseButtonEventHandler((s, e) => (s as DropDownControlArea)?.PreviewMouseRightButtonUpCallback(e)));
		}

		/// <summary>
		/// <see cref="DropDownContextMenu"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register(nameof(DropDownContextMenu), typeof(ContextMenu), typeof(DropDownControlArea),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the drop Down Context Menu.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the drop down menu to show up when user click on an anchorable menu pin.")]
		[Category("Menu")]
		public ContextMenu DropDownContextMenu
		{
			get => (ContextMenu)GetValue(DropDownContextMenuProperty);
			set => SetValue(DropDownContextMenuProperty, value);
		}

		/// <summary>
		/// <see cref="DropDownContextMenuDataContext"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DropDownContextMenuDataContextProperty = DependencyProperty.Register(nameof(DropDownContextMenuDataContext), typeof(object), typeof(DropDownControlArea),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the drop Down Context Menu Data Context.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the DataContext to set for the DropDownContext menu property.")]
		[Category("Menu")]
		public object DropDownContextMenuDataContext
		{
			get => (object)GetValue(DropDownContextMenuDataContextProperty);
			set => SetValue(DropDownContextMenuDataContextProperty, value);
		}

		// The core code uses OnPreviewMouseButtonUp for this logic. However, this change fixes
		// a bug when the context menu style has no border. The right click to show the context
		// menu gets handled by the first menu item. If that happens to be Close, the tab closes
		// unexpectedly.
		// We use a class handler for the event - which gets called earlier in the event handling
		// chain - to show the right-click context menu and, importantly, mark the event as handled,
		// so no further processing occurs.
		private void PreviewMouseRightButtonUpCallback(MouseButtonEventArgs e)
		{
			if (!e.Handled && DropDownContextMenu != null)
			{
				// Fix for multi-dpi aware aplications
				DropDownContextMenu.PlacementTarget = e.Source as UIElement;
				// DropDownContextMenu.PlacementTarget = null;
				DropDownContextMenu.Placement = PlacementMode.MousePoint;
				DropDownContextMenu.HorizontalOffset = 0d;
				DropDownContextMenu.VerticalOffset = 0d;
				DropDownContextMenu.DataContext = DropDownContextMenuDataContext;
				DropDownContextMenu.IsOpen = true;

				e.Handled = true;
			}
		}

		// protected override System.Windows.Media.HitTestResult HitTestCore(System.Windows.Media.PointHitTestParameters hitTestParameters)
		// {
		//    var hitResult = base.HitTestCore(hitTestParameters);
		//    if (hitResult == null)
		//        return new PointHitTestResult(this, hitTestParameters.HitPoint);

		// return hitResult;
		// }
	}
}