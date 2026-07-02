using System;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// VS2026 Dark theme. Built from the VS2022 Dark base palette with a VS2026 JSON
	/// override that applies the Fluent accent and the new independent header/tab colors.
	/// </summary>
	public class VS2026DarkTheme : VsTheme
	{
		private static readonly Uri VS2026GenericXamlUri =
			new Uri("/AvalonDock.Themes.VS;component/Themes/VS2022/Generic.xaml", UriKind.Relative);

		/// <summary>
		/// Initializes a new instance of the <see cref="VS2026DarkTheme"/> class.
		/// </summary>
		public VS2026DarkTheme()
			: base(Vs2026ThemeFactory.Build(
				"AvalonDock.Themes.Resources.vs2022dark.vstheme",
				"AvalonDock.Themes.Resources.vs2026dark.json",
				VS2026GenericXamlUri))
		{
		}
	}
}