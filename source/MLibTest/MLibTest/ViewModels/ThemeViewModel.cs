namespace MLibTest.ViewModels
{
    using MLib.Interfaces;
    using MLib.Themes;
    using Settings.Interfaces;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// ViewModel class that manages theme properties for binding and display in WPF UI.
    /// </summary>
    internal class ThemeViewModel : Base.ViewModelBase
    {
        #region private fields
        private ThemeDefinitionViewModel _DefaultTheme = null;
        private Dictionary<string, ThemeDefinitionViewModel> _ListOfThemes = null;
        private ThemeDefinitionViewModel _SelectedTheme = null;
        private bool _IsEnabled = true;
        #endregion private fields

        #region constructors
        /// <summary>
        /// Standard Constructor
        /// </summary>
        public ThemeViewModel()
        {

            _ListOfThemes = new Dictionary<string, ThemeDefinitionViewModel>();
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Returns a default theme that should be applied when nothing else is available.
        /// </summary>
        public ThemeDefinitionViewModel DefaultTheme
        {
            get
            {
                return _DefaultTheme;
            }
        }

        /// <summary>
        /// Returns a list of theme definitons.
        /// </summary>
        public List<ThemeDefinitionViewModel> ListOfThemes
        {
            get
            {
                return _ListOfThemes.Select(it => it.Value).ToList();
            }
        }

        /// <summary>
        /// Gets the currently selected theme (or desfault on applaiction start-up)
        /// </summary>
        public ThemeDefinitionViewModel SelectedTheme
        {
            get
            {
                return _SelectedTheme;
            }

            private set
            {
                if (_SelectedTheme != value)
                {
                    if (_SelectedTheme != null)
                        _SelectedTheme.IsSelected = false;

                    _SelectedTheme = value;

                    if (_SelectedTheme != null)
                        _SelectedTheme.IsSelected = true;

                    this.RaisePropertyChanged(() => this.SelectedTheme);
                }
            }
        }

        /// <summary>
        /// Gets whether a different theme can be selected right now or not.
        /// This property should be bound to the UI that selects a different
        /// theme to avoid the case in which a user could select a theme and
        /// select a different theme while the first theme change request is
        /// still processed.
        /// </summary>
        public bool IsEnabled
        {
            get { return _IsEnabled; }

            private set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged(() => IsEnabled);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Applies a new theme based on the changed selection in the input element.
        /// </summary>
        /// <param name="ts"></param>
        public void ApplyTheme(FrameworkElement fe, string themeName)
        {
            if (themeName != null)
            {
                IsEnabled = false;
                try
                {
                    var settings = GetService<ISettingsManager>(); // add the default themes

                    Color AccentColor = ThemeViewModel.GetCurrentAccentColor(settings);
                    GetService<IAppearanceManager>().SetTheme(settings.Themes, themeName, AccentColor);

                    ThemeDefinitionViewModel o;
                    _ListOfThemes.TryGetValue(themeName, out o);
                    SelectedTheme = o;
                }
                catch
                {
                }
                finally
                {
                    IsEnabled = true;
                }
            }
        }

        public static Color GetCurrentAccentColor(ISettingsManager settings)
        {
            Color AccentColor = default(Color);

            if (settings.Options.GetOptionValue<bool>("Appearance", "ApplyWindowsDefaultAccent"))
            {
                try
                {
                    AccentColor = SystemParameters.WindowGlassColor;
                }
                catch
                {
                }

                // This may be black on Windows 7 and the experience is black & white then :-(
                if (AccentColor == default(Color) || AccentColor == Colors.Black || AccentColor.A == 0)
                {
                    // default blue accent color
                    AccentColor = Color.FromRgb(0x1b, 0xa1, 0xe2);
                }
            }
            else
                AccentColor = settings.Options.GetOptionValue<Color>("Appearance", "AccentColor");

            return AccentColor;
        }

        internal void InitThemes(ISettingsManager settings)
        {
            var themes = settings.Themes.GetThemeInfos();

            if (themes != null)
            {
                foreach (var item in themes)
                {
                    _ListOfThemes.Add(item.DisplayName, new ThemeDefinitionViewModel(item));
                }

                // Lets make sure there is a default
                var defaultTheme = GetService<IAppearanceManager>().GetDefaultTheme().DisplayName;
                _ListOfThemes.TryGetValue(defaultTheme, out _DefaultTheme);

                // and something sensible is selected
                _SelectedTheme = _DefaultTheme;
                _SelectedTheme.IsSelected = true;
            }
        }
        #endregion methods
    }
}
