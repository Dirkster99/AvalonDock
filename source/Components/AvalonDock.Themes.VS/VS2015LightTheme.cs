using System.IO;
using System.Reflection;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// VS2015 Light theme loaded from an embedded .vstheme resource file.
	/// </summary>
	public class VS2015LightTheme : VsTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VS2015LightTheme"/> class.
		/// </summary>
		public VS2015LightTheme()
			: base(LoadEmbeddedResource("AvalonDock.Themes.Resources.vs2015light.vstheme"))
		{
		}

		private static Stream LoadEmbeddedResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		}
	}
}