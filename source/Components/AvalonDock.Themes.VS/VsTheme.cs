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