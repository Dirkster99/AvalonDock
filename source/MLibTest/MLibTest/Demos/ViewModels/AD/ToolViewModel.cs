namespace AvalonDock.MVVMTestApp
{
    internal class ToolViewModel : PaneViewModel
    {
        #region Fields
        private bool _isVisible = true;
        #endregion Fields

        #region constructors
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
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the name of this tool window.
        /// </summary>
        public string Name { get; }

        #region IsVisible
        /// <summary>
        /// Gets/sets whether this tool window is visible or not.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged(() => IsVisible);
                }
            }
        }
        #endregion IsVisible
        #endregion properties
    }
}
