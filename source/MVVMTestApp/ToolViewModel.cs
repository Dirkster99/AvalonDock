namespace AvalonDock.MVVMTestApp
{
	internal class ToolViewModel : PaneViewModel
	{
		private bool _isVisible = true;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="name"></param>
		public ToolViewModel(string name)
		{
			Name = name;
			Title = name;
		}

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
	}
}
