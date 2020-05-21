namespace AvalonDock.MVVMTestApp
{
	using System.IO;
	using System.Windows.Input;
	using System.Windows.Media;

	class FileViewModel : PaneViewModel
	{
		#region fields
		private static ImageSourceConverter ISC = new ImageSourceConverter();
		private string _filePath = null;
		private string _textContent = string.Empty;
		private bool _isDirty = false;
		private RelayCommand _saveCommand = null;
		private RelayCommand _saveAsCommand = null;
		private RelayCommand _closeCommand = null;
		#endregion fields

		#region constructors
		/// <summary>
		/// Class constructor from file path.
		/// </summary>
		/// <param name="filePath"></param>
		public FileViewModel(string filePath)
		{
			FilePath = filePath;
			Title = FileName;

			//Set the icon only for open documents (just a test)
			IconSource = ISC.ConvertFromInvariantString(@"pack://application:,,/Images/document.png") as ImageSource;
		}

		/// <summary>
		/// Default class constructor
		/// </summary>
		public FileViewModel()
		{
			IsDirty = true;
			Title = FileName;
		}
		#endregion constructors

		#region Properties
		public string FilePath
		{
			get => _filePath;
			set
			{
				if (_filePath != value)
				{
					_filePath = value;
					RaisePropertyChanged(nameof(FilePath));
					RaisePropertyChanged(nameof(FileName));
					RaisePropertyChanged(nameof(Title));

					if (File.Exists(_filePath))
					{
						_textContent = File.ReadAllText(_filePath);
						ContentId = _filePath;
					}
				}
			}
		}

		public string FileName
		{
			get
			{
				if (FilePath == null)
					return "Noname" + (IsDirty ? "*" : "");

				return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
			}
		}

		public string TextContent
		{
			get => _textContent;
			set
			{
				if (_textContent != value)
				{
					_textContent = value;
					RaisePropertyChanged(nameof(TextContent));
					IsDirty = true;
				}
			}
		}

		public bool IsDirty
		{
			get => _isDirty;
			set
			{
				if (_isDirty != value)
				{
					_isDirty = value;
					RaisePropertyChanged(nameof(IsDirty));
					RaisePropertyChanged(nameof(FileName));
				}
			}
		}

		public ICommand SaveCommand
		{
			get
			{
				if (_saveCommand == null)
				{
					_saveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
				}

				return _saveCommand;
			}
		}

		public ICommand SaveAsCommand
		{
			get
			{
				if (_saveAsCommand == null)
				{
					_saveAsCommand = new RelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p));
				}

				return _saveAsCommand;
			}
		}

		public ICommand CloseCommand
		{
			get
			{
				if (_closeCommand == null)
				{
					_closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
				}

				return _closeCommand;
			}
		}
		#endregion  Properties

		#region methods
		private bool CanClose()
		{
			return true;
		}

		private void OnClose()
		{
			Workspace.This.Close(this);
		}

		private bool CanSave(object parameter)
		{
			return IsDirty;
		}

		private void OnSave(object parameter)
		{
			Workspace.This.Save(this, false);
		}

		private bool CanSaveAs(object parameter)
		{
			return IsDirty;
		}

		private void OnSaveAs(object parameter)
		{
			Workspace.This.Save(this, true);
		}
		#endregion methods
	}
}
