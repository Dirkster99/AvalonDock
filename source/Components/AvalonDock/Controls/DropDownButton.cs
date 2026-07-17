using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the drop Down Button.
	/// </summary>
	public class DropDownButton : ToggleButton
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DropDownButton"/> class.
		/// </summary>
		public DropDownButton()
		{
			Unloaded += DropDownButton_Unloaded;
		}

		/// <summary>
		/// <see cref="DropDownContextMenu"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register(nameof(DropDownContextMenu), typeof(ContextMenu), typeof(DropDownButton),
				new FrameworkPropertyMetadata(null, OnDropDownContextMenuChanged));

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
		/// Handles the on Drop Down Context Menu Changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The event arguments.</param>
		private static void OnDropDownContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DropDownButton)d).OnDropDownContextMenuChanged(e);

		/// <summary>
		/// Handles the on Drop Down Context Menu Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnDropDownContextMenuChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue is ContextMenu oldContextMenu && IsChecked.GetValueOrDefault())
				oldContextMenu.Closed -= OnContextMenuClosed;
		}

		/// <summary>
		/// <see cref="DropDownContextMenuDataContext"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DropDownContextMenuDataContextProperty = DependencyProperty.Register(nameof(DropDownContextMenuDataContext), typeof(object), typeof(DropDownButton),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the drop Down Context Menu Data Context.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the DataContext to set for the DropDownContext menu property.")]
		[Category("Menu")]
		public object DropDownContextMenuDataContext
		{
			get => GetValue(DropDownContextMenuDataContextProperty);
			set => SetValue(DropDownContextMenuDataContextProperty, value);
		}

		/// <inheritdoc/>
		protected override void OnClick()
		{
			if (DropDownContextMenu != null)
			{
				// IsChecked = true;
				DropDownContextMenu.PlacementTarget = this;
				DropDownContextMenu.Placement = PlacementMode.Bottom;
				DropDownContextMenu.DataContext = DropDownContextMenuDataContext;
				DropDownContextMenu.Closed += OnContextMenuClosed;
				DropDownContextMenu.IsOpen = true;
			}

			base.OnClick();
		}

		private void OnContextMenuClosed(object sender, RoutedEventArgs e)
		{
			// Debug.Assert(IsChecked.GetValueOrDefault());
			var ctxMenu = sender as ContextMenu;
			ctxMenu.Closed -= OnContextMenuClosed;
			IsChecked = false;
		}

		private void DropDownButton_Unloaded(object sender, RoutedEventArgs e)
		{
			// When changing theme, Unloaded event is called, erasing the DropDownContextMenu.
			// Prevent this on theme changes.
			if (IsLoaded) DropDownContextMenu = null;
		}
	}
}