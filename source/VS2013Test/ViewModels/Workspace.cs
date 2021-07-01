using AvalonDock.Themes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AvalonDock.VS2013Test.ViewModels
{
	internal class Workspace : ViewModelBase
	{
		#region fields

		private static Workspace _this = new Workspace();
		private ToolViewModel[] _tools;
		private ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
		private ReadOnlyObservableCollection<FileViewModel> _readonyFiles;
		private FileViewModel _activeDocument;
		private ErrorViewModel _errors;
		private PropertiesViewModel _props;
		private ExplorerViewModel _explorer;
		private OutputViewModel _output;
		private GitChangesViewModel _git;
		private ToolboxViewModel _toolbox;
		private RelayCommand _openCommand;
		private RelayCommand _newCommand;
		private Tuple<string, Theme> _selectedTheme;

		#endregion fields

		#region constructors

		/// <summary>
		/// Class constructor
		/// </summary>
		public Workspace()
		{
			SelectedTheme = Themes.First();
		}

		#endregion constructors

		public event EventHandler ActiveDocumentChanged;

		#region properties

		public static Workspace This => _this;

		public ReadOnlyObservableCollection<FileViewModel> Files
		{
			get
			{
				if (_readonyFiles == null)
					_readonyFiles = new ReadOnlyObservableCollection<FileViewModel>(_files);

				return _readonyFiles;
			}
		}

		public IEnumerable<ToolViewModel> Tools
		{
			get
			{
				if (_tools == null)
					_tools = new ToolViewModel[] { Explorer, Props, Errors, Output, Git, Toolbox };
				return _tools;
			}
		}

		public ExplorerViewModel Explorer
		{
			get
			{
				if (_explorer == null)
					_explorer = new ExplorerViewModel();

				return _explorer;
			}
		}

		public PropertiesViewModel Props
		{
			get
			{
				if (_props == null)
					_props = new PropertiesViewModel();

				return _props;
			}
		}

		public ErrorViewModel Errors
		{
			get
			{
				if (_errors == null)
					_errors = new ErrorViewModel();

				return _errors;
			}
		}

		public OutputViewModel Output
		{
			get
			{
				if (_output == null)
					_output = new OutputViewModel();

				return _output;
			}
		}

		public GitChangesViewModel Git
		{
			get
			{
				if (_git == null)
					_git = new GitChangesViewModel();

				return _git;
			}
		}

		public ToolboxViewModel Toolbox
		{
			get
			{
				if (_toolbox == null)
					_toolbox = new ToolboxViewModel();

				return _toolbox;
			}
		}

		public ICommand OpenCommand
		{
			get
			{
				if (_openCommand == null)
				{
					_openCommand = new RelayCommand((p) => OnOpen(p), (p) => CanOpen(p));
				}

				return _openCommand;
			}
		}

		public ICommand NewCommand
		{
			get
			{
				if (_newCommand == null)
				{
					_newCommand = new RelayCommand((p) => OnNew(p), (p) => CanNew(p));
				}

				return _newCommand;
			}
		}

		public FileViewModel ActiveDocument
		{
			get => _activeDocument;
			set
			{
				if (_activeDocument != value)
				{
					_activeDocument = value;
					RaisePropertyChanged(nameof(ActiveDocument));
					if (ActiveDocumentChanged != null)
						ActiveDocumentChanged(this, EventArgs.Empty);
				}
			}
		}

		public List<Tuple<string, Theme>> Themes { get; set; } = new List<Tuple<string, Theme>>
		{
			new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
			new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
			new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme())
		};

		public Tuple<string, Theme> SelectedTheme
		{
			get { return _selectedTheme; }
			set
			{
				_selectedTheme = value;
				SwitchExtendedTheme();
				RaisePropertyChanged(nameof(SelectedTheme));
			}
		}

		#endregion properties

		#region methods

		private void SwitchExtendedTheme()
		{
			switch (_selectedTheme.Item1)
			{
				case "Vs2013DarkTheme":
					Application.Current.Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/MLib;component/Themes/DarkTheme.xaml");
					Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/VS2013Test;component/Themes/DarkBrushsExtended.xaml");
					break;
				case "Vs2013LightTheme":
					Application.Current.Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/MLib;component/Themes/LightTheme.xaml");
					Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/VS2013Test;component/Themes/LightBrushsExtended.xaml");
					break;
				case "Vs2013BlueTheme":
					//TODO: Create new color resources for blue theme
					Application.Current.Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/MLib;component/Themes/LightTheme.xaml");
					Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/VS2013Test;component/Themes/BlueBrushsExtended.xaml");
					break;
				default:
					break;
			}
		}

		internal void Close(FileViewModel fileToClose)
		{
			if (fileToClose.IsDirty)
			{
				var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "AvalonDock Test App", MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
					return;
				if (res == MessageBoxResult.Yes)
				{
					Save(fileToClose);
				}
			}

			_files.Remove(fileToClose);
		}

		internal void Save(FileViewModel fileToSave, bool saveAsFlag = false)
		{
			string newTitle = string.Empty;

			if (fileToSave.FilePath == null || saveAsFlag)
			{
				var dlg = new SaveFileDialog();
				if (dlg.ShowDialog().GetValueOrDefault())
				{
					fileToSave.FilePath = dlg.FileName;
					newTitle = dlg.SafeFileName;
				}
			}
			if (fileToSave.FilePath == null)
			{
				return;
			}
			File.WriteAllText(fileToSave.FilePath, fileToSave.TextContent);
			ActiveDocument.IsDirty = false;

			if (string.IsNullOrEmpty(newTitle)) return;
			ActiveDocument.Title = newTitle;
		}

		internal FileViewModel Open(string filepath)
		{
			var fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);
			if (fileViewModel != null)
				return fileViewModel;

			fileViewModel = new FileViewModel(filepath);
			_files.Add(fileViewModel);
			return fileViewModel;
		}

		#region OpenCommand

		private bool CanOpen(object parameter) => true;

		private void OnOpen(object parameter)
		{
			var dlg = new OpenFileDialog();
			if (dlg.ShowDialog().GetValueOrDefault())
			{
				var fileViewModel = Open(dlg.FileName);
				ActiveDocument = fileViewModel;
			}
		}

		#endregion OpenCommand

		#region NewCommand

		private bool CanNew(object parameter)
		{
			return true;
		}

		private void OnNew(object parameter)
		{
			_files.Add(new FileViewModel());
			ActiveDocument = _files.Last();
		}

		#endregion NewCommand

		#endregion methods
	}
}