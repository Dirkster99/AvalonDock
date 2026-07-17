using System;
using System.Collections.Generic;
using System.Linq;
using AvalonDock.Core;
using AvalonDock.Themes;

namespace AvalonDock
{
	/// <summary>
	/// Represents the theme Manager.
	/// </summary>
	public class ThemeManager : IThemeManager
	{
		private readonly List<Theme> _themes = new List<Theme>();
		private DockingManager _dockingManager;
		private string _currentThemeName;

		/// <summary>
		/// Initializes a new instance of the <see cref="ThemeManager"/> class.
		/// </summary>
		/// <param name="themes">The themes.</param>
		public ThemeManager(IEnumerable<Theme> themes = null)
		{
			if (themes != null)
			{
				foreach (var theme in themes)
					_themes.Add(theme);
			}
		}

		/// <summary>
		/// Gets the current Theme Name.
		/// </summary>
		public string CurrentThemeName => _currentThemeName;

#pragma warning disable CS3003 // Type not CLS-compliant

		/// <summary>
		/// Gets the available Themes.
		/// </summary>
		public IReadOnlyList<IThemeInfo> AvailableThemes => _themes.Cast<IThemeInfo>().ToList().AsReadOnly();
#pragma warning restore CS3003

		/// <summary>
		/// Occurs when theme Changed.
		/// </summary>
		public event EventHandler ThemeChanged;

		/// <summary>
		/// Executes the attach operation.
		/// </summary>
		/// <param name="dockingManager">The docking Manager.</param>
		public void Attach(DockingManager dockingManager)
		{
			_dockingManager = dockingManager ?? throw new ArgumentNullException(nameof(dockingManager));
		}

		/// <summary>
		/// Executes the register Theme operation.
		/// </summary>
		/// <param name="theme">The theme.</param>
		public void RegisterTheme(Theme theme)
		{
			if (theme == null) throw new ArgumentNullException(nameof(theme));
			if (!_themes.Any(t => t.GetType() == theme.GetType()))
				_themes.Add(theme);
		}

		/// <summary>
		/// Executes the apply Theme operation.
		/// </summary>
		/// <param name="themeName">The theme Name.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
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