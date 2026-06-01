using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the toggle Anchorable Pane Title.
	/// </summary>
	public class ToggleAnchorablePaneTitle : AnchorablePaneTitle
	{
		/// <summary>
		/// Initializes static members of the <see cref="ToggleAnchorablePaneTitle"/> class.
		/// </summary>
		static ToggleAnchorablePaneTitle()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleAnchorablePaneTitle), new FrameworkPropertyMetadata(typeof(ToggleAnchorablePaneTitle)));
		}

		/// <summary>
		/// <see cref="ShowMinimizeButton"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowMinimizeButtonProperty =
			DependencyProperty.Register(nameof(ShowMinimizeButton), typeof(bool), typeof(ToggleAnchorablePaneTitle),
				new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets a value indicating whether show Minimize Button.
		/// </summary>
		public bool ShowMinimizeButton
		{
			get => (bool)GetValue(ShowMinimizeButtonProperty);
			set => SetValue(ShowMinimizeButtonProperty, value);
		}

		/// <summary>
		/// <see cref="ShowOptionsButton"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowOptionsButtonProperty =
			DependencyProperty.Register(nameof(ShowOptionsButton), typeof(bool), typeof(ToggleAnchorablePaneTitle),
				new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets a value indicating whether show Options Button.
		/// </summary>
		public bool ShowOptionsButton
		{
			get => (bool)GetValue(ShowOptionsButtonProperty);
			set => SetValue(ShowOptionsButtonProperty, value);
		}

		/// <inheritdoc/>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild("PART_OptionsButton") is Button optionsBtn)
			{
				optionsBtn.Click += OnOptionsButtonClick;
			}
		}

		private void OnOptionsButtonClick(object sender, RoutedEventArgs e)
		{
			var manager = FindToggleDockingManager();
			if (manager == null || Model == null)
				return;

			var menu = manager.BuildToggleContextMenu(Model);
			menu.PlacementTarget = sender as Button;
			menu.Placement = PlacementMode.Bottom;
			menu.IsOpen = true;
		}

		private ToggleDockingManager FindToggleDockingManager()
		{
			DependencyObject current = this;
			while (current != null)
			{
				if (current is ToggleDockingManager mgr)
					return mgr;
				current = VisualTreeHelper.GetParent(current);
			}

			return null;
		}
	}
}