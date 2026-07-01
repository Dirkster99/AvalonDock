using System;

namespace AvalonDock.Themes;

/// <summary>
/// Implements the Arc Light theme for AvalonDock.
/// Modern theme with compact tabs, rounded corners, and semi-transparent design.
/// </summary>
public class ArcLightTheme : Theme
{
	/// <inheritdoc/>
	public override Uri GetResourceUri()
	{
		return new Uri(
			"/AvalonDock.Themes.Arc;component/LightTheme.xaml",
			UriKind.Relative);
	}
}