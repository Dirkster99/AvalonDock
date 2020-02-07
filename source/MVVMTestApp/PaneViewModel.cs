namespace AvalonDock.MVVMTestApp
{
	using System.Windows.Media;

	class PaneViewModel : ViewModelBase
	{
		#region fields
		private string _title = null;
		private string _contentId = null;
		private bool _isSelected = false;
		private bool _isActive = false;
		#endregion fields

		#region constructors
		public PaneViewModel()
		{
		}
		#endregion constructors

		#region Properties
		public string Title
		{
			get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					RaisePropertyChanged(nameof(Title));
				}
			}
		}

		public ImageSource IconSource { get; protected set; }

		public string ContentId
		{
			get => _contentId;
			set
			{
				if (_contentId != value)
				{
					_contentId = value;
					RaisePropertyChanged(nameof(ContentId));
				}
			}
		}

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					RaisePropertyChanged(nameof(IsSelected));
				}
			}
		}

		public bool IsActive
		{
			get => _isActive;
			set
			{
				if (_isActive != value)
				{
					_isActive = value;
					RaisePropertyChanged(nameof(IsActive));
				}
			}
		}
		#endregion Properties
	}
}
