namespace AvalonDock.Themes
{
	/// <summary>
	/// VS2013 Blue theme, built at runtime from the embedded GZIP-compressed .vstheme palette.
	/// </summary>
	public class Vs2013BlueTheme : DictionaryTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Vs2013BlueTheme"/> class.
		/// </summary>
		public Vs2013BlueTheme()
			: base(VsThemePaletteFactory.BuildDictionary(VsThemeResources.Blue))
		{
		}
	}
}