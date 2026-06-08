using System.IO;
using System.Reflection;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// VS2022 Blue theme loaded from an embedded .vstheme resource file.
	/// </summary>
	public class VS2022BlueTheme : VsTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VS2022BlueTheme"/> class.
		/// </summary>
		public VS2022BlueTheme()
			: base(LoadEmbeddedResource("AvalonDock.Themes.VS.Resources.vs2022blue.vstheme"))
		{
		}

		private static Stream LoadEmbeddedResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		}
	}
}