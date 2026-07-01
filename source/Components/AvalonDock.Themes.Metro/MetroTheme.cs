using System;

namespace AvalonDock.Themes;

/// <inheritdoc/>
public class MetroTheme : Theme
{
	/// <inheritdoc/>
	public override Uri GetResourceUri()
	{
		return new Uri(
			"/AvalonDock.Themes.Metro;component/Theme.xaml",
			UriKind.Relative);
	}
}