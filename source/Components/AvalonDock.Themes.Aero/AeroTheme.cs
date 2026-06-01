using System;

namespace AvalonDock.Themes
{
	/// <inheritdoc/>
	public class AeroTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri(
				"/AvalonDock.Themes.Aero;component/Theme.xaml",
				UriKind.Relative);
		}
	}
}