namespace MLibTest.ViewModels
{
	using MLib.Interfaces;

	internal class ThemeDefinitionViewModel : Base.ViewModelBase
	{
		readonly private IThemeInfo _model;

		private bool _IsSelected;

		public ThemeDefinitionViewModel(IThemeInfo model)
			: this()
		{
			_model = model;
		}

		protected ThemeDefinitionViewModel()
		{
			_model = null;
			_IsSelected = false;
		}

		/// <summary>
		/// Gets the static theme model based data items.
		/// </summary>
		public IThemeInfo Model
		{
			get
			{
				return _model;
			}
		}

		/// <summary>
		/// Determines whether this theme is currently selected or not.
		/// </summary>
		public bool IsSelected
		{
			get => _IsSelected;
			set
			{
				if (_IsSelected != value)
				{
					_IsSelected = value;
				}
			}
		}
	}
}
