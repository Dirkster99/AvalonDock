namespace MLibTest.ViewModels
{
    using Base;
    using Demos.ViewModels;
    using MLib.Interfaces;
    using MLibTest.Demos.ViewModels.Interfaces;
    using Settings.Interfaces;
    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Main ViewModel vlass that manages session start-up, life span, and shutdown
    /// of the application.
    /// </summary>
    internal class AppViewModel : Base.ViewModelBase, IDisposable
    {
        #region private fields
        private bool _isInitialized = false;       // application should be initialized through one method ONLY!
        private object _lockObject = new object(); // thread lock semaphore

        private ICommand _ThemeSelectionChangedCommand = null;

        private AppLifeCycleViewModel _AppLifeCycle = null;
        private ThemeViewModel _AppTheme = null;
        private readonly IWorkSpaceViewModel _AD_WorkSpace = null;
        private bool _Disposed = false;
        #endregion private fields

        #region constructors
        /// <summary>
        /// Standard Constructor
        /// </summary>
        public AppViewModel(AppLifeCycleViewModel lifecycle)
            : this()
        {
            _AppLifeCycle = lifecycle;
        }

        /// <summary>
        /// Hidden standard constructor
        /// </summary>
        protected AppViewModel()
        {
            _AD_WorkSpace = new WorkSpaceViewModel();
            _AppTheme = new ThemeViewModel();
        }
        #endregion constructors

        #region properties
        public AppLifeCycleViewModel AppLifeCycle
        {
            get
            {
                return _AppLifeCycle;
            }

            internal set
            {
                _AppLifeCycle = value;
            }
        }

        #region app theme
        /// <summary>
        /// Command executes when the user has selected
        /// a different UI theme to display.
        /// 
        /// Command Parameter is the <seealso cref="ThemeDefinitionViewModel"/> object
        /// that should be selected next. This object can be handed over as:
        /// 1> an object[] array at object[0] or as simple object
        /// 2> <seealso cref="ThemeDefinitionViewModel"/> p
        /// </summary>
        public ICommand ThemeSelectionChangedCommand
        {
            get
            {
                if (_ThemeSelectionChangedCommand == null)
                {
                    _ThemeSelectionChangedCommand = new RelayCommand<object>((p) =>
                    {
                        if (this._Disposed == true)
                            return;

                        object[] paramets = p as object[];

                        ThemeDefinitionViewModel theme = null;

                        // Try to convert object[0] command parameter
                        if(paramets != null)
                        {
                            if (paramets.Length == 1)
                            {
                                theme = paramets[0] as ThemeDefinitionViewModel;
                            }
                        }

                        // Try to convert ThemeDefinitionViewModel command parameter
                        if (theme == null)
                            theme = p as ThemeDefinitionViewModel;

                        if (Application.Current == null)
                            return;

                        if (Application.Current.MainWindow == null)
                            return;

                        if (theme != null)
                        {
                            _AppTheme.ApplyTheme(Application.Current.MainWindow,
                                                 theme.Model.DisplayName);
                        }
                    });
                }

                return _ThemeSelectionChangedCommand;
            }
        }

        /// <summary>
        /// Gets the currently selected application theme object.
        /// </summary>
        public ThemeViewModel AppTheme
        {
            get { return _AppTheme; }

            private set
            {
                if (_AppTheme != value)
                {
                    _AppTheme = value;
                    RaisePropertyChanged(() => this.AppTheme);
                }
            }
        }
        #endregion app theme

        /// <summary>
        /// Gets the demo viewmodel and all its properties and commands
        /// </summary>
        public IWorkSpaceViewModel AD_WorkSpace
        {
            get
            {
                return _AD_WorkSpace;
            }            
        }
        #endregion properties

        #region methods
        #region Get/set Session Application Data
        internal void GetSessionData(IProfile sessionData)
        {
/***
            if (sessionData.LastActiveTargetFile != TargetFile.FileName)
                sessionData.LastActiveTargetFile = TargetFile.FileName;

            sessionData.LastActiveSourceFiles = new List<SettingsModel.Models.FileReference>();
            if (SourceFiles != null)
            {
                foreach (var item in SourceFiles)
                    sessionData.LastActiveSourceFiles.Add(new SettingsModel.Models.FileReference()
                    { path = item.FileName }
                                                         );
            }
***/
        }

        internal void SetSessionData(IProfile sessionData)
        {
/***
            TargetFile.FileName = sessionData.LastActiveTargetFile;

            _SourceFiles = new ObservableCollection<FileInfoViewModel>();
            if (sessionData.LastActiveSourceFiles != null)
            {
                foreach (var item in sessionData.LastActiveSourceFiles)
                    _SourceFiles.Add(new FileInfoViewModel(item.path));
            }
***/
        }
        #endregion Get/set Session Application Data

        /// <summary>
        /// Call this method if you want to initialize a headless
        /// (command line) application. This method will initialize only
        /// Non-WPF related items.
        /// 
        /// Method should not be called after <seealso cref="InitForMainWindow"/>
        /// </summary>
        public void InitWithoutMainWindow()
        {
            lock (_lockObject)
            {
                if (_isInitialized == true)
                    throw new Exception("AppViewModel initizialized twice.");

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Call this to initialize application specific items that should be initialized
        /// before loading and display of mainWindow.
        /// 
        /// Invocation of This method is REQUIRED if UI is used in this application instance.
        /// 
        /// Method should not be called after <seealso cref="InitWithoutMainWindow"/>
        /// </summary>
        public void InitForMainWindow(IAppearanceManager appearance
                                      , string themeDisplayName)
        {
            // Initialize base that does not require UI
            InitWithoutMainWindow();

            appearance.AccentColorChanged += Appearance_AccentColorChanged;

            // Initialize UI specific stuff here
            this.AppTheme.ApplyTheme(Application.Current.MainWindow, themeDisplayName);
        }

        /// <summary>
        /// Standard dispose method of the <seealso cref="IDisposable" /> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Source: http://www.codeproject.com/Articles/15360/Implementing-IDisposable-and-the-Dispose-Pattern-P
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed == false)
            {
                if (disposing == true)
                {
                    // Dispose of the curently displayed content
                    ////mContent.Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }

            _Disposed = true;

            //// If it is available, make the call to the
            //// base class's Dispose(Boolean) method
            ////base.Dispose(disposing);
        }

        /// <summary>
        /// Method is invoked when theme manager is asked
        /// to change the accent color and has actually changed it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Appearance_AccentColorChanged(object sender, MLib.Events.ColorChangedEventArgs e)
        {

        }
        #endregion methods
    }
}
