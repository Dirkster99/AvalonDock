using System;

namespace AvalonDock.Themes
{
	/// <inheritdoc/>
	public class VS2010Theme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri(
				"/AvalonDock.Themes.VS2010;component/Theme.xaml",
				UriKind.Relative);
		}
	}
}