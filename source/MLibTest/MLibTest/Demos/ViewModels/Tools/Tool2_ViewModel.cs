namespace AvalonDock.Tools
{
    using MLibTest.Demos.ViewModels.Interfaces;
    using System;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using System.Windows.Input;
    using MLibTest.ViewModels.Base;
    using MLib.Interfaces;
    using Settings.Interfaces;
    using AvalonDock.MVVMTestApp;

    /// <summary>
    /// Implements the viewmodel that drives a sample tool window view.
    /// </summary>
    internal class Tool2_ViewModel : ToolViewModel
    {
        #region fields
        /// <summary>
        /// Identifies the <see ref="ContentId"/> of this tool window.
        /// </summary>
        public const string ToolContentId = "Tool2_Tool";

        /// <summary>
        /// Identifies the caption string used for this tool window.
        /// </summary>
        public const string ToolTitle = "Tool 2";

        private IWorkSpaceViewModel _workSpaceViewModel = null;

        private Color _SelectedBackgroundColor;
        private Color _SelectedAccentColor;
        private ICommand _ResetAccentColorCommand;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="workSpaceViewModel">Is the link to the application's viewmodel
        /// to enable (event based) communication between this viewmodel and the application.</param>
        public Tool2_ViewModel(IWorkSpaceViewModel workSpaceViewModel)
            : base(ToolTitle)
        {
            _workSpaceViewModel = workSpaceViewModel;

            SetupADToolDefaults();
            SetupToolDefaults();
        }

        /// <summary>
        /// Hidden default class constructor
        /// </summary>
        protected Tool2_ViewModel()
          : base(ToolTitle)
        {
            SetupADToolDefaults();
            SetupToolDefaults();
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets/sets the currently selected accent color for the color picker in the tool window's view.
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get { return _SelectedBackgroundColor; }
            set
            {
                if (_SelectedBackgroundColor != value)
                {
                    _SelectedBackgroundColor = value;
                    RaisePropertyChanged(() => SelectedBackgroundColor);
                }
            }
        }

        /// <summary>
        /// Gets/sets the currently selected accent color for the color picker in the tool window's view.
        /// </summary>
        public Color SelectedAccentColor
        {
            get { return _SelectedAccentColor; }
            set
            {
                if (_SelectedAccentColor != value)
                {
                    _SelectedAccentColor = value;
                    RaisePropertyChanged(() => SelectedAccentColor);
                }
            }
        }

        /// <summary>
        /// Gets a command to reset the currently selected accent color
        /// and reloads all current resources to make sure that the
        /// accent is changed consistently.
        /// </summary>
        public ICommand ResetAccentColorCommand
        {
            get
            {
                if (_ResetAccentColorCommand == null)
                {
                    _ResetAccentColorCommand = new RelayCommand<object>((p) =>
                    {
                        if ((p is Color) == false)
                            return;

                        Color accentColor = (Color)p;

                        var appearance = GetService<IAppearanceManager>();
                        var settings = GetService<ISettingsManager>(); // add the default themes

                        // 1) You could use this if you where using MLib only
                        // appearance.SetAccentColor(accentColor);

                        // 2) But you should use this if you use MLib with additional libraries
                        //    with additional accent colors to be synchronized at run-time
                        appearance.SetTheme(settings.Themes
                                            , appearance.ThemeName
                                            , accentColor);

                        // 3 You could also use something like this to change accent color
                        // If you were using your own Theming Framework or MUI, Mahapps etc
                        //
////                        Application.Current.Resources[MWindowLib.Themes.ResourceKeys.ControlAccentColorKey] = accentColor;
////                        Application.Current.Resources[MWindowLib.Themes.ResourceKeys.ControlAccentBrushKey] = new SolidColorBrush(accentColor);
////
////                        Application.Current.Resources[MLib.Themes.ResourceKeys.ControlAccentColorKey] = accentColor;
////                        Application.Current.Resources[MLib.Themes.ResourceKeys.ControlAccentBrushKey] = new SolidColorBrush(accentColor);
////
////                        Application.Current.Resources[Xceed.Wpf.AvalonDock.Themes.VS2013.Themes.ResourceKeys.ControlAccentColorKey] = accentColor;
////                        Application.Current.Resources[Xceed.Wpf.AvalonDock.Themes.VS2013.Themes.ResourceKeys.ControlAccentBrushKey] = new SolidColorBrush(accentColor);
////
////                        Application.Current.Resources[NumericUpDownLib.Themes.ResourceKeys.ControlAccentColorKey] = accentColor;
////                        Application.Current.Resources[NumericUpDownLib.Themes.ResourceKeys.ControlAccentBrushKey] = new SolidColorBrush(accentColor);

                    });
                }

                return _ResetAccentColorCommand;
            }
        }

        /// <summary>
        /// Gets a human readable description for the <see ref="SelectedAccentColor"/> property.
        /// </summary>
        public string SelectedAccentColorDescription
        {
            get
            {
                return "Define a custom color.";
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Initialize Avalondock specific defaults that are specific to this tool window.
        /// </summary>
        private void SetupADToolDefaults()
        {
            ContentId = ToolContentId;           // Define a unique contentid for this toolwindow

            BitmapImage bi = new BitmapImage();  // Define an icon for this toolwindow
            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,/Demos/Images/property-blue.png");
            bi.EndInit();
            IconSource = bi;
        }

        /// <summary>
        /// Initialize non-Avalondock defaults that are specific to this tool window.
        /// </summary>
        private void SetupToolDefaults()
        {
            SelectedBackgroundColor = Color.FromArgb(255, 0, 0, 0);
            SelectedAccentColor = Color.FromArgb(128, 0, 180, 0);
        }
        #endregion methods
    }
}
