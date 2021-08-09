using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace AvalonDock.VS2013Test.ViewModels
{
	internal class ErrorViewModel : ToolViewModel
	{
		#region fields
		public const string ToolContentId = "FileStatsTool";
		private DateTime _lastModified;
		private long _fileSize;
		private string _FileName;
		private string _FilePath;
		#endregion fields

		#region constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		public ErrorViewModel()
			: base("Error List")
		{
			Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
			ContentId = ToolContentId;
		}
		#endregion constructors

		#region Properties

		public long FileSize
		{
			get => _fileSize;
			protected set
			{
				if (_fileSize != value)
				{
					_fileSize = value;
					RaisePropertyChanged(nameof(FileSize));
				}
			}
		}

		public DateTime LastModified
		{
			get => _lastModified;
			protected set
			{
				if (_lastModified != value)
				{
					_lastModified = value;
					RaisePropertyChanged(nameof(LastModified));
				}
			}
		}

		public string FileName
		{
			get => _FileName;
			protected set
			{
				if (_FileName != value)
				{
					_FileName = value;
					RaisePropertyChanged(nameof(FileName));
				}
			}
		}

		public string FilePath
		{
			get => _FilePath;
			protected set
			{
				if (_FilePath != value)
				{
					_FilePath = value;
					RaisePropertyChanged(nameof(FilePath));
				}
			}
		}

		#endregion Properties

		#region methods
		private void OnActiveDocumentChanged(object sender, EventArgs e)
		{
			if (Workspace.This.ActiveDocument != null &&
				Workspace.This.ActiveDocument.FilePath != null &&
				File.Exists(Workspace.This.ActiveDocument.FilePath))
			{
				var fi = new FileInfo(Workspace.This.ActiveDocument.FilePath);
				FileSize = fi.Length;
				LastModified = fi.LastWriteTime;
				FileName = fi.Name;
				FilePath = fi.Directory.FullName;
			}
			else
			{
				FileSize = 0;
				LastModified = DateTime.MinValue;
				FileName = string.Empty;
				FilePath = string.Empty;
			}
		}
		#endregion methods

	}
}
