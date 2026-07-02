using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Parses Visual Studio 2026 JSON theme files and extracts color definitions.
	/// </summary>
	/// <remarks>
	/// Visual Studio 2026 replaced the XML <c>.vstheme</c> format with a JSON based
	/// override format. A file is a flat array of color entries, for example:
	/// <code>
	/// [
	///   { "Name": "EnvironmentBackground", "Category": "5af241b7-...", "Background": "FFCCD5F0" },
	///   { "Name": "EnvironmentHeader",     "Category": "5af241b7-...", "Background": "FFF5CC84" }
	/// ]
	/// </code>
	/// Each entry carries the classic VS color token name, an (ignored) category GUID,
	/// and an 8-digit AARRGGBB color value. Such files are <em>override only</em>: they
	/// list only the tokens that were changed, so they are normally layered on top of a
	/// base palette via <see cref="VsThemeColorPalette.Merge"/>.
	/// </remarks>
	public static class VsJsonThemeParser
	{
		private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true,
		};

		/// <summary>
		/// Parses a VS2026 JSON theme from a string and returns the color palette.
		/// </summary>
		/// <param name="json">The JSON content.</param>
		/// <returns>A palette mapping color token names to their resolved colors.</returns>
		public static VsThemeColorPalette Parse(string json)
		{
			if (json == null)
			{
				throw new ArgumentNullException(nameof(json));
			}

			var entries = JsonSerializer.Deserialize<List<JsonColorEntry>>(json, Options);
			return Build(entries);
		}

		/// <summary>
		/// Parses a VS2026 JSON theme from a stream and returns the color palette.
		/// </summary>
		/// <param name="stream">The stream containing the JSON content.</param>
		/// <returns>A palette mapping color token names to their resolved colors.</returns>
		public static VsThemeColorPalette Parse(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			using (var reader = new StreamReader(stream))
			{
				return Parse(reader.ReadToEnd());
			}
		}

		/// <summary>
		/// Parses a VS2026 JSON theme from a file path and returns the color palette.
		/// </summary>
		/// <param name="filePath">The path to the JSON theme file.</param>
		/// <returns>A palette mapping color token names to their resolved colors.</returns>
		public static VsThemeColorPalette ParseFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			return Parse(File.ReadAllText(filePath));
		}

		private static VsThemeColorPalette Build(List<JsonColorEntry> entries)
		{
			var colors = new Dictionary<string, VsThemeColorEntry>();
			if (entries == null)
			{
				return new VsThemeColorPalette(colors);
			}

			foreach (var entry in entries)
			{
				if (entry == null || string.IsNullOrEmpty(entry.Name))
				{
					continue;
				}

				var bg = VsColorParser.FromArgbHex(entry.Background);
				var fg = VsColorParser.FromArgbHex(entry.Foreground);

				// Last entry wins for a duplicated token name.
				colors[entry.Name] = new VsThemeColorEntry(bg, fg);
			}

			return new VsThemeColorPalette(colors);
		}

		private sealed class JsonColorEntry
		{
			[JsonPropertyName("Name")]
			public string Name { get; set; }

			[JsonPropertyName("Category")]
			public string Category { get; set; }

			[JsonPropertyName("Background")]
			public string Background { get; set; }

			[JsonPropertyName("Foreground")]
			public string Foreground { get; set; }
		}
	}
}