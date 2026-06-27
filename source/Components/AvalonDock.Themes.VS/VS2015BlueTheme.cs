using System.IO;
using System.Reflection;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// VS2015 Blue theme loaded from an embedded .vstheme resource file.
	/// </summary>
	public class VS2015BlueTheme : VsTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VS2015BlueTheme"/> class.
		/// </summary>
		public VS2015BlueTheme()
			: base(LoadEmbeddedResource("AvalonDock.Themes.Resources.vs2015blue.vstheme"))
		{
		}

		private static Stream LoadEmbeddedResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		}
	}
}