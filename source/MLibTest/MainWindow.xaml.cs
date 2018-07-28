namespace MLibTest
{
    using MLibTest.ViewModels.Base;
    using Settings.UserProfile;
    using System.Diagnostics;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        #region LoadLayoutCommand
        public ICommand LoadLayoutCommand
        {
            get
            {
                if (_loadLayoutCommand == null)
                {
                    _loadLayoutCommand = new RelayCommand<object>((p) => OnLoadLayout(p), (p) => CanLoadLayout(p));
                }

                return _loadLayoutCommand;
            }
        }

        private bool CanLoadLayout(object parameter)
        {
            return System.IO.File.Exists(@".\AvalonDock.Layout.config");
        }

        internal void OnLoadLayout(object parameter = null)
        {
            try
            {
                if (System.IO.File.Exists(@".\AvalonDock.Layout.config") == false)
                    return;

                var layoutSerializer = new XmlLayoutSerializer(dockManager);
                //Here I've implemented the LayoutSerializationCallback just to show
                // a way to feed layout desarialization with content loaded at runtime
                //Actually I could in this case let AvalonDock to attach the contents
                //from current layout using the content ids
                //LayoutSerializationCallback should anyway be handled to attach contents
                //not currently loaded
                layoutSerializer.LayoutSerializationCallback += (s, e) =>
                {
                    //if (e.Model.ContentId == FileStatsViewModel.ToolContentId)
                    //    e.Content = Workspace.This.FileStats;
                    //else if (!string.IsNullOrWhiteSpace(e.Model.ContentId) &&
                    //    File.Exists(e.Model.ContentId))
                    //    e.Content = Workspace.This.Open(e.Model.ContentId);
                };
                layoutSerializer.Deserialize(@".\AvalonDock.Layout.config");
            }
            catch (System.Exception exception)
            {
                Debug.WriteLine(exception);
            }
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
            // Uncomment when TRACE is activated on AvalonDock project
            //dockManager.Layout.ConsoleDump(0);
        }
    }
}
