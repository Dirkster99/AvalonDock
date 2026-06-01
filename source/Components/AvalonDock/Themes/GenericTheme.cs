using System;

namespace AvalonDock.Themes
{
	/// <inheritdoc/>
	public class GenericTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri("/AvalonDock;component/Themes/generic.xaml", UriKind.Relative);
		}
	}
}