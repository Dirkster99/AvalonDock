using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
		/// </summary>
		/// <param name="stream">The stream containing the .vstheme XML content.</param>
		/// <returns>A dictionary mapping color token names to their resolved colors.</returns>
		public static VsThemeColorPalette Parse(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			var doc = XDocument.Load(stream);
			return ParseDocument(doc);
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
			if (string.IsNullOrEmpty(source) || source.Length != 8)
			{
				return null;
			}

			if (!byte.TryParse(source.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a) ||
				!byte.TryParse(source.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
				!byte.TryParse(source.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
				!byte.TryParse(source.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
			{
				return null;
			}

			return Color.FromArgb(a, r, g, b);
		}
	}
}