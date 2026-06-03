using System;

namespace AvalonDock.Themes;

/// <inheritdoc/>
public class ExpressionLightTheme : Theme
{
	/// <inheritdoc/>
	public override Uri GetResourceUri()
	{
		return new Uri(
			"/AvalonDock.Themes.Expression;component/LightTheme.xaml",
			UriKind.Relative);
	}
}