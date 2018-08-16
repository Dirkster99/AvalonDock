namespace Settings.ProgramSettings
{
    using Settings.Interfaces;
    using SettingsModel.Interfaces;
    using SettingsModel.Models;

    internal class OptionsPanel : IOptionsPanel
    {
        private IEngine mQuery = null;

        public OptionsPanel()
        {
            mQuery = Factory.CreateEngine();
        }

        /// <summary>
        /// Gets the options <seealso cref="IEngine"/> that used to manage program options.
        /// </summary>
        public IEngine Options
        {
            get
            {
                return mQuery;
            }

            private set
            {
                mQuery = value;
            }
        }
    }
}
