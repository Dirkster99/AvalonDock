using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AvalonDock.MVVMTestApp
{
	using AvalonDock.Layout.Serialization;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = Workspace.This;

			this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
			this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
			serializer.LayoutSerializationCallback += (s, args) =>
			{
				args.Content = args.Content;
			};

			if (File.Exists(@".\AvalonDock.config"))
				serializer.Deserialize(@".\AvalonDock.config");
		}

		private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
		{
			var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
			serializer.Serialize(@".\AvalonDock.config");
		}

		#region LoadLayoutCommand

		private RelayCommand _loadLayoutCommand = null;

		public ICommand LoadLayoutCommand
		{
			get
			{
				if (_loadLayoutCommand == null)
				{
					_loadLayoutCommand = new RelayCommand((p) => OnLoadLayout(), (p) => CanLoadLayout());
				}

				return _loadLayoutCommand;
			}
		}

		private bool CanLoadLayout()
		{
			return File.Exists(@".\AvalonDock.Layout.config");
		}

		private void OnLoadLayout()
		{
			var layoutSerializer = new XmlLayoutSerializer(dockManager);

			// Here I've implemented the LayoutSerializationCallback just to show
			//  a way to feed layout desarialization with content loaded at runtime
			// Actually I could in this case let AvalonDock to attach the contents
			// from current layout using the content ids
			// LayoutSerializationCallback should anyway be handled to attach contents
			// not currently loaded
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

		#endregion LoadLayoutCommand

		#region SaveLayoutCommand

		private RelayCommand _saveLayoutCommand = null;

		public ICommand SaveLayoutCommand
		{
			get
			{
				if (_saveLayoutCommand == null)
				{
					_saveLayoutCommand = new RelayCommand((p) => OnSaveLayout(), (p) => CanSaveLayout());
				}

				return _saveLayoutCommand;
			}
		}

		private bool CanSaveLayout()
		{
			return true;
		}

		private void OnSaveLayout()
		{
			var layoutSerializer = new XmlLayoutSerializer(dockManager);
			layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
		}

		#endregion SaveLayoutCommand

		private void OnDumpToConsole(object sender, RoutedEventArgs e)
		{
			// Uncomment when TRACE is activated on AvalonDock project
			//dockManager.Layout.ConsoleDump(0);
		}
	}
}