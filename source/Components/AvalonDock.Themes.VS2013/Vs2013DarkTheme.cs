namespace AvalonDock.Themes
{
	/// <summary>
	/// VS2013 Dark theme, built at runtime from the embedded GZIP-compressed .vstheme palette.
	/// </summary>
	public class Vs2013DarkTheme : DictionaryTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Vs2013DarkTheme"/> class.
		/// </summary>
		public Vs2013DarkTheme()
			: base(VsThemePaletteFactory.BuildDictionary(VsThemeResources.Dark))
		{
		}
	}
}