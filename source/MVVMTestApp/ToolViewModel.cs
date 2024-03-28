namespace AvalonDock.MVVMTestApp
{
	internal class ToolViewModel : PaneViewModel
	{
		#region fields

		private bool _isVisible = true;
		private bool _canResize = true;

		#endregion fields

		#region constructor

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="name"></param>
		public ToolViewModel(string name)
		{
			Name = name;
			Title = name;
		}

		#endregion constructor

		#region Properties

		public string Name { get; private set; }

		public bool IsVisible
		{
			get => _isVisible;
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					RaisePropertyChanged(nameof(IsVisible));
				}
			}
		}

		public bool CanResize
		{
			get => _canResize;
			set
			{
				if (_canResize != value)
				{
					_canResize = value;
					RaisePropertyChanged(nameof(CanResize));
				}
			}
		}

		#endregion Properties
	}
}