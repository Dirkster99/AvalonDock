using System;
using System.IO;
using System.Reflection;
using System.Windows.Markup;

namespace AvalonDock.Themes.VS;

/// <summary>
/// Markup extension to create a <see cref="VsTheme"/> from XAML.
/// </summary>
[MarkupExtensionReturnType(typeof(VsTheme))]
public class VsThemeExtension : MarkupExtension
{
	/// <summary>
	/// Gets or sets the theme path (absolute or relative to the application directory
	/// </summary>
	public string Path { get; set; }

	/// <inheritdoc />
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		var path = System.IO.Path.IsPathRooted(Path)
			? Path
			: System.IO.Path.GetFullPath(
				System.IO.Path.Combine(
					Directory.GetParent(Assembly.GetEntryAssembly().Location)?.FullName ?? ".",
					Path));
		return new VsTheme(path);
	}
}