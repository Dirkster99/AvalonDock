namespace AvalonDock.MVVMTestApp
{
    using MLibTest.Demos.ViewModels.Interfaces;
    using System;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;

    /// <summary>
    /// Implements the viewmodel that drives the view a Color Picker tool window.
    ///
    /// https://github.com/Dirkster99/ColorPickerLib
    /// </summary>
    internal class ColorPickerViewModel : ToolViewModel
    {
        #region fields
        /// <summary>
        /// Identifies the <see ref="ContentId"/> of this tool window.
        /// </summary>
        public const string ToolContentId = "ColorPickerTool";

        /// <summary>
        /// Identifies the caption string used for this tool window.
        /// </summary>
        public const string ToolTitle = "Color Picker";

        private IWorkSpaceViewModel _workSpaceViewModel = null;

        private Color _SelectedAccentColor;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="workSpaceViewModel">Is the link to the application's viewmodel
        /// to enable (event based) communication between this viewmodel and the application.</param>
        public ColorPickerViewModel(IWorkSpaceViewModel workSpaceViewModel)
            : base(ToolTitle)
        {
            _workSpaceViewModel = workSpaceViewModel;

            SetupADToolDefaults();
            SetupToolDefaults();
        }

        /// <summary>
        /// Hidden default class constructor
        /// </summary>
        protected ColorPickerViewModel()
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
            SelectedAccentColor = Color.FromRgb(180, 0, 0);
        }
        #endregion methods
    }
}
