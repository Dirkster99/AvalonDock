namespace AvalonDock.Themes.VS.Themes.Menu
{
	using System.Windows;

	/// <summary>
	/// Resource key management class for menu-specific colors, styles and other elements
	/// that are typically changed between themes.
	/// </summary>
	public static class MenuKeys
	{
		/// <summary>
		/// Gets the resource key for the menu separator border brush.
		/// </summary>
		public static readonly ComponentResourceKey MenuSeparatorBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "MenuSeparatorBorderBrushKey");

		/// <summary>
		/// Gets the resource key for the submenu item background.
		/// </summary>
		public static readonly ComponentResourceKey SubmenuItemBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "SubmenuItemBackgroundKey");

		/// <summary>
		/// Gets the resource key for the menu item highlighted background.
		/// </summary>
		public static readonly ComponentResourceKey MenuItemHighlightedBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "MenuItemHighlightedBackgroundKey");

		/// <summary>
		/// Gets the resource key for the submenu item background highlighted.
		/// </summary>
		public static readonly ComponentResourceKey SubmenuItemBackgroundHighlightedKey = new ComponentResourceKey(typeof(ResourceKeys), "SubmenuItemBackgroundHighlightedKey");

		/// <summary>
		/// Gets the resource key for the check mark background brush.
		/// </summary>
		public static readonly ComponentResourceKey CheckMarkBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkBackgroundBrushKey");

		/// <summary>
		/// Gets the resource key for the check mark border brush.
		/// </summary>
		public static readonly ComponentResourceKey CheckMarkBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkBorderBrushKey");

		/// <summary>
		/// Gets the resource key for the check mark foreground brush.
		/// </summary>
		public static readonly ComponentResourceKey CheckMarkForegroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkForegroundBrushKey");

		/// <summary>
		/// Gets the resource key for the disabled sub menu item background brush.
		/// </summary>
		public static readonly ComponentResourceKey DisabledSubMenuItemBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DisabledSubMenuItemBackgroundBrushKey");

		/// <summary>
		/// Gets the resource key for the disabled sub menu item border brush.
		/// </summary>
		public static readonly ComponentResourceKey DisabledSubMenuItemBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DisabledSubMenuItemBorderBrushKey");

		/// <summary>
		/// Gets the resource key for the text brush.
		/// </summary>
		public static readonly ComponentResourceKey TextBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "TextBrushKey");

		/// <summary>
		/// Gets the resource key for the item background selected.
		/// </summary>
		public static readonly ComponentResourceKey ItemBackgroundSelectedKey = new ComponentResourceKey(typeof(ResourceKeys), "ItemBackgroundSelectedKey");

		/// <summary>
		/// Gets the resource key for the item text disabled.
		/// </summary>
		public static readonly ComponentResourceKey ItemTextDisabledKey = new ComponentResourceKey(typeof(ResourceKeys), "ItemTextDisabledKey");

		/// <summary>
		/// Gets the resource key for the item background hover.
		/// </summary>
		public static readonly ComponentResourceKey ItemBackgroundHoverKey = new ComponentResourceKey(typeof(ResourceKeys), "ItemBackgroundHoverKey");

		/// <summary>
		/// Gets the resource key for the drop shadow effect.
		/// </summary>
		public static readonly ComponentResourceKey DropShadowEffectKey = new ComponentResourceKey(typeof(ResourceKeys), "DropShadowEffectKey");
	}
}