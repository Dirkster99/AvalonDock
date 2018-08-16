namespace MLibTest
{
    using MLib.Interfaces;
    using MLibTest.Models;
    using MLibTest.ViewModels;
    using MWindowInterfacesLib.Interfaces;
    using Settings.Interfaces;
    using Settings.UserProfile;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region fields
        private ViewModels.AppViewModel _appVM = null;
        private MainWindow _mainWindow = null;
        #endregion fields

        #region constructors
        static App()
        {
            // Create service model to ensure available services
            ServiceInjector.InjectServices();
        }
        #endregion constructors

        #region methods
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Set shutdown mode here (and reset further below) to enable showing custom dialogs (messageboxes)
                // durring start-up without shutting down application when the custom dialogs (messagebox) closes
                ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
            }
            catch
            {
            }

            var settings = GetService<ISettingsManager>(); // add the default themes
            var appearance = GetService<IAppearanceManager>();
            AppLifeCycleViewModel lifeCycle = null;

            try
            {
                lifeCycle = new AppLifeCycleViewModel();
                lifeCycle.LoadConfigOnAppStartup(settings, appearance);

                appearance.SetTheme(settings.Themes
                                    , settings.Options.GetOptionValue<string>("Appearance", "ThemeDisplayName")
                                    , ThemeViewModel.GetCurrentAccentColor(settings));

                // Construct Application ViewMOdel and mainWindow
                _appVM = new ViewModels.AppViewModel(lifeCycle);
                _appVM.SetSessionData(settings.SessionData);

                // Customize services specific items for this application
                // Program message box service for Modern UI (Metro Light and Dark)
                //                var msgBox = GetService<IMessageBoxService>();
                //                msgBox.Style = MsgBoxStyle.WPFThemed;

            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
            }

            try
            {
                var selectedLanguage = settings.Options.GetOptionValue<string>("Options", "LanguageSelected");

                Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
            }

            // Create the optional appearance viewmodel and apply
            // current settings to start-up with correct colors etc...
            ////var appearSettings = new AppearanceViewModel(settings.Themes);
            ////appearSettings.ApplyOptionsFromModel(settings.Options);

            // Initialize WPF theming and friends ...
            var defaultTheme = settings.Options.GetOptionValue<string>("Appearance", "ThemeDisplayName");
            _appVM.InitForMainWindow(appearance, defaultTheme);

            Application.Current.MainWindow = _mainWindow = new MainWindow();
            MainWindow.DataContext = _appVM;

            AppCore.CreateAppDataFolder();

            if (MainWindow != null && _appVM != null)
            {
                ConstructMainWindowSession(_appVM, _mainWindow);

                // and show it to the user ...
                MainWindow.Loaded += MainWindow_Loaded;
                MainWindow.Closing += OnClosing;

                // When the ViewModel asks to be closed, close the window.
                // Source: http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
                MainWindow.Closed += delegate
                {
                    // Save session data and close application
                    OnClosed(_appVM, _mainWindow);

                    var dispose = _appVM as IDisposable;
                    if (dispose != null)
                        dispose.Dispose();

                    _mainWindow.DataContext = null;
                    _appVM = null;
                    _mainWindow = null;
                };

                MainWindow.Show();
            }
        }

        /// <summary>
        /// Method is invoked when the mainwindow is loaded and visble to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose;
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
            }

            _mainWindow.OnLoadLayout();

        /***
            try
            {
                Application.Current.MainWindow = mMainWin = new MainWindow();
                ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;
                AppCore.CreateAppDataFolder();
                if (mMainWin != null && app != null)
                {
                    mMainWin.Closing += OnClosing;
                    ConstructMainWindowSession(app, mMainWin);
                    mMainWin.Show();
                }
            }
            catch (Exception exp)
            {
                Logger.Error(exp);
            }
        ***/
        }

        /// <summary>
        /// COnstruct MainWindow an attach datacontext to it.
        /// </summary>
        /// <param name="workSpace"></param>
        /// <param name="win"></param>
        private void ConstructMainWindowSession(AppViewModel workSpace, IViewSize win)
        {
            try
            {
                var settings = GetService<ISettingsManager>();

                // Establish command binding to accept user input via commanding framework
                // workSpace.InitCommandBinding(win);

                ViewPosSizeModel viewSz;
                settings.SessionData.WindowPosSz.TryGetValue(settings.SessionData.MainWindowName
                                                           , out viewSz);

                viewSz.SetWindowsState(win);

                string lastActiveFile = settings.SessionData.LastActiveSolution;

                MainWindow mainWin = win as MainWindow;
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
            }
        }

        /// <summary>
        /// Save session data on closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                AppViewModel wsVM = base.MainWindow.DataContext as AppViewModel;

                if (wsVM != null)
                {
                    var MainWindowCanClose = MainWindow as IMetroWindow;

                    if (MainWindowCanClose != null)
                    {
                        if (MainWindowCanClose.IsContentDialogVisible == true)
                        {
                            e.Cancel = true;     // Lets not close with open dialog
                            return;
                        }
                    }

                    // Close all open files and check whether application is ready to close
                    if (wsVM.AppLifeCycle.Exit_CheckConditions(wsVM) == true)
                    {
                        // (other than exception and error handling)
                        wsVM.AppLifeCycle.OnRequestClose(true);

                        _mainWindow.OnSaveLayout();
                        e.Cancel = false;
                    }
                    else
                    {
                        wsVM.AppLifeCycle.CancelShutDown();
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
            }
        }

        /// <summary>
        /// Execute closing function and persist session data to be reloaded on next restart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosed(AppViewModel appVM, IViewSize win)
        {
            try
            {
                var settings = GetService<ISettingsManager>();

                ViewPosSizeModel viewSz;
                settings.SessionData.WindowPosSz.TryGetValue(settings.SessionData.MainWindowName
                                                           , out viewSz);
                viewSz.GetWindowsState(win);

                _appVM.GetSessionData(settings.SessionData);

                // Save/initialize program options that determine global programm behaviour
                appVM.AppLifeCycle.SaveConfigOnAppClosed(win);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
            }
        }

        /// <summary>
        /// This method gets the service locator instance
        /// that is  used in turn to get an application specific service instance.
        /// </summary>
        /// <typeparam name="TServiceContract"></typeparam>
        /// <returns></returns>
        private TServiceContract GetService<TServiceContract>() where TServiceContract : class
        {
            return ServiceLocator.ServiceContainer.Instance.GetService<TServiceContract>();
        }
        #endregion methods
    }
}
