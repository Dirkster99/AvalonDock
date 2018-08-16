namespace MLibTest.ViewModels
{
    using MLib.Themes;

    internal class ThemeDefinitionViewModel : Base.ViewModelBase
    {
        #region private fields
        readonly private ThemeDefinition _model;

        private bool _IsSelected;
        #endregion private fields

        #region constructors
        public ThemeDefinitionViewModel(ThemeDefinition model)
            : this()
        {
            _model = model;
        }

        protected ThemeDefinitionViewModel()
        {
            _model = null;
            _IsSelected = false;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the static theme model based data items.
        /// </summary>
        public ThemeDefinition Model
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
            get { return _IsSelected; }

            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                }
            }
        }
        #endregion properties
    }
}
