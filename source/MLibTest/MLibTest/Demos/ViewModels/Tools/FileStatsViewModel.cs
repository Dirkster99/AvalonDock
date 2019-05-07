namespace AvalonDock.Tools
{
    using AvalonDock.MVVMTestApp;
    using MLibTest.Demos.ViewModels.Interfaces;
    using System;
    using System.IO;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Implements the viewmodel that drives the view of the File Stats tool window.
    /// </summary>
    internal class FileStatsViewModel : ToolViewModel
    {
        #region fields
        /// <summary>
        /// Identifies the <see ref="ContentId"/> of this tool window.
        /// </summary>
        public const string ToolContentId = "FileStatsTool";

        /// <summary>
        /// Identifies the caption string used for this tool window.
        /// </summary>
        public const string ToolTitle = "File Stats";

        private IWorkSpaceViewModel _workSpaceViewModel = null;

        private DateTime _lastModified;
        private long _fileSize;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="workSpaceViewModel">Is the link to the application's viewmodel
        /// to enable (event based) communication between this viewmodel and the application.</param>
        public FileStatsViewModel(IWorkSpaceViewModel workSpaceViewModel)
            : base(ToolTitle)
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

        /// <summary>
        /// Hidden default class constructor
        /// </summary>
        protected FileStatsViewModel()
          : base(ToolTitle)
        {
        }
        #endregion constructors

        #region properties
        #region FileSize
        /// <summary>
        /// Gets the byte size of the on disc file (if any).
        /// </summary>
        public long FileSize
        {
            get { return _fileSize; }

            protected set
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
        /// <summary>
        /// Gets the last modification time of the on disc file (if any).
        /// </summary>
        public DateTime LastModified
        {
            get { return _lastModified; }

            protected set
            {
                if (_lastModified != value)
                {
                    _lastModified = value;
                    RaisePropertyChanged("LastModified");
                }
            }
        }
        #endregion
        #endregion properties

        #region methods
        /// <summary>
        /// Update this tool windows content with file based properties,
        /// if the current document has changed.
        /// </summary>
        protected void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (_workSpaceViewModel.ActiveDocument != null &&
                _workSpaceViewModel.ActiveDocument.FilePath != null &&
                File.Exists(_workSpaceViewModel.ActiveDocument.FilePath))
            {
                // Let the toolwindow's content react to the selected documents properties (if any)
                var fi = new FileInfo(_workSpaceViewModel.ActiveDocument.FilePath);
                FileSize = fi.Length;
                LastModified = fi.LastWriteTime;
            }
            else
            {
                // Just use default properties
                // if there is currently no selected document matching
                // the requirements of this toolwindow.
                FileSize = 0;
                LastModified = DateTime.MinValue;
            }
        }
        #endregion methods
    }
}
