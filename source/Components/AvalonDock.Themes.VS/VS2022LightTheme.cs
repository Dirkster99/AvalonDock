using System;
using System.IO;
using System.Reflection;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// VS2022 Light theme loaded from an embedded .vstheme resource file.
	/// Uses VS2022-specific control templates with rounded corners and modern tab design.
	/// </summary>
	public class VS2022LightTheme : VsTheme
	{
		private static readonly Uri VS2022GenericXamlUri =
			new Uri("/AvalonDock.Themes.VS;component/Themes/VS2022/Generic.xaml", UriKind.Relative);

		/// <summary>
		/// Initializes a new instance of the <see cref="VS2022LightTheme"/> class.
		/// </summary>
		public VS2022LightTheme()
			: base(LoadEmbeddedResource("AvalonDock.Themes.Resources.vs2022light.vstheme"), VS2022GenericXamlUri)
		{
		}

		private static Stream LoadEmbeddedResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		}
	}
}