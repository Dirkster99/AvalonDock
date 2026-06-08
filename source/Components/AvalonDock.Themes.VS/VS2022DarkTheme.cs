using System.IO;
using System.Reflection;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// VS2022 Dark theme loaded from an embedded .vstheme resource file.
	/// </summary>
	public class VS2022DarkTheme : VsTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VS2022DarkTheme"/> class.
		/// </summary>
		public VS2022DarkTheme()
			: base(LoadEmbeddedResource("AvalonDock.Themes.VS.Resources.vs2022dark.vstheme"))
		{
		}

		private static Stream LoadEmbeddedResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		}
	}
}