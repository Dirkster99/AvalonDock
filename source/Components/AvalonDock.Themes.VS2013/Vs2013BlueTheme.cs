using System;

namespace AvalonDock.Themes
{
	/// <inheritdoc/>
	public class Vs2013BlueTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri(
				"/AvalonDock.Themes.VS2013;component/BlueTheme.xaml",
				UriKind.Relative);
		}
	}
}