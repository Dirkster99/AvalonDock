namespace MLibTest
{
    using MLibTest.Models;
    using MLibTest.ViewModels;
    using MLibTest.ViewModels.Base;
    using Settings.UserProfile;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Xceed.Wpf.AvalonDock.Layout.Serialization;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MWindowLib.MetroWindow
                                    , IViewSize  // Implements saving and loading/repositioning of Window
    {
        #region fields
        ICommand _loadLayoutCommand = null;
        ICommand _saveLayoutCommand = null;
        #endregion fields

        #region ctors
        /// <summary>
        /// Class constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion ctors

        #region methods
        #region LoadLayoutCommand
        public ICommand LoadLayoutCommand
        {
            get
            {
                if (_loadLayoutCommand == null)
                {
                    _loadLayoutCommand = new RelayCommand<object>(
                        (p) => OnLoadLayoutAsync(p),
                        (p) => CanLoadLayout(p));
                }

                return _loadLayoutCommand;
            }
        }

        private bool CanLoadLayout(object parameter)
        {
            App myApp = (App)Application.Current;

            return myApp.LayoutLoaded.CanLoadLayout();
        }

        internal void OnLayoutLoaded_Event(object sender, LayoutLoadedEventArgs layoutLoadedEvent)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Process the finally block since we have nothing to do here
                    if (layoutLoadedEvent == null)
                        return;

                    var result = layoutLoadedEvent.Result;
                    if (result.LoadwasSuccesful == true)
                    {
                        var stringLayoutSerializer = new XmlLayoutSerializer(dockManager);
                        //Here I've implemented the LayoutSerializationCallback just to show
                        // a way to feed layout desarialization with content loaded at runtime
                        //Actually I could in this case let AvalonDock to attach the contents
                        //from current layout using the content ids
                        //LayoutSerializationCallback should anyway be handled to attach contents
                        //not currently loaded

                        stringLayoutSerializer.LayoutSerializationCallback += (s, e) =>
                        {
                            try
                            {
                                var workSpace = (DataContext as AppViewModel).AD_WorkSpace;

                                if (workSpace == null || string.IsNullOrEmpty(e.Model.ContentId))
                                {
                                    e.Cancel = true;
                                    return;
                                }

                                // Is this a tool window layout ? Then, get its viewmodel and connect it to the view
                                var tool = workSpace.Tools.FirstOrDefault(i => i.ContentId == e.Model.ContentId);
                                if (tool != null)
                                {
                                    e.Content = tool;
                                    return;
                                }

                                // Its not a tool window -> So, this could rever to a document then
                                if (!string.IsNullOrWhiteSpace(e.Model.ContentId)  && File.Exists(e.Model.ContentId))
                                {
                                    e.Content = workSpace.Open(e.Model.ContentId);
                                    return;
                                }

                                // Not something we could recognize -> So, we won't handle it beyond this point
                                e.Cancel = true;
                            }
                            catch (System.Exception exc)
                            {
                                Debug.WriteLine(exc.StackTrace);
                            }
                        };

                        using (var reader = new StringReader(result.XmlContent))   // Read Xml Data from string
                        {
                            stringLayoutSerializer.Deserialize(reader);
                        }
                    }
                }
                catch (System.Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                finally
                {
                    // Make sure AvalonDock control is visible at the end of restoring layout
                    dockManager.Visibility = Visibility.Visible;
                }
            },
            System.Windows.Threading.DispatcherPriority.Background);
        }

        internal async void OnLoadLayoutAsync(object parameter = null)
        {
            App myApp = (App)Application.Current;

            LayoutLoaderResult LoaderResult = await myApp.LayoutLoaded.GetLayoutString(OnLayoutLoaded_Event);

            // Call this even with null to ensure standard initialization takes place
            this.OnLayoutLoaded_Event(null, (LoaderResult == null ? null: new LayoutLoadedEventArgs(LoaderResult)));
        }

        #endregion 

        #region SaveLayoutCommand
        public ICommand SaveLayoutCommand
        {
            get
            {
                if (_saveLayoutCommand == null)
                {
                    _saveLayoutCommand = new RelayCommand<object>((p) => OnSaveLayout(p), (p) => CanSaveLayout(p));
                }

                return _saveLayoutCommand;
            }
        }

        private bool CanSaveLayout(object parameter)
        {
            return true;
        }

        internal void OnSaveLayout(object parameter = null)
        {
            var layoutSerializer = new XmlLayoutSerializer(dockManager);
            layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
        }

        #endregion 

        private void OnDumpToConsole(object sender, RoutedEventArgs e)
        {
            // Check Define TRACE constant
            // in build Tab of MLibTest project and AvalonDock project
            // to generate trace in output window
#if TRACE
    dockManager.Layout.ConsoleDump(0);
#endif
        }
        #endregion methods
    }
}
