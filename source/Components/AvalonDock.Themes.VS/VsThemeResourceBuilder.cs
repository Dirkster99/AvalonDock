using System;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Themes.VS.Themes;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Builds a WPF <see cref="ResourceDictionary"/> from a <see cref="VsThemeColorPalette"/>
	/// by mapping VS Environment color tokens to AvalonDock resource keys.
	/// </summary>
	public static class VsThemeResourceBuilder
	{
		/// <summary>
		/// Default pack URI for the shared control templates (VS2015-era Generic.xaml).
		/// </summary>
		private static readonly Uri DefaultGenericXamlUri =
			new Uri("/AvalonDock.Themes.VS;component/Themes/Generic.xaml", UriKind.Relative);

		/// <summary>
		/// Builds a complete <see cref="ResourceDictionary"/> using the default Generic.xaml templates.
		/// </summary>
		/// <param name="palette">The parsed VS theme color palette.</param>
		/// <returns>A resource dictionary ready to be used with <see cref="DictionaryTheme"/>.</returns>
		public static ResourceDictionary Build(VsThemeColorPalette palette)
		{
			return Build(palette, DefaultGenericXamlUri);
		}

		/// <summary>
		/// Builds a complete <see cref="ResourceDictionary"/> containing brushes
		/// for all AvalonDock resource keys, mapped from the given VS theme palette,
		/// and merges the specified Generic.xaml control templates.
		/// </summary>
		/// <param name="palette">The parsed VS theme color palette.</param>
		/// <param name="genericXamlUri">Pack URI for the Generic.xaml resource dictionary to merge.</param>
		/// <returns>A resource dictionary ready to be used with <see cref="DictionaryTheme"/>.</returns>
		public static ResourceDictionary Build(VsThemeColorPalette palette, Uri genericXamlUri)
		{
			if (palette == null)
			{
				throw new ArgumentNullException(nameof(palette));
			}

			if (genericXamlUri == null)
			{
				throw new ArgumentNullException(nameof(genericXamlUri));
			}

			var dict = new ResourceDictionary();

			var accent = palette.GetBackgroundOrDefault("FileTabSelectedBorder", Color.FromRgb(0x00, 0x7A, 0xCC));
			var background = palette.GetBackgroundOrDefault("EnvironmentBackground", Color.FromRgb(0x2D, 0x2D, 0x30));
			var panelBorder = palette.GetBackgroundOrDefault("ToolWindowBorder", Color.FromRgb(0x3F, 0x3F, 0x46));
			var tabBg = palette.GetBackgroundOrDefault("ToolWindowTabGradientBegin", Color.FromRgb(0x25, 0x25, 0x26));
			var inactiveText = palette.GetBackgroundOrDefault("FileTabInactiveText", Color.FromRgb(0xD0, 0xD0, 0xD0));
			var dimText = palette.GetForegroundOrDefault("ToolWindowTabText", Color.FromRgb(0x8C, 0x8C, 0x8C));
			var brightText = Colors.White;

			// VS2026 introduced tokens that color the tab strip and window/tool headers
			// independently from the rest of the shell chrome. When present they take
			// precedence over the classic per-element tokens; otherwise the original
			// fallbacks below are used, so existing XML themes are unaffected.
			var environmentTab = palette.GetBackground("EnvironmentTab");
			var environmentTabInactive = palette.GetBackground("EnvironmentTabInactive");
			var environmentHeader = palette.GetBackground("EnvironmentHeader");
			var environmentHeaderInactive = palette.GetBackground("EnvironmentHeaderInactive");

			// Accent
			dict[ResourceKeys.ControlAccentColorKey] = accent;
			SetBrush(dict, ResourceKeys.ControlAccentBrushKey, accent);

			// General
			SetBrush(dict, ResourceKeys.Background, background);
			SetBrush(dict, ResourceKeys.PanelBorderBrush, panelBorder);
			SetBrush(dict, ResourceKeys.TabBackground, environmentTab ?? tabBg);

			// Auto Hide : Tab
			SetBrush(dict, ResourceKeys.AutoHideTabDefaultBackground, palette.GetBackgroundOrDefault("AutoHideTabBackgroundBegin", background));
			SetBrush(dict, ResourceKeys.AutoHideTabDefaultBorder, palette.GetBackgroundOrDefault("AutoHideTabBorder", panelBorder));
			SetBrush(dict, ResourceKeys.AutoHideTabDefaultText, palette.GetBackgroundOrDefault("AutoHideTabText", inactiveText));
			SetBrush(dict, ResourceKeys.AutoHideTabHoveredBackground, palette.GetBackgroundOrDefault("AutoHideTabMouseOverBackgroundBegin", panelBorder));
			SetBrush(dict, ResourceKeys.AutoHideTabHoveredBorder, palette.GetBackgroundOrDefault("AutoHideTabMouseOverBorder", accent));
			SetBrush(dict, ResourceKeys.AutoHideTabHoveredText, palette.GetBackgroundOrDefault("AutoHideTabMouseOverText", accent));

			// Document Well : Overflow Button
			SetBrush(dict, ResourceKeys.DocumentWellOverflowButtonDefaultGlyph, palette.GetBackgroundOrDefault("DocWellOverflowButtonGlyph", inactiveText));
			SetBrush(dict, ResourceKeys.DocumentWellOverflowButtonHoveredBackground, palette.GetBackgroundOrDefault("DocWellOverflowButtonMouseOverBackground", panelBorder));
			SetBrush(dict, ResourceKeys.DocumentWellOverflowButtonHoveredBorder, palette.GetBackgroundOrDefault("DocWellOverflowButtonMouseOverBackground", panelBorder));
			SetBrush(dict, ResourceKeys.DocumentWellOverflowButtonHoveredGlyph, palette.GetBackgroundOrDefault("DocWellOverflowButtonMouseOverGlyph", accent));
			SetBrush(dict, ResourceKeys.DocumentWellOverflowButtonPressedBackground, palette.GetBackgroundOrDefault("DocWellOverflowButtonMouseDownBackground", accent));
			SetBrush(dict, ResourceKeys.DocumentWellOverflowButtonPressedBorder, palette.GetBackgroundOrDefault("DocWellOverflowButtonMouseDownBackground", accent));
			SetBrush(dict, ResourceKeys.DocumentWellOverflowButtonPressedGlyph, palette.GetBackgroundOrDefault("DocWellOverflowButtonMouseDownGlyph", brightText));

			// Document Well : Tab
			SetBrush(dict, ResourceKeys.DocumentWellTabSelectedActiveBackground, palette.GetBackgroundOrDefault("FileTabSelectedBorder", accent));
			SetBrush(dict, ResourceKeys.DocumentWellTabSelectedActiveText, palette.GetBackgroundOrDefault("FileTabSelectedText", brightText));
			SetBrush(dict, ResourceKeys.DocumentWellTabSelectedInactiveBackground, environmentTabInactive ?? palette.GetBackgroundOrDefault("FileTabInactiveBorder", background));
			SetBrush(dict, ResourceKeys.DocumentWellTabSelectedInactiveText, palette.GetBackgroundOrDefault("FileTabInactiveText", inactiveText));
			SetBrush(dict, ResourceKeys.DocumentWellTabUnselectedBackground, environmentTab ?? palette.GetBackgroundOrDefault("FileTabBackground", tabBg));
			SetBrush(dict, ResourceKeys.DocumentWellTabUnselectedText, palette.GetBackgroundOrDefault("FileTabText", dimText));
			SetBrush(dict, ResourceKeys.DocumentWellTabUnselectedHoveredBackground, palette.GetBackgroundOrDefault("FileTabHotBorder", panelBorder));
			SetBrush(dict, ResourceKeys.DocumentWellTabUnselectedHoveredText, palette.GetBackgroundOrDefault("FileTabHotText", brightText));

			// Document Well : Tab : Close Buttons
			SetBrush(dict, ResourceKeys.DocumentWellTabButtonSelectedActiveGlyph, palette.GetBackgroundOrDefault("FileTabButtonSelectedActiveGlyph", inactiveText));
			MapCloseButton(dict, palette, "FileTabButtonHoverSelectedActive", ResourceKeys.DocumentWellTabButtonSelectedActiveHoveredBackground, ResourceKeys.DocumentWellTabButtonSelectedActiveHoveredBorder, ResourceKeys.DocumentWellTabButtonSelectedActiveHoveredGlyph, accent);
			MapCloseButton(dict, palette, "FileTabButtonDownSelectedActive", ResourceKeys.DocumentWellTabButtonSelectedActivePressedBackground, ResourceKeys.DocumentWellTabButtonSelectedActivePressedBorder, ResourceKeys.DocumentWellTabButtonSelectedActivePressedGlyph, accent);
			SetBrush(dict, ResourceKeys.DocumentWellTabButtonSelectedInactiveGlyph, palette.GetBackgroundOrDefault("FileTabButtonSelectedInactiveGlyph", dimText));
			MapCloseButton(dict, palette, "FileTabButtonHoverSelectedInactive", ResourceKeys.DocumentWellTabButtonSelectedInactiveHoveredBackground, ResourceKeys.DocumentWellTabButtonSelectedInactiveHoveredBorder, ResourceKeys.DocumentWellTabButtonSelectedInactiveHoveredGlyph, accent);
			MapCloseButton(dict, palette, "FileTabButtonDownSelectedInactive", ResourceKeys.DocumentWellTabButtonSelectedInactivePressedBackground, ResourceKeys.DocumentWellTabButtonSelectedInactivePressedBorder, ResourceKeys.DocumentWellTabButtonSelectedInactivePressedGlyph, accent);
			SetBrush(dict, ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredGlyph, palette.GetBackgroundOrDefault("FileTabHotGlyph", inactiveText));
			MapCloseButton(dict, palette, "FileTabButtonHoverInactive", ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBackground, ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBorder, ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonHoveredGlyph, accent);
			MapCloseButton(dict, palette, "FileTabButtonDownInactive", ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonPressedBackground, ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonPressedBorder, ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonPressedGlyph, accent);

			// Tool Window : Caption
			SetBrush(dict, ResourceKeys.ToolWindowCaptionActiveBackground, environmentHeader ?? palette.GetBackgroundOrDefault("TitleBarActiveBorder", accent));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionActiveGrip, palette.GetBackgroundOrDefault("TitleBarDragHandleActive", panelBorder));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionActiveText, palette.GetBackgroundOrDefault("TitleBarActiveText", brightText));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionInactiveBackground, environmentHeaderInactive ?? palette.GetBackgroundOrDefault("TitleBarInactive", background));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionInactiveGrip, palette.GetBackgroundOrDefault("TitleBarDragHandle", background));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionInactiveText, palette.GetBackgroundOrDefault("TitleBarInactiveText", dimText));

			// ToggleDockButton foreground
			SetBrush(dict, Controls.ToggleDockButton.ForegroundBrushKey, dimText);

			// Tool Window : Caption : Buttons
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonActiveGlyph, palette.GetBackgroundOrDefault("ToolWindowButtonActiveGlyph", inactiveText));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonActiveHoveredBackground, palette.GetBackgroundOrDefault("ToolWindowButtonHoverActive", panelBorder));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonActiveHoveredBorder, palette.GetBackgroundOrDefault("ToolWindowButtonHoverActiveBorder", panelBorder));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonActiveHoveredGlyph, palette.GetBackgroundOrDefault("ToolWindowButtonHoverActiveGlyph", brightText));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonActivePressedBackground, palette.GetBackgroundOrDefault("ToolWindowButtonDown", accent));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonActivePressedBorder, palette.GetBackgroundOrDefault("ToolWindowButtonDownBorder", accent));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonActivePressedGlyph, palette.GetBackgroundOrDefault("ToolWindowButtonDownActiveGlyph", brightText));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonInactiveGlyph, palette.GetBackgroundOrDefault("ToolWindowButtonInactiveGlyph", dimText));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonInactiveHoveredBackground, palette.GetBackgroundOrDefault("ToolWindowButtonHoverInactive", background));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonInactiveHoveredBorder, palette.GetBackgroundOrDefault("ToolWindowButtonHoverInactiveBorder", background));
			SetBrush(dict, ResourceKeys.ToolWindowCaptionButtonInactiveHoveredGlyph, palette.GetBackgroundOrDefault("ToolWindowButtonHoverInactiveGlyph", inactiveText));

			// Tool Window : Tab
			SetBrush(dict, ResourceKeys.ToolWindowTabSelectedActiveBackground, palette.GetBackgroundOrDefault("ToolWindowTabSelectedTab", accent));
			SetBrush(dict, ResourceKeys.ToolWindowTabSelectedActiveText, palette.GetBackgroundOrDefault("ToolWindowTabSelectedActiveText", brightText));
			SetBrush(dict, ResourceKeys.ToolWindowTabSelectedInactiveBackground, environmentTabInactive ?? palette.GetBackgroundOrDefault("ToolWindowTabSelectedTab", panelBorder));
			SetBrush(dict, ResourceKeys.ToolWindowTabSelectedInactiveText, palette.GetBackgroundOrDefault("ToolWindowTabSelectedText", inactiveText));
			SetBrush(dict, ResourceKeys.ToolWindowTabUnselectedBackground, environmentTab ?? palette.GetBackgroundOrDefault("ToolWindowTabGradientBegin", tabBg));
			SetBrush(dict, ResourceKeys.ToolWindowTabUnselectedText, palette.GetBackgroundOrDefault("ToolWindowTabText", dimText));
			SetBrush(dict, ResourceKeys.ToolWindowTabUnselectedHoveredBackground, palette.GetBackgroundOrDefault("ToolWindowTabMouseOverBackgroundBegin", panelBorder));
			SetBrush(dict, ResourceKeys.ToolWindowTabUnselectedHoveredText, palette.GetBackgroundOrDefault("ToolWindowTabMouseOverText", brightText));

			// Floating Document Window
			SetBrush(dict, ResourceKeys.FloatingDocumentWindowBackground, tabBg);
			SetBrush(dict, ResourceKeys.FloatingDocumentWindowBorder, panelBorder);

			// Floating Tool Window
			SetBrush(dict, ResourceKeys.FloatingToolWindowBackground, tabBg);
			SetBrush(dict, ResourceKeys.FloatingToolWindowBorder, panelBorder);

			// Navigator Window
			SetBrush(dict, ResourceKeys.NavigatorWindowBackground, tabBg);
			SetBrush(dict, ResourceKeys.NavigatorWindowForeground, inactiveText);
			SetBrush(dict, ResourceKeys.NavigatorWindowSelectedBackground, accent);
			SetBrush(dict, ResourceKeys.NavigatorWindowSelectedText, brightText);

			// Docking Buttons (drop indicators)
			SetBrush(dict, ResourceKeys.DockingButtonForegroundBrushKey, palette.GetBackgroundOrDefault("DockTargetGlyphArrow", accent));
			SetBrush(dict, ResourceKeys.DockingButtonForegroundArrowBrushKey, brightText);
			SetBrush(dict, ResourceKeys.DockingButtonStarBorderBrushKey, palette.GetBackgroundOrDefault("DockTargetBorder", Color.FromArgb(0x40, 0x80, 0x80, 0x80)));
			SetBrush(dict, ResourceKeys.DockingButtonStarBackgroundBrushKey, palette.GetBackgroundOrDefault("DockTargetBackground", Color.FromArgb(0x4C, 0x00, 0x00, 0x00)));

			// Preview Box
			var previewColor = palette.GetBackgroundOrDefault("DockTargetGlyphBackgroundBegin", accent);
			SetBrush(dict, ResourceKeys.PreviewBoxBorderBrushKey, previewColor);
			var previewBrush = new SolidColorBrush(previewColor) { Opacity = 0.5 };
			previewBrush.Freeze();
			dict[ResourceKeys.PreviewBoxBackgroundBrushKey] = previewBrush;

			// Docking button sizes (match VS2013 overlay button dimensions)
			dict[ResourceKeys.DockingButtonWidthKey] = 40.0;
			dict[ResourceKeys.DockingButtonHeightKey] = 40.0;

			// Menu brushes
			MapMenuBrushes(dict, palette, background, panelBorder, inactiveText, dimText, accent, brightText);

			// Merge the control templates (Generic.xaml)
			dict.MergedDictionaries.Add(new ResourceDictionary
			{
				Source = genericXamlUri,
			});

			return dict;
		}

		private static void SetBrush(ResourceDictionary dict, object key, Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			dict[key] = brush;
		}

		private static void MapCloseButton(
			ResourceDictionary dict,
			VsThemeColorPalette palette,
			string tokenPrefix,
			ComponentResourceKey bgKey,
			ComponentResourceKey borderKey,
			ComponentResourceKey glyphKey,
			Color fallbackAccent)
		{
			SetBrush(dict, bgKey, palette.GetBackgroundOrDefault(tokenPrefix, fallbackAccent));
			SetBrush(dict, borderKey, palette.GetBackgroundOrDefault(tokenPrefix + "Border", fallbackAccent));
			SetBrush(dict, glyphKey, palette.GetBackgroundOrDefault(tokenPrefix + "Glyph", Colors.White));
		}

		private static void MapMenuBrushes(
			ResourceDictionary dict,
			VsThemeColorPalette palette,
			Color background,
			Color panelBorder,
			Color inactiveText,
			Color dimText,
			Color accent,
			Color brightText)
		{
			var menuBg = palette.GetBackgroundOrDefault("CommandShelfHighlightGradientBegin", background);
			var menuHover = DarkenOrLighten(menuBg, 0.15);

			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "MenuSeparatorBorderBrushKey"), panelBorder);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "SubmenuItemBackgroundKey"), menuBg);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "MenuItemHighlightedBackgroundKey"), menuHover);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "SubmenuItemBackgroundHighlightedKey"), menuHover);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkBackgroundBrushKey"), Colors.Transparent);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkBorderBrushKey"), Color.FromArgb(0x90, 0x51, 0x51, 0x51));
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "CheckMarkForegroundBrushKey"), brightText);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "DisabledSubMenuItemBackgroundBrushKey"), menuBg);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "DisabledSubMenuItemBorderBrushKey"), panelBorder);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "TextBrushKey"), inactiveText);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "ItemBackgroundSelectedKey"), accent);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "ItemTextDisabledKey"), dimText);
			SetBrush(dict, new ComponentResourceKey(typeof(ResourceKeys), "ItemBackgroundHoverKey"), menuHover);
		}

		private static Color DarkenOrLighten(Color color, double amount)
		{
			var brightness = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
			if (brightness < 128)
			{
				return Color.FromArgb(color.A,
					(byte)Math.Min(255, color.R + (int)(255 * amount)),
					(byte)Math.Min(255, color.G + (int)(255 * amount)),
					(byte)Math.Min(255, color.B + (int)(255 * amount)));
			}
			else
			{
				return Color.FromArgb(color.A,
					(byte)Math.Max(0, color.R - (int)(255 * amount)),
					(byte)Math.Max(0, color.G - (int)(255 * amount)),
					(byte)Math.Max(0, color.B - (int)(255 * amount)));
			}
		}
	}
}