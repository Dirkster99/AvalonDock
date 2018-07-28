namespace AvalonDock.MVVMTestApp
{
    using MLibTest.Demos.ViewModels.Interfaces;
    using System;
    using System.IO;
    using System.Windows.Media.Imaging;

    internal class FileStatsViewModel : ToolViewModel
    {
        private IWorkSpaceViewModel _workSpaceViewModel = null;

        public FileStatsViewModel(IWorkSpaceViewModel workSpaceViewModel)
            :base("File Stats")
        {
            _workSpaceViewModel = workSpaceViewModel;

            _workSpaceViewModel.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
            ContentId = ToolContentId;

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,/Demos/Images/property-blue.png");
            bi.EndInit();
            IconSource = bi;
        }

        public const string ToolContentId = "FileStatsTool";

        void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (_workSpaceViewModel.ActiveDocument != null &&
                _workSpaceViewModel.ActiveDocument.FilePath != null &&
                File.Exists(_workSpaceViewModel.ActiveDocument.FilePath))
            {
                var fi = new FileInfo(_workSpaceViewModel.ActiveDocument.FilePath);
                FileSize = fi.Length;
                LastModified = fi.LastWriteTime;
            }
            else
            {
                FileSize = 0;
                LastModified = DateTime.MinValue;
            }
        }

        #region FileSize

        private long _fileSize;
        public long FileSize
        {
            get { return _fileSize; }
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    RaisePropertyChanged("FileSize");
                }
            }
        }

        #endregion

        #region LastModified

        private DateTime _lastModified;
        public DateTime LastModified
        {
            get { return _lastModified; }
            set
            {
                if (_lastModified != value)
                {
                    _lastModified = value;
                    RaisePropertyChanged("LastModified");
                }
            }
        }

        #endregion
    }
}
