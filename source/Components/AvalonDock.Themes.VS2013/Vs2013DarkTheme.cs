using System;

namespace AvalonDock.Themes;

/// <inheritdoc/>
public class Vs2013DarkTheme : Theme
{
	/// <inheritdoc/>
	public override Uri GetResourceUri()
	{
		return new Uri(
			"/AvalonDock.Themes.VS2013;component/DarkTheme.xaml",
			UriKind.Relative);
	}
}