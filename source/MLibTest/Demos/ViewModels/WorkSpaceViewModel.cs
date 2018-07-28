namespace MLibTest.Demos.ViewModels
{
    using AvalonDock.MVVMTestApp;
    using Microsoft.Win32;
    using MLibTest.Demos.ViewModels.Interfaces;
    using MLibTest.ViewModels.Base;
    using MWindowInterfacesLib.MsgBox.Enums;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    #region Helper Test Classes
    /// <summary>
    /// This class is uses to create a type safe list
    /// of enumeration members for selection in combobox.
    /// </summary>
    public class MessageImageCollection
    {
        public string Name { get; set; }
        public MsgBoxImage EnumKey { get; set; }
    }

    /// <summary>
    /// Test class to enumerate over message box buttons enumeration.
    /// </summary>
    public class MessageButtonCollection
    {
        public string Name { get; set; }
        public MsgBoxButtons EnumKey { get; set; }
    }

    /// <summary>
    /// Test class to enumerate over message box result enumeration.
    /// The <seealso cref="MsgBoxResult"/> enumeration is used to define
    /// a default button (if any).
    /// </summary>
    public class MessageResultCollection
    {
        public string Name { get; set; }
        public MsgBoxResult EnumKey { get; set; }
    }

    /// <summary>
    /// Test class to enumerate over languages (and their locale) that
    /// are supported with specific (non-English) button and tool tip strings.
    /// 
    /// The class definition is based on BCP 47 which in turn is used to
    /// set the UI and thread culture (which in turn selects the correct string resource in MsgBox assembly).
    /// </summary>
    public class LanguageCollection
    {
        public string Language { get; set; }
        public string Locale { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Get BCP47 language tag for this language
        /// See also http://en.wikipedia.org/wiki/IETF_language_tag
        /// </summary>
        public string BCP47
        {
            get
            {
                if (string.IsNullOrEmpty(this.Locale) == false)
                    return String.Format("{0}-{1}", this.Language, this.Locale);
                else
                    return String.Format("{0}", this.Language);
            }
        }

        /// <summary>
        /// Get BCP47 language tag for this language
        /// See also http://en.wikipedia.org/wiki/IETF_language_tag
        /// </summary>
        public string DisplayName
        {
            get
            {
                return String.Format("{0} {1}", this.Name, this.BCP47);
            }
        }
    }
    #endregion Helper Test Classes

    internal class WorkSpaceViewModel : MLibTest.ViewModels.Base.ViewModelBase, IWorkSpaceViewModel
    {
        #region private fields
        ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
        ReadOnlyObservableCollection<FileViewModel> _readonyFiles = null;
        ToolViewModel[] _tools = null;
        ICommand _openCommand = null;
        ICommand _newCommand = null;

        FileStatsViewModel _fileStats = null;
        private FileViewModel _activeDocument = null;
        #endregion private fields

        #region constructors
        public WorkSpaceViewModel()
        {
        }
        #endregion constructors

        public event EventHandler ActiveDocumentChanged;

        #region Properties
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
                    _fileStats = new FileStatsViewModel(this as IWorkSpaceViewModel);

                return _fileStats;
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                {
                    _openCommand = new RelayCommand<object>((p) => OnOpen(p), (p) => CanOpen(p));
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
                    _newCommand = new RelayCommand<object>((p) => OnNew(p), (p) => CanNew(p));
                }

                return _newCommand;
            }
        }
        #endregion Properties

        #region methods

        public void Close(FileViewModel fileToClose)
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

        public void Save(FileViewModel fileToSave, bool saveAsFlag = false)
        {
            if (fileToSave.FilePath == null || saveAsFlag)
            {
                var dlg = new SaveFileDialog();
                if (dlg.ShowDialog().GetValueOrDefault())
                    fileToSave.FilePath = dlg.SafeFileName;
            }

            System.IO.File.WriteAllText(fileToSave.FilePath, fileToSave.TextContent);
            ActiveDocument.IsDirty = false;
        }

        #region OpenCommand
        private bool CanOpen(object parameter)
        {
            return true;
        }

        private void OnOpen(object parameter)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                var fileViewModel = Open(dlg.FileName);
                ActiveDocument = fileViewModel;
            }
        }

        public FileViewModel Open(string filepath)
        {
            var fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);
            if (fileViewModel != null)
                return fileViewModel;

            fileViewModel = new FileViewModel(filepath, this as IWorkSpaceViewModel);
            _files.Add(fileViewModel);
            return fileViewModel;
        }
        #endregion  OpenCommand

        #region NewCommand
        private bool CanNew(object parameter)
        {
            return true;
        }

        private void OnNew(object parameter)
        {
            _files.Add(new FileViewModel(this as IWorkSpaceViewModel));
            ActiveDocument = _files.Last();
        }

        #endregion 

        #region ActiveDocument
        public FileViewModel ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                if (_activeDocument != value)
                {
                    _activeDocument = value;
                    RaisePropertyChanged("ActiveDocument");
                    if (ActiveDocumentChanged != null)
                        ActiveDocumentChanged(this, EventArgs.Empty);
                }
            }
        }
        #endregion
        #endregion methods
    }
}
