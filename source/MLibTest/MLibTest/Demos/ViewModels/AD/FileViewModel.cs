namespace AvalonDock.MVVMTestApp
{
    using MLibTest.Demos.ViewModels.Interfaces;
    using MLibTest.ViewModels.Base;
    using System.IO;
    using System.Windows.Input;
    using System.Windows.Media;

    internal class FileViewModel : PaneViewModel
    {
        #region fields
        private static ImageSourceConverter ISC = new ImageSourceConverter();
        private IWorkSpaceViewModel _workSpaceViewModel = null;

        private string _textContent = string.Empty;
        private string _filePath = null;
        private bool _isDirty = false;

        ICommand _closeCommand = null;
        ICommand _saveAsCommand = null;
        ICommand _saveCommand = null;
        #endregion fields

        #region ctors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="workSpaceViewModel"></param>
        public FileViewModel(string filePath, IWorkSpaceViewModel workSpaceViewModel)
            : this(workSpaceViewModel)
        {
            FilePath = filePath;
            Title = FileName;

            //Set the icon only for open documents (just a test)
            IconSource = ISC.ConvertFromInvariantString(@"pack://application:,,/Demos/Images/document.png") as ImageSource;
        }

        /// <summary>
        /// class constructor
        /// </summary>
        /// <param name="workSpaceViewModel"></param>
        public FileViewModel(IWorkSpaceViewModel workSpaceViewModel)
        {
            _workSpaceViewModel = workSpaceViewModel;
            IsDirty = false;
            Title = FileName;
        }
        #endregion ctors

        #region Properties
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChanged("FilePath");
                    RaisePropertyChanged("FileName");
                    RaisePropertyChanged("Title");

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

        #region TextContent
        public string TextContent
        {
            get { return _textContent; }
            set
            {
                if (_textContent != value)
                {
                    _textContent = value;
                    RaisePropertyChanged("TextContent");
                    IsDirty = true;
                }
            }
        }

        #endregion

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged("IsDirty");
                    RaisePropertyChanged("FileName");
                }
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand<object>((p) => OnSave(p), (p) => CanSave(p));
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
                    _saveAsCommand = new RelayCommand<object>((p) => OnSaveAs(p), (p) => CanSaveAs(p));
                }

                return _saveAsCommand;
            }
        }

        #region CloseCommand
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand<object>((p) => OnClose(), (p) => CanClose());
                }

                return _closeCommand;
            }
        }
        #endregion
        #endregion Properties

        #region methods
        private bool CanClose()
        {
            return true;
        }

        private void OnClose()
        {
            _workSpaceViewModel.Close(this);
        }

        private bool CanSave(object parameter)
        {
            return IsDirty;
        }

        private void OnSave(object parameter)
        {
            _workSpaceViewModel.Save(this, false);
        }

        private bool CanSaveAs(object parameter)
        {
            return IsDirty;
        }

        private void OnSaveAs(object parameter)
        {
            _workSpaceViewModel.Save(this, true);
        }
        #endregion methods
    }
}
