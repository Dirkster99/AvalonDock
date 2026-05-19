#nullable disable
using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// UI-agnostic interface for theme metadata.
	/// Implemented by the WPF <c>Theme</c> class and theme implementations.
	/// </summary>
	public interface IThemeInfo
	{
		/// <summary>Gets the display name of the theme.</summary>
		string Name { get; }

		/// <summary>Gets the resource URI for the theme's resource dictionary.</summary>
		Uri ResourceUri { get; }
	}

	/// <summary>
	/// Service interface for managing themes in the docking system.
	/// </summary>
	/// <remarks>
	/// <para>Register an implementation via DI to enable theme switching from ViewModels.</para>
	/// <para>The WPF implementation manages ResourceDictionary merging internally.</para>
	/// </remarks>
	public interface IThemeManager
	{
		/// <summary>Gets the name of the currently active theme, or null if none.</summary>
		string CurrentThemeName { get; }

		/// <summary>Gets the available themes.</summary>
		IReadOnlyList<IThemeInfo> AvailableThemes { get; }

		/// <summary>Applies the specified theme by name.</summary>
		/// <param name="themeName">Name of the theme to apply (case-insensitive).</param>
		/// <returns>True if the theme was found and applied; false otherwise.</returns>
		bool ApplyTheme(string themeName);

		/// <summary>Raised after the theme has changed.</summary>
		event EventHandler ThemeChanged;
	}
}