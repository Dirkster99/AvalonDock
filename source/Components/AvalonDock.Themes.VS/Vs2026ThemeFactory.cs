using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Builds the resource dictionary for a bundled VS2026 theme by layering an embedded
	/// VS2026 JSON override file on top of a full VS2022 base palette.
	/// </summary>
	/// <remarks>
	/// VS2026 ships themes as JSON override files that only contain the tokens that differ
	/// from a base theme. Until full VS2026 palette dumps are available, the bundled VS2026
	/// themes reuse the existing VS2022 <c>.vstheme</c> palettes as their base and apply a
	/// small VS2026 override (Fluent accent plus the new independent header/tab tokens).
	/// </remarks>
	internal static class Vs2026ThemeFactory
	{
		/// <summary>
		/// Builds a resource dictionary by merging a base VS theme palette with a VS2026 JSON override.
		/// </summary>
		/// <param name="baseVsThemeResource">The embedded resource name of the base VS theme file (e.g., VS2022 .vstheme).</param>
		/// <param name="jsonOverrideResource">The embedded resource name of the VS2026 JSON override file containing token overrides.</param>
		/// <param name="genericXamlUri">The URI to the generic XAML resource dictionary for theme styling.</param>
		/// <returns>A <see cref="ResourceDictionary"/> containing the merged theme colors and styles.</returns>
		/// <exception cref="InvalidOperationException">Thrown if either the base or override resource cannot be found.</exception>
		public static ResourceDictionary Build(string baseVsThemeResource, string jsonOverrideResource, Uri genericXamlUri)
		{
			VsThemeColorPalette basePalette;
			using (var baseStream = OpenResource(baseVsThemeResource))
			{
				basePalette = VsThemeParser.Parse(baseStream);
			}

			VsThemeColorPalette overrides;
			using (var jsonStream = OpenResource(jsonOverrideResource))
			{
				overrides = VsJsonThemeParser.Parse(jsonStream);
			}

			var merged = basePalette.Merge(overrides);
			return VsThemeResourceBuilder.Build(merged, genericXamlUri);
		}

		/// <summary>
		/// Opens an embedded manifest resource stream from the current assembly.
		/// </summary>
		/// <param name="name">The fully qualified name of the embedded resource.</param>
		/// <returns>A <see cref="Stream"/> containing the resource data.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the resource with the specified name does not exist.</exception>
		private static Stream OpenResource(string name)
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
			if (stream == null)
			{
				throw new InvalidOperationException($"Embedded resource '{name}' was not found.");
			}

			return stream;
		}
	}
}