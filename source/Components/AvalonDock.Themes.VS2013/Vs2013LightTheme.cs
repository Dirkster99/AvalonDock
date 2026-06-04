namespace AvalonDock.Themes
{
	/// <summary>
	/// VS2013 Light theme, built at runtime from the embedded GZIP-compressed .vstheme palette.
	/// </summary>
	public class Vs2013LightTheme : DictionaryTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Vs2013LightTheme"/> class.
		/// </summary>
		public Vs2013LightTheme()
			: base(VsThemePaletteFactory.BuildDictionary(VsThemeResources.Light))
		{
		}
	}
}