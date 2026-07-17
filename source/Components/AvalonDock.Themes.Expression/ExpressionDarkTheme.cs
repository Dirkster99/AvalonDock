using System;

namespace AvalonDock.Themes;

/// <inheritdoc/>
public class ExpressionDarkTheme : Theme
{
	/// <inheritdoc/>
	public override Uri GetResourceUri()
	{
		return new Uri(
			"/AvalonDock.Themes.Expression;component/DarkTheme.xaml",
			UriKind.Relative);
	}
}