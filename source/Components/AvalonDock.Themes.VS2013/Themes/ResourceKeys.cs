namespace AvalonDock.Themes.VS2013.Themes
{
	using System.Windows;

	/// <summary>
	/// Resource key management class to keep track of all resources
	/// that can be re-styled in applications that make use of the implemented controls.
	/// </summary>
	public static class ResourceKeys
	{
		/// <summary>
		/// Accent Color Key - This Color key is used to accent elements in the UI
		/// (e.g.: Color of Activated Normal Window Frame, ResizeGrip, Focus or MouseOver input elements)
		/// </summary>
		public static readonly ComponentResourceKey ControlAccentColorKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlAccentColorKey");

		/// <summary>
		/// Accent Brush Key - This Brush key is used to accent elements in the UI
		/// (e.g.: Color of Activated Normal Window Frame, ResizeGrip, Focus or MouseOver input elements)
		/// </summary>
		public static readonly ComponentResourceKey ControlAccentBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlAccentBrushKey");

		// General

		/// <summary>
		/// Gets the resource key for the background brush.
		/// </summary>
		public static readonly ComponentResourceKey Background = new ComponentResourceKey(typeof(ResourceKeys), "Background");

		/// <summary>
		/// Gets the resource key for the panel border brush.
		/// </summary>
		public static readonly ComponentResourceKey PanelBorderBrush = new ComponentResourceKey(typeof(ResourceKeys), "PanelBorderBrush");

		/// <summary>
		/// Gets the resource key for the tab background brush.
		/// </summary>
		public static readonly ComponentResourceKey TabBackground = new ComponentResourceKey(typeof(ResourceKeys), "TabBackground");

		// Auto Hide : Tab

		/// <summary>
		/// Gets the resource key for the auto hide tab default background brush.
		/// </summary>
		public static readonly ComponentResourceKey AutoHideTabDefaultBackground = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabDefaultBackground");

		/// <summary>
		/// Gets the resource key for the auto hide tab default border brush.
		/// </summary>
		public static readonly ComponentResourceKey AutoHideTabDefaultBorder = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabDefaultBorder");

		/// <summary>
		/// Gets the resource key for the auto hide tab default text brush.
		/// </summary>
		public static readonly ComponentResourceKey AutoHideTabDefaultText = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabDefaultText");

		// Mouse Over Auto Hide Button for (collapsed) Auto Hidden Elements

		/// <summary>
		/// Gets the resource key for the auto hide tab hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey AutoHideTabHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabHoveredBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the auto hide tab hovered border brush.
		/// </summary>
		public static readonly ComponentResourceKey AutoHideTabHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabHoveredBorder");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the auto hide tab hovered text brush.
		/// </summary>
		public static readonly ComponentResourceKey AutoHideTabHoveredText = new ComponentResourceKey(typeof(ResourceKeys), "AutoHideTabHoveredText");

		// Document Well : Overflow Button

		/// <summary>
		/// Gets the resource key for the document well overflow button default glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellOverflowButtonDefaultGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonDefaultGlyph");

		/// <summary>
		/// Gets the resource key for the document well overflow button hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellOverflowButtonHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonHoveredBackground");

		/// <summary>
		/// Gets the resource key for the document well overflow button hovered border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellOverflowButtonHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonHoveredBorder");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well overflow button hovered glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellOverflowButtonHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonHoveredGlyph");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well overflow button pressed background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellOverflowButtonPressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonPressedBackground");

		/// <summary>
		/// Gets the resource key for the document well overflow button pressed border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellOverflowButtonPressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonPressedBorder");

		/// <summary>
		/// Gets the resource key for the document well overflow button pressed glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellOverflowButtonPressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellOverflowButtonPressedGlyph");

		// Document Well : Tab
		// Selected Document Highlight Header Top color (AccentColor)

		/// <summary>
		/// Gets the resource key for the document well tab selected active background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabSelectedActiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedActiveBackground");

		/// <summary>
		/// Gets the resource key for the document well tab selected active text brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabSelectedActiveText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedActiveText");

		/// <summary>
		/// Gets the resource key for the document well tab selected inactive background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabSelectedInactiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedInactiveBackground");

		/// <summary>
		/// Gets the resource key for the document well tab selected inactive text brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabSelectedInactiveText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabSelectedInactiveText");

		/// <summary>
		/// Gets the resource key for the document well tab unselected background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabUnselectedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedBackground");

		/// <summary>
		/// Gets the resource key for the document well tab unselected text brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabUnselectedText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedText");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab unselected hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabUnselectedHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedHoveredBackground");

		/// <summary>
		/// Gets the resource key for the document well tab unselected hovered text brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabUnselectedHoveredText = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabUnselectedHoveredText");

		// Document Well : Tab : Button

		/// <summary>
		/// Gets the resource key for the document well tab button selected active glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveGlyph");

		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button selected active hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveHoveredBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button selected active hovered border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveHoveredBorder");

		/// <summary>
		/// Gets the resource key for the document well tab button selected active hovered glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActiveHoveredGlyph");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button selected active pressed background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActivePressedBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button selected active pressed border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActivePressedBorder");

		/// <summary>
		/// Gets the resource key for the document well tab button selected active pressed glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedActivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedActivePressedGlyph");

		/// <summary>
		/// Gets the resource key for the document well tab button selected inactive glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveGlyph");

		/// <summary>
		/// Gets the resource key for the document well tab button selected inactive hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveHoveredBackground");

		/// <summary>
		/// Gets the resource key for the document well tab button selected inactive hovered border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveHoveredBorder");

		/// <summary>
		/// Gets the resource key for the document well tab button selected inactive hovered glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactiveHoveredGlyph");

		/// <summary>
		/// Gets the resource key for the document well tab button selected inactive pressed background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactivePressedBackground");

		/// <summary>
		/// Gets the resource key for the document well tab button selected inactive pressed border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactivePressedBorder");

		/// <summary>
		/// Gets the resource key for the document well tab button selected inactive pressed glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonSelectedInactivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonSelectedInactivePressedGlyph");

		/// <summary>
		/// Gets the resource key for the document well tab button unselected tab hovered glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredGlyph");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button unselected tab hovered button hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button unselected tab hovered button hovered border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonHoveredBorder");

		/// <summary>
		/// Gets the resource key for the document well tab button unselected tab hovered button hovered glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonHoveredGlyph");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button unselected tab hovered button pressed background brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonPressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonPressedBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the document well tab button unselected tab hovered button pressed border brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonPressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonPressedBorder");

		/// <summary>
		/// Gets the resource key for the document well tab button unselected tab hovered button pressed glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey DocumentWellTabButtonUnselectedTabHoveredButtonPressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "DocumentWellTabButtonUnselectedTabHoveredButtonPressedGlyph");

		// Tool Window : Caption
		// Background of selected toolwindow (AccentColor)

		/// <summary>
		/// Gets the resource key for the tool window caption active background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionActiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionActiveBackground");

		/// <summary>
		/// Gets the resource key for the tool window caption active grip brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionActiveGrip = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionActiveGrip");

		/// <summary>
		/// Gets the resource key for the tool window caption active text brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionActiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionActiveText");

		/// <summary>
		/// Gets the resource key for the tool window caption inactive background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionInactiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionInactiveBackground");

		/// <summary>
		/// Gets the resource key for the tool window caption inactive grip brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionInactiveGrip = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionInactiveGrip");

		/// <summary>
		/// Gets the resource key for the tool window caption inactive text brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionInactiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionInactiveText");

		// Tool Window : Caption : Button

		/// <summary>
		/// Gets the resource key for the tool window caption button active glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveGlyph");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window caption button active hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveHoveredBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window caption button active hovered border brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveHoveredBorder");

		/// <summary>
		/// Gets the resource key for the tool window caption button active hovered glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActiveHoveredGlyph");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window caption button active pressed background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActivePressedBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window caption button active pressed border brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActivePressedBorder");

		/// <summary>
		/// Gets the resource key for the tool window caption button active pressed glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonActivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonActivePressedGlyph");

		/// <summary>
		/// Gets the resource key for the tool window caption button inactive glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveGlyph");

		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window caption button inactive hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveHoveredBackground");

		/// <summary>
		/// Gets the resource key for the tool window caption button inactive hovered border brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveHoveredBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveHoveredBorder");

		/// <summary>
		/// Gets the resource key for the tool window caption button inactive hovered glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactiveHoveredGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactiveHoveredGlyph");

		/// <summary>
		/// Gets the resource key for the tool window caption button inactive pressed background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactivePressedBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactivePressedBackground");

		/// <summary>
		/// Gets the resource key for the tool window caption button inactive pressed border brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactivePressedBorder = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactivePressedBorder");

		/// <summary>
		/// Gets the resource key for the tool window caption button inactive pressed glyph brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowCaptionButtonInactivePressedGlyph = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowCaptionButtonInactivePressedGlyph");

		// Tool Window : Tab

		/// <summary>
		/// Gets the resource key for the tool window tab selected active background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabSelectedActiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedActiveBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window tab selected active text brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabSelectedActiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedActiveText");

		/// <summary>
		/// Gets the resource key for the tool window tab selected inactive background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabSelectedInactiveBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedInactiveBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window tab selected inactive text brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabSelectedInactiveText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabSelectedInactiveText");

		/// <summary>
		/// Gets the resource key for the tool window tab unselected background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabUnselectedBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedBackground");

		/// <summary>
		/// Gets the resource key for the tool window tab unselected text brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabUnselectedText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedText");

		/// <summary>
		/// Gets the resource key for the tool window tab unselected hovered background brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabUnselectedHoveredBackground = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedHoveredBackground");
		// AccentColor

		/// <summary>
		/// Gets the resource key for the tool window tab unselected hovered text brush.
		/// </summary>
		public static readonly ComponentResourceKey ToolWindowTabUnselectedHoveredText = new ComponentResourceKey(typeof(ResourceKeys), "ToolWindowTabUnselectedHoveredText");

		// Floating Document Window

		/// <summary>
		/// Gets the resource key for the floating document window background brush.
		/// </summary>
		public static readonly ComponentResourceKey FloatingDocumentWindowBackground = new ComponentResourceKey(typeof(ResourceKeys), "FloatingDocumentWindowBackground");

		/// <summary>
		/// Gets the resource key for the floating document window border brush.
		/// </summary>
		public static readonly ComponentResourceKey FloatingDocumentWindowBorder = new ComponentResourceKey(typeof(ResourceKeys), "FloatingDocumentWindowBorder");

		// Floating Tool Window

		/// <summary>
		/// Gets the resource key for the floating tool window background brush.
		/// </summary>
		public static readonly ComponentResourceKey FloatingToolWindowBackground = new ComponentResourceKey(typeof(ResourceKeys), "FloatingToolWindowBackground");

		/// <summary>
		/// Gets the resource key for the floating tool window border brush.
		/// </summary>
		public static readonly ComponentResourceKey FloatingToolWindowBorder = new ComponentResourceKey(typeof(ResourceKeys), "FloatingToolWindowBorder");

		// Navigator Window

		/// <summary>
		/// Gets the resource key for the navigator window background brush.
		/// </summary>
		public static readonly ComponentResourceKey NavigatorWindowBackground = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowBackground");

		/// <summary>
		/// Gets the resource key for the navigator window foreground brush.
		/// </summary>
		public static readonly ComponentResourceKey NavigatorWindowForeground = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowForeground");

		// Background of selected text in Navigator Window (AccentColor)

		/// <summary>
		/// Gets the resource key for the navigator window selected background brush.
		/// </summary>
		public static readonly ComponentResourceKey NavigatorWindowSelectedBackground = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowSelectedBackground");

		/// <summary>
		/// Gets the resource key for the navigator window selected text brush.
		/// </summary>
		public static readonly ComponentResourceKey NavigatorWindowSelectedText = new ComponentResourceKey(typeof(ResourceKeys), "NavigatorWindowSelectedText");

		// Defines the height and width of the docking indicator buttons that are shown when
		// documents or tool windows are dragged

		/// <summary>
		/// Gets the resource key for the docking button width.
		/// </summary>
		public static readonly ComponentResourceKey DockingButtonWidthKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonWidthKey");

		/// <summary>
		/// Gets the resource key for the docking button height.
		/// </summary>
		public static readonly ComponentResourceKey DockingButtonHeightKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonHeightKey");

		/// <summary>
		/// Gets the resource key for the docking button background brush.
		/// </summary>
		public static readonly ComponentResourceKey DockingButtonBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonBackgroundBrushKey");

		/// <summary>
		/// Gets the resource key for the docking button foreground brush.
		/// </summary>
		public static readonly ComponentResourceKey DockingButtonForegroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonForegroundBrushKey");

		/// <summary>
		/// Gets the resource key for the docking button foreground arrow brush.
		/// </summary>
		public static readonly ComponentResourceKey DockingButtonForegroundArrowBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonForegroundArrowBrushKey");

		/// <summary>
		/// Gets the resource key for the docking button star border brush.
		/// </summary>
		public static readonly ComponentResourceKey DockingButtonStarBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonStarBorderBrushKey");

		/// <summary>
		/// Gets the resource key for the docking button star background brush.
		/// </summary>
		public static readonly ComponentResourceKey DockingButtonStarBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "DockingButtonStarBackgroundBrushKey");

		// Preview Box is the highlighted rectangle that shows when a drop area in a window is indicated

		/// <summary>
		/// Gets the resource key for the preview box border brush.
		/// </summary>
		public static readonly ComponentResourceKey PreviewBoxBorderBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "PreviewBoxBorderBrushKey");

		/// <summary>
		/// Gets the resource key for the preview box background brush.
		/// </summary>
		public static readonly ComponentResourceKey PreviewBoxBackgroundBrushKey = new ComponentResourceKey(typeof(ResourceKeys), "PreviewBoxBackgroundBrushKey");
	}
}