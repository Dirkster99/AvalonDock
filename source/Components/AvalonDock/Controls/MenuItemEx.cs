using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the menu Item Ex.
	/// </summary>
	public class MenuItemEx : MenuItem
	{
		private bool _reentrantFlag = false;

		/// <summary>
		/// Initializes static members of the <see cref="MenuItemEx"/> class.
		/// </summary>
		static MenuItemEx()
		{
			IconProperty.OverrideMetadata(typeof(MenuItemEx), new FrameworkPropertyMetadata(OnIconPropertyChanged));
		}

		/// <summary>
		/// <see cref="IconTemplate"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(MenuItemEx),
				new FrameworkPropertyMetadata(null, OnIconTemplateChanged));

		/// <summary>
		/// Gets or sets the icon Template.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the data template for the icon in the menu item..")]
		[Category("Menu")]
		public DataTemplate IconTemplate
		{
			get => (DataTemplate)GetValue(IconTemplateProperty);
			set => SetValue(IconTemplateProperty, value);
		}

		/// <summary>
		/// Handles the on Icon Template Changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The event arguments.</param>
		private static void OnIconTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MenuItemEx)d).OnIconTemplateChanged(e);

		/// <summary>
		/// Handles the on Icon Template Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnIconTemplateChanged(DependencyPropertyChangedEventArgs e) => UpdateIcon();

		/// <summary>
		/// <see cref="IconTemplateSelector"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IconTemplateSelectorProperty = DependencyProperty.Register(nameof(IconTemplateSelector), typeof(DataTemplateSelector), typeof(MenuItemEx),
				new FrameworkPropertyMetadata(null, OnIconTemplateSelectorChanged));

		/// <summary>
		/// Gets or sets the icon Template Selector.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the DataTemplateSelector for the icon in the menu item.")]
		[Category("Menu")]
		public DataTemplateSelector IconTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(IconTemplateSelectorProperty);
			set => SetValue(IconTemplateSelectorProperty, value);
		}

		/// <summary>
		/// Handles the on Icon Template Selector Changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The event arguments.</param>
		private static void OnIconTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MenuItemEx)d).OnIconTemplateSelectorChanged(e);

		/// <summary>
		/// Handles the on Icon Template Selector Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnIconTemplateSelectorChanged(DependencyPropertyChangedEventArgs e) => UpdateIcon();

		private static void OnIconPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null) ((MenuItemEx)sender).UpdateIcon();
		}

		private void UpdateIcon()
		{
			if (_reentrantFlag) return;
			_reentrantFlag = true;
			if (IconTemplateSelector != null)
			{
				var dataTemplateToUse = IconTemplateSelector.SelectTemplate(Icon, this);
				if (dataTemplateToUse != null) Icon = dataTemplateToUse.LoadContent();
			}
			else if (IconTemplate != null)
			{
				Icon = IconTemplate.LoadContent();
			}

			_reentrantFlag = false;
		}
	}
}