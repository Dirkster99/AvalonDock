using System;
using System.Collections.Generic;
using System.Linq;
using AvalonDock.Core;
using AvalonDock.Themes;

namespace AvalonDock
{
	/// <summary>
	/// WPF implementation of <see cref="IThemeManager"/>.
	/// Manages theme discovery, selection, and application to a <see cref="DockingManager"/>.
	/// </summary>
	/// <remarks>
	/// Register themes via <see cref="RegisterTheme"/> or pass them in the constructor.
	/// Apply a theme by name via <see cref="ApplyTheme"/>; the manager sets
	/// <see cref="DockingManager.Theme"/> internally.
	/// </remarks>
	public class ThemeManager : IThemeManager
	{
		private readonly List<Theme> _themes = new List<Theme>();
		private DockingManager _dockingManager;
		private string _currentThemeName;

		/// <summary>
		/// Initializes a new <see cref="ThemeManager"/>.
		/// </summary>
		/// <param name="themes">Optional initial set of themes to register.</param>
		public ThemeManager(IEnumerable<Theme> themes = null)
		{
			if (themes != null)
			{
				foreach (var theme in themes)
					_themes.Add(theme);
			}
		}

		/// <inheritdoc />
		public string CurrentThemeName => _currentThemeName;

		/// <inheritdoc />
#pragma warning disable CS3003 // Type not CLS-compliant
		public IReadOnlyList<IThemeInfo> AvailableThemes => _themes.Cast<IThemeInfo>().ToList().AsReadOnly();
#pragma warning restore CS3003

		/// <inheritdoc />
		public event EventHandler ThemeChanged;

		/// <summary>
		/// Binds this manager to a <see cref="DockingManager"/> instance.
		/// Must be called before <see cref="ApplyTheme"/>.
		/// </summary>
		public void Attach(DockingManager dockingManager)
		{
			_dockingManager = dockingManager ?? throw new ArgumentNullException(nameof(dockingManager));
		}

		/// <summary>Registers an additional theme.</summary>
		public void RegisterTheme(Theme theme)
		{
			if (theme == null) throw new ArgumentNullException(nameof(theme));
			if (!_themes.Any(t => t.GetType() == theme.GetType()))
				_themes.Add(theme);
		}

		/// <inheritdoc />
		public bool ApplyTheme(string themeName)
		{
			if (string.IsNullOrWhiteSpace(themeName)) return false;

			var theme = _themes.FirstOrDefault(t =>
				string.Equals(t.Name, themeName, StringComparison.OrdinalIgnoreCase));
			if (theme == null) return false;

			if (_dockingManager != null)
				_dockingManager.Theme = theme;

			_currentThemeName = theme.Name;
			ThemeChanged?.Invoke(this, EventArgs.Empty);
			return true;
		}
	}
}