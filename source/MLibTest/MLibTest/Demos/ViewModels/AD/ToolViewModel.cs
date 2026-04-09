namespace MLibTest.Demos.ViewModels.AD
{
	internal class ToolViewModel : PaneViewModel
	{
		private bool _isVisible = false;

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="name"></param>
		public ToolViewModel(string name)
		{
			Name = name;
			Title = name;
		}

		/// <summary>
		/// Hidden default class constructor
		/// </summary>
		protected ToolViewModel()
		{
		}

		/// <summary>
		/// Gets the name of this tool window.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets/sets whether this tool window is visible or not.
		/// </summary>
		public bool IsVisible
		{
			get => _isVisible;
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					RaisePropertyChanged(() => IsVisible);
				}
			}
		}
	}
}
