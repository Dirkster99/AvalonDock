using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Parses Visual Studio .vstheme XML files and extracts color definitions
	/// from the Environment category.
	/// </summary>
	public static class VsThemeParser
	{
		private const string EnvironmentCategory = "Environment";

		/// <summary>
		/// Parses a .vstheme file from a stream and returns the color palette.
		/// The stream may contain either plain .vstheme XML or GZIP-compressed
		/// .vstheme XML; the compression is detected automatically.
		/// </summary>
		/// <param name="stream">The stream containing the (optionally GZIP-compressed) .vstheme XML content.</param>
		/// <returns>A dictionary mapping color token names to their resolved colors.</returns>
		public static VsThemeColorPalette Parse(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			byte[] bytes;
			using (var buffer = new MemoryStream())
			{
				stream.CopyTo(buffer);
				bytes = buffer.ToArray();
			}

			return ParseBytes(bytes);
		}

		/// <summary>
		/// Parses a GZIP-compressed .vstheme file from its raw compressed bytes
		/// and returns the color palette.
		/// </summary>
		/// <param name="gzipBytes">The GZIP-compressed .vstheme XML content.</param>
		/// <returns>A dictionary mapping color token names to their resolved colors.</returns>
		public static VsThemeColorPalette ParseGZip(byte[] gzipBytes)
		{
			if (gzipBytes == null)
			{
				throw new ArgumentNullException(nameof(gzipBytes));
			}

			using (var ms = new MemoryStream(gzipBytes))
			using (var gz = new GZipStream(ms, CompressionMode.Decompress))
			{
				var doc = XDocument.Load(gz);
				return ParseDocument(doc);
			}
		}

		/// <summary>
		/// Parses a .vstheme file from a file path and returns the color palette.
		/// </summary>
		/// <param name="filePath">The path to the .vstheme file.</param>
		/// <returns>A dictionary mapping color token names to their resolved colors.</returns>
		public static VsThemeColorPalette ParseFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			using (var stream = File.OpenRead(filePath))
			{
				return Parse(stream);
			}
		}

		private static VsThemeColorPalette ParseBytes(byte[] bytes)
		{
			// GZIP streams start with the magic bytes 0x1F 0x8B.
			if (bytes.Length >= 2 && bytes[0] == 0x1F && bytes[1] == 0x8B)
			{
				return ParseGZip(bytes);
			}

			using (var ms = new MemoryStream(bytes))
			{
				var doc = XDocument.Load(ms);
				return ParseDocument(doc);
			}
		}

		private static VsThemeColorPalette ParseDocument(XDocument doc)
		{
			var colors = new Dictionary<string, VsThemeColorEntry>();
			var themeElement = doc.Root?.Element("Theme");
			if (themeElement == null)
			{
				return new VsThemeColorPalette(colors);
			}

			var envCategory = themeElement
				.Elements("Category")
				.FirstOrDefault(c => string.Equals(
					c.Attribute("Name")?.Value,
					EnvironmentCategory,
					StringComparison.OrdinalIgnoreCase));

			if (envCategory == null)
			{
				return new VsThemeColorPalette(colors);
			}

			foreach (var colorElement in envCategory.Elements("Color"))
			{
				var name = colorElement.Attribute("Name")?.Value;
				if (string.IsNullOrEmpty(name))
				{
					continue;
				}

				var bg = ParseColorElement(colorElement.Element("Background"));
				var fg = ParseColorElement(colorElement.Element("Foreground"));

				colors[name] = new VsThemeColorEntry(bg, fg);
			}

			return new VsThemeColorPalette(colors);
		}

		private static Color? ParseColorElement(XElement element)
		{
			if (element == null)
			{
				return null;
			}

			var type = element.Attribute("Type")?.Value;
			if (type != "CT_RAW")
			{
				return null;
			}

			var source = element.Attribute("Source")?.Value;
			return VsColorParser.FromArgbHex(source);
		}
	}
}