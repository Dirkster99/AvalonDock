using System;
using System.IO;
using System.Windows;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Base class for themes that load their colors from a .vstheme file.
	/// Extends <see cref="DictionaryTheme"/> to build the <see cref="ResourceDictionary"/>
	/// at runtime from parsed VS theme colors.
	/// </summary>
	public class VsTheme : DictionaryTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VsTheme"/> class
		/// from a .vstheme file stream.
		/// </summary>
		/// <param name="stream">The stream containing the .vstheme XML content.</param>
		public VsTheme(Stream stream)
			: base(BuildFromStream(stream))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VsTheme"/> class
		/// from a .vstheme file path.
		/// </summary>
		/// <param name="filePath">The path to the .vstheme file.</param>
		public VsTheme(string filePath)
			: base(BuildFromFile(filePath))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VsTheme"/> class
		/// from GZIP-compressed .vstheme bytes (for example, an embedded .vstheme.gz resource).
		/// </summary>
		/// <param name="gzipBytes">The GZIP-compressed .vstheme XML content.</param>
		public VsTheme(byte[] gzipBytes)
			: base(VsThemeResourceBuilder.Build(VsThemeParser.ParseGZip(gzipBytes)))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VsTheme"/> class
		/// from a pre-built <see cref="VsThemeColorPalette"/>.
		/// </summary>
		/// <param name="palette">The parsed color palette.</param>
		public VsTheme(VsThemeColorPalette palette)
			: base(VsThemeResourceBuilder.Build(palette))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VsTheme"/> class
		/// from a .vstheme file stream, using an alternate Generic.xaml for control templates.
		/// </summary>
		/// <param name="stream">The stream containing the .vstheme XML content.</param>
		/// <param name="genericXamlUri">Pack URI for the Generic.xaml resource dictionary to merge.</param>
		protected VsTheme(Stream stream, Uri genericXamlUri)
			: base(BuildFromStream(stream, genericXamlUri))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VsTheme"/> class
		/// from a pre-built resource dictionary.
		/// </summary>
		/// <param name="resourceDictionary">The resource dictionary.</param>
		protected VsTheme(ResourceDictionary resourceDictionary)
			: base(resourceDictionary)
		{
		}

		/// <summary>
		/// Creates a <see cref="VsTheme"/> from a Visual Studio 2026 JSON theme file.
		/// </summary>
		/// <remarks>
		/// VS2026 JSON theme files are override only: they list just the color tokens that
		/// were changed. Pass <paramref name="basePalette"/> (for example, the palette of a
		/// built-in VS theme) to layer the overrides on top of a full base; if omitted, only
		/// the tokens defined in the file are used and all other colors fall back to the
		/// resource builder's defaults.
		/// </remarks>
		/// <param name="filePath">The path to the VS2026 JSON theme file.</param>
		/// <param name="basePalette">An optional base palette to merge the overrides onto.</param>
		/// <param name="genericXamlUri">An optional pack URI for the Generic.xaml control templates to merge.</param>
		/// <returns>A theme built from the merged palette.</returns>
		public static VsTheme FromJson(string filePath, VsThemeColorPalette basePalette = null, Uri genericXamlUri = null)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			return FromPalette(VsJsonThemeParser.ParseFile(filePath), basePalette, genericXamlUri);
		}

		/// <summary>
		/// Creates a <see cref="VsTheme"/> from a Visual Studio 2026 JSON theme stream.
		/// </summary>
		/// <param name="jsonStream">The stream containing the VS2026 JSON theme content.</param>
		/// <param name="basePalette">An optional base palette to merge the overrides onto.</param>
		/// <param name="genericXamlUri">An optional pack URI for the Generic.xaml control templates to merge.</param>
		/// <returns>A theme built from the merged palette.</returns>
		public static VsTheme FromJson(Stream jsonStream, VsThemeColorPalette basePalette = null, Uri genericXamlUri = null)
		{
			if (jsonStream == null)
			{
				throw new ArgumentNullException(nameof(jsonStream));
			}

			return FromPalette(VsJsonThemeParser.Parse(jsonStream), basePalette, genericXamlUri);
		}

		private static VsTheme FromPalette(VsThemeColorPalette overrides, VsThemeColorPalette basePalette, Uri genericXamlUri)
		{
			var palette = basePalette != null ? basePalette.Merge(overrides) : overrides;
			var dict = genericXamlUri != null
				? VsThemeResourceBuilder.Build(palette, genericXamlUri)
				: VsThemeResourceBuilder.Build(palette);
			return new VsTheme(dict);
		}

		private static ResourceDictionary BuildFromStream(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			var palette = VsThemeParser.Parse(stream);
			return VsThemeResourceBuilder.Build(palette);
		}

		private static ResourceDictionary BuildFromStream(Stream stream, Uri genericXamlUri)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			var palette = VsThemeParser.Parse(stream);
			return VsThemeResourceBuilder.Build(palette, genericXamlUri);
		}

		private static ResourceDictionary BuildFromFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			var palette = VsThemeParser.ParseFile(filePath);
			return VsThemeResourceBuilder.Build(palette);
		}
	}
}