using AvalonDock.Themes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AvalonDock.MVVMTestApp
{
	internal class Workspace : ViewModelBase
	{
		#region fields

		private static Workspace _this = new Workspace();

		private ToolViewModel[] _tools = null;
		private ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
		private ReadOnlyObservableCollection<FileViewModel> _readonyFiles = null;
		private FileViewModel _activeDocument = null;
		private FileStatsViewModel _fileStats = null;
		private RelayCommand _openCommand = null;
		private RelayCommand _newCommand = null;
		private Tuple<string, Theme> selectedTheme;

		#endregion fields

		#region constructors

		/// <summary>
		/// Class constructor
		/// </summary>
		protected Workspace()
		{
			this.Themes = new List<Tuple<string, Theme>>
			{
				new Tuple<string, Theme>(nameof(GenericTheme), new GenericTheme()),
				new Tuple<string, Theme>(nameof(AeroTheme),new AeroTheme()),
				new Tuple<string, Theme>(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
				new Tuple<string, Theme>(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
				new Tuple<string, Theme>(nameof(MetroTheme),new MetroTheme()),
				new Tuple<string, Theme>(nameof(VS2010Theme),new VS2010Theme()),
				new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
				new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
				new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
			};
			this.SelectedTheme = Themes.First();
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
					_tools = new ToolViewModel[] { FileStats };
				return _tools;
			}
		}

		public FileStatsViewModel FileStats
		{
			get
			{
				if (_fileStats == null)
					_fileStats = new FileStatsViewModel();

				return _fileStats;
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

		public List<Tuple<string, Theme>> Themes { get; set; }

		public Tuple<string, Theme> SelectedTheme
		{
			get { return selectedTheme; }
			set
			{
				selectedTheme = value;
				RaisePropertyChanged(nameof(SelectedTheme));
			}
		}

		#endregion properties

		#region methods

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
			if (fileToSave.FilePath == null || saveAsFlag)
			{
				var dlg = new SaveFileDialog();
				if (dlg.ShowDialog().GetValueOrDefault())
					fileToSave.FilePath = dlg.SafeFileName;
			}
			if (fileToSave.FilePath == null)
			{
				return;
			}
			File.WriteAllText(fileToSave.FilePath, fileToSave.TextContent);
			ActiveDocument.IsDirty = false;
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