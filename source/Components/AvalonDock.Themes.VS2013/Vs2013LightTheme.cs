using System;

namespace AvalonDock.Themes;

/// <inheritdoc/>
public class Vs2013LightTheme : Theme
{
	/// <inheritdoc/>
	public override Uri GetResourceUri()
	{
		return new Uri(
			"/AvalonDock.Themes.VS2013;component/LightTheme.xaml",
			UriKind.Relative);
	}
}