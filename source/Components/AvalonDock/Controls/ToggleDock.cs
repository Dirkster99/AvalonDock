using System.Windows;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Provides helper members for toggle Dock.
	/// </summary>
	public static class ToggleDock
	{
		/// <summary>
		/// Icon attached dependency property.
		/// </summary>
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.RegisterAttached(
				"Icon",
				typeof(object),
				typeof(ToggleDock),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets the get Icon.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>The requested value.</returns>
		public static object GetIcon(DependencyObject element) => element.GetValue(IconProperty);

		/// <summary>
		/// Sets the set Icon.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="value">The value.</param>
		public static void SetIcon(DependencyObject element, object value) => element.SetValue(IconProperty, value);

		/// <summary>
		/// ToolTip attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ToolTipProperty =
			DependencyProperty.RegisterAttached(
				"ToolTip",
				typeof(object),
				typeof(ToggleDock),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets the get Tool Tip.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>The requested value.</returns>
		public static object GetToolTip(DependencyObject element) => element.GetValue(ToolTipProperty);

		/// <summary>
		/// Sets the set Tool Tip.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="value">The value.</param>
		public static void SetToolTip(DependencyObject element, object value) => element.SetValue(ToolTipProperty, value);

		/// <summary>
		/// IconTemplate attached dependency property.
		/// </summary>
		public static readonly DependencyProperty IconTemplateProperty =
			DependencyProperty.RegisterAttached(
				"IconTemplate",
				typeof(DataTemplate),
				typeof(ToggleDock),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets the get Icon Template.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>The requested value.</returns>
		public static DataTemplate GetIconTemplate(DependencyObject element) => (DataTemplate)element.GetValue(IconTemplateProperty);

		/// <summary>
		/// Sets the set Icon Template.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="value">The value.</param>
		public static void SetIconTemplate(DependencyObject element, DataTemplate value) => element.SetValue(IconTemplateProperty, value);
	}
}