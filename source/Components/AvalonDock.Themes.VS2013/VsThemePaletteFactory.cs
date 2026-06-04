/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Themes.VS;
using AvalonDock.Themes.VS2013.Themes;

namespace AvalonDock.Themes
{
	/// <summary>
	/// Builds a VS2013 <see cref="ResourceDictionary"/> of <see cref="SolidColorBrush"/>
	/// entries keyed by <see cref="ResourceKeys"/> values from a GZIP-compressed
	/// .vstheme palette. Decompression and XML parsing are delegated to the shared
	/// <see cref="VsThemeParser"/> in AvalonDock.Themes.VS; this class only owns the
	/// VS2013-specific color-key mapping and merges the VS2013 control templates.
	/// </summary>
	/// <remarks>
	/// Color-key mapping authority: DockPanel Suite VS2012PaletteFactory
	/// (WeifenLuo.WinFormsUI.ThemeVS2012.VS2012PaletteFactory).
	/// </remarks>
	internal sealed class VsThemePaletteFactory
	{
		private readonly VsThemeColorPalette _palette;

		private VsThemePaletteFactory(byte[] gzipBytes)
		{
			_palette = VsThemeParser.ParseGZip(gzipBytes);
		}

		/// <summary>
		/// Decompresses and parses the given GZIP-compressed .vstheme bytes and builds
		/// a complete VS2013 <see cref="ResourceDictionary"/>.
		/// </summary>
		/// <param name="gzipBytes">The GZIP-compressed .vstheme XML content.</param>
		/// <returns>A resource dictionary ready to be used with a dictionary-based theme.</returns>
		internal static ResourceDictionary BuildDictionary(byte[] gzipBytes)
			=> new VsThemePaletteFactory(gzipBytes).Build();

		private ResourceDictionary Build()
		{
			var dict = new ResourceDictionary();
			PopulateBrushes(dict);
			dict.MergedDictionaries.Add(new ResourceDictionary
			{
				Source = new Uri("/AvalonDock.Themes.VS2013;component/OverlayButtons.xaml", UriKind.Relative)
			});
			dict.MergedDictionaries.Add(new ResourceDictionary
			{
				Source = new Uri("/AvalonDock.Themes.VS2013;component/Themes/Generic.xaml", UriKind.Relative)
			});
			return dict;
		}

		// VS2013 color-key mapping (mirrors VS2012PaletteFactory).
		private void PopulateBrushes(ResourceDictionary d)
		{
			// Accent
			d[ResourceKeys.ControlAccentBrushKey] = Brush("FileTabSelectedBorder");

			// General / frame
			d[ResourceKeys.Background] = Brush("AutoHideTabBackgroundBegin");
			d[ResourceKeys.PanelBorderBrush] = Brush("ToolWindowBorder");
			d[ResourceKeys.TabBackground] = Brush("ToolWindowBackground");

			// Auto-hide strip
			d[ResourceKeys.AutoHideTabDefaultBackground] = Brush("AutoHideTabBackgroundBegin");
			d[ResourceKeys.AutoHideTabDefaultBorder] = Brush("AutoHideTabBorder");
			d[ResourceKeys.AutoHideTabDefaultText] = Brush("AutoHideTabText");
			d[ResourceKeys.AutoHideTabHoveredBackground] = Brush("AutoHideTabMouseOverBackgroundBegin");
			d[ResourceKeys.AutoHideTabHoveredBorder] = Brush("AutoHideTabMouseOverBorder");
			d[ResourceKeys.AutoHideTabHoveredText] = Brush("AutoHideTabMouseOverText");

			// Document well — overflow button
			d[ResourceKeys.DocumentWellOverflowButtonDefaultGlyph] = Brush("DocWellOverflowButtonGlyph");
			d[ResourceKeys.DocumentWellOverflowButtonHoveredBackground] = Brush("DocWellOverflowButtonMouseOverBackground");
			d[ResourceKeys.DocumentWellOverflowButtonHoveredBorder] = Brush("DocWellOverflowButtonMouseOverBorder");
			d[ResourceKeys.DocumentWellOverflowButtonHoveredGlyph] = Brush("DocWellOverflowButtonMouseOverGlyph");
			d[ResourceKeys.DocumentWellOverflowButtonPressedBackground] = Brush("DocWellOverflowButtonMouseDownBackground");
			d[ResourceKeys.DocumentWellOverflowButtonPressedBorder] = Brush("DocWellOverflowButtonMouseDownBorder");
			d[ResourceKeys.DocumentWellOverflowButtonPressedGlyph] = Brush("DocWellOverflowButtonMouseDownGlyph");

			// Document well — selected active/inactive tabs
			d[ResourceKeys.DocumentWellTabSelectedActiveBackground] = Brush("FileTabSelectedBorder");
			d[ResourceKeys.DocumentWellTabSelectedActiveText] = Brush("FileTabSelectedText");
			d[ResourceKeys.DocumentWellTabSelectedInactiveBackground] = Brush("FileTabInactiveBorder");
			d[ResourceKeys.DocumentWellTabSelectedInactiveText] = Brush("FileTabInactiveText");
			d[ResourceKeys.DocumentWellTabUnselectedBackground] = Brush("FileTabBackground");
			d[ResourceKeys.DocumentWellTabUnselectedText] = Brush("FileTabText");
			d[ResourceKeys.DocumentWellTabUnselectedHoveredBackground] = Brush("FileTabHotBorder");
			d[ResourceKeys.DocumentWellTabUnselectedHoveredText] = Brush("FileTabHotText");

			// Document well — close button glyphs
			d[ResourceKeys.DocumentWellTabButtonSelectedActiveGlyph] = Brush("FileTabButtonSelectedActiveGlyph");
			d[ResourceKeys.DocumentWellTabButtonSelectedInactiveGlyph] = Brush("FileTabButtonSelectedInactiveGlyph");
			d[ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredGlyph] = Brush("FileTabHotGlyph");

			// Document well — close button hover states (selected active)
			d[ResourceKeys.DocumentWellTabButtonSelectedActiveHoveredBackground] = Brush("FileTabButtonHoverSelectedActive");
			d[ResourceKeys.DocumentWellTabButtonSelectedActiveHoveredBorder] = Brush("FileTabButtonHoverSelectedActiveBorder");
			d[ResourceKeys.DocumentWellTabButtonSelectedActiveHoveredGlyph] = Brush("FileTabButtonHoverSelectedActiveGlyph");
			d[ResourceKeys.DocumentWellTabButtonSelectedActivePressedBackground] = Brush("FileTabButtonDownSelectedActive");
			d[ResourceKeys.DocumentWellTabButtonSelectedActivePressedBorder] = Brush("FileTabButtonDownSelectedActiveBorder");
			d[ResourceKeys.DocumentWellTabButtonSelectedActivePressedGlyph] = Brush("FileTabButtonDownSelectedActiveGlyph");

			// Document well — close button hover states (selected inactive)
			d[ResourceKeys.DocumentWellTabButtonSelectedInactiveHoveredBackground] = Brush("FileTabButtonHoverSelectedInactive");
			d[ResourceKeys.DocumentWellTabButtonSelectedInactiveHoveredBorder] = Brush("FileTabButtonHoverSelectedInactiveBorder");
			d[ResourceKeys.DocumentWellTabButtonSelectedInactiveHoveredGlyph] = Brush("FileTabButtonHoverSelectedInactiveGlyph");
			d[ResourceKeys.DocumentWellTabButtonSelectedInactivePressedBackground] = Brush("FileTabButtonDownSelectedInactive");
			d[ResourceKeys.DocumentWellTabButtonSelectedInactivePressedBorder] = Brush("FileTabButtonDownSelectedInactiveBorder");
			d[ResourceKeys.DocumentWellTabButtonSelectedInactivePressedGlyph] = Brush("FileTabButtonDownSelectedInactiveGlyph");

			// Document well — close button hover states (unselected tab hovered)
			d[ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBackground] = Brush("FileTabButtonHoverInactive");
			d[ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBorder] = Brush("FileTabButtonHoverInactiveBorder");
			d[ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonHoveredGlyph] = Brush("FileTabButtonHoverInactiveGlyph");
			d[ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonPressedBackground] = Brush("FileTabButtonDownInactive");
			d[ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonPressedBorder] = Brush("FileTabButtonDownInactiveBorder");
			d[ResourceKeys.DocumentWellTabButtonUnselectedTabHoveredButtonPressedGlyph] = Brush("FileTabButtonDownInactiveGlyph");

			// Tool window captions
			d[ResourceKeys.ToolWindowCaptionActiveBackground] = Brush("TitleBarActiveBorder");
			d[ResourceKeys.ToolWindowCaptionActiveGrip] = Brush("TitleBarDragHandleActive");
			d[ResourceKeys.ToolWindowCaptionActiveText] = Brush("TitleBarActiveText");
			d[ResourceKeys.ToolWindowCaptionInactiveBackground] = Brush("TitleBarInactive");
			d[ResourceKeys.ToolWindowCaptionInactiveGrip] = Brush("TitleBarDragHandle");
			d[ResourceKeys.ToolWindowCaptionInactiveText] = Brush("TitleBarInactiveText");

			d[ResourceKeys.ToolWindowCaptionButtonActiveGlyph] = Brush("ToolWindowButtonActiveGlyph");
			d[ResourceKeys.ToolWindowCaptionButtonInactiveGlyph] = Brush("ToolWindowButtonInactiveGlyph");

			d[ResourceKeys.ToolWindowCaptionButtonActiveHoveredBackground] = Brush("ToolWindowButtonHoverActive");
			d[ResourceKeys.ToolWindowCaptionButtonActiveHoveredBorder] = Brush("ToolWindowButtonHoverActiveBorder");
			d[ResourceKeys.ToolWindowCaptionButtonActiveHoveredGlyph] = Brush("ToolWindowButtonHoverActiveGlyph");
			d[ResourceKeys.ToolWindowCaptionButtonActivePressedBackground] = Brush("ToolWindowButtonDown");
			d[ResourceKeys.ToolWindowCaptionButtonActivePressedBorder] = Brush("ToolWindowButtonDownBorder");
			d[ResourceKeys.ToolWindowCaptionButtonActivePressedGlyph] = Brush("ToolWindowButtonDownActiveGlyph");

			d[ResourceKeys.ToolWindowCaptionButtonInactiveHoveredBackground] = Brush("ToolWindowButtonHoverInactive");
			d[ResourceKeys.ToolWindowCaptionButtonInactiveHoveredBorder] = Brush("ToolWindowButtonHoverInactiveBorder");
			d[ResourceKeys.ToolWindowCaptionButtonInactiveHoveredGlyph] = Brush("ToolWindowButtonHoverInactiveGlyph");
			d[ResourceKeys.ToolWindowCaptionButtonInactivePressedBackground] = Brush("ToolWindowButtonDown");
			d[ResourceKeys.ToolWindowCaptionButtonInactivePressedBorder] = Brush("ToolWindowButtonDownBorder");
			d[ResourceKeys.ToolWindowCaptionButtonInactivePressedGlyph] = Brush("ToolWindowButtonDownActiveGlyph");

			// Tool window tabs
			d[ResourceKeys.ToolWindowTabSelectedActiveBackground] = Brush("ToolWindowTabSelectedTab");
			d[ResourceKeys.ToolWindowTabSelectedActiveText] = Brush("ToolWindowTabSelectedActiveText");
			d[ResourceKeys.ToolWindowTabSelectedInactiveBackground] = Brush("ToolWindowTabSelectedTab");
			d[ResourceKeys.ToolWindowTabSelectedInactiveText] = Brush("ToolWindowTabSelectedText");
			d[ResourceKeys.ToolWindowTabUnselectedBackground] = Brush("ToolWindowTabGradientBegin");
			d[ResourceKeys.ToolWindowTabUnselectedText] = Brush("ToolWindowTabText");
			d[ResourceKeys.ToolWindowTabUnselectedHoveredBackground] = Brush("ToolWindowTabMouseOverBackgroundBegin");
			d[ResourceKeys.ToolWindowTabUnselectedHoveredText] = Brush("ToolWindowTabMouseOverText");

			// Floating windows — same general palette as the main frame
			d[ResourceKeys.FloatingDocumentWindowBackground] = Brush("AutoHideTabBackgroundBegin");
			d[ResourceKeys.FloatingDocumentWindowBorder] = Brush("AutoHideTabBorder");
			d[ResourceKeys.FloatingToolWindowBackground] = Brush("AutoHideTabBackgroundBegin");
			d[ResourceKeys.FloatingToolWindowBorder] = Brush("AutoHideTabBorder");

			// Navigator window
			d[ResourceKeys.NavigatorWindowBackground] = Brush("AutoHideTabBackgroundBegin");
			d[ResourceKeys.NavigatorWindowForeground] = Raw(0xFF, 0x80, 0x80, 0x80);
			d[ResourceKeys.NavigatorWindowSelectedBackground] = Brush("FileTabSelectedBorder");
			d[ResourceKeys.NavigatorWindowSelectedText] = Brush("FileTabSelectedText");

			// Dock indicator / drop-zone
			d[ResourceKeys.DockingButtonForegroundBrushKey] = Brush("DockTargetGlyphBorder");
			d[ResourceKeys.DockingButtonForegroundArrowBrushKey] = Brush("DockTargetGlyphArrow");
			d[ResourceKeys.PreviewBoxBorderBrushKey] = Brush("DockTargetGlyphBorder");
			d[ResourceKeys.PreviewBoxBackgroundBrushKey] = BrushWithAlpha("DockTargetGlyphBorder", 0x80);

			d[ResourceKeys.DockingButtonBackgroundBrushKey] = Raw(0x20, 0x00, 0x00, 0x00);
			d[ResourceKeys.DockingButtonStarBorderBrushKey] = Raw(0x40, 0x80, 0x80, 0x80);
			d[ResourceKeys.DockingButtonStarBackgroundBrushKey] = Raw(0x20, 0x00, 0x00, 0x00);
		}

		private SolidColorBrush Brush(string key)
			=> new SolidColorBrush(_palette.GetBackgroundOrDefault(key, Colors.Transparent));

		private SolidColorBrush BrushWithAlpha(string key, byte alpha)
		{
			var c = _palette.GetBackgroundOrDefault(key, Colors.Transparent);
			return new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
		}

		private static SolidColorBrush Raw(byte a, byte r, byte g, byte b)
			=> new SolidColorBrush(Color.FromArgb(a, r, g, b));
	}
}