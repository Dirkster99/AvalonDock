namespace Settings.Interfaces
{
    using Settings.ProgramSettings;
    using MLib.Interfaces;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public interface ISettingsManager : IOptionsPanel
    {
        void CheckSettingsOnLoad(double SystemParameters_VirtualScreenLeft, double SystemParameters_VirtualScreenTop);

        ////void LoadOptions(string settingsFileName);
        void LoadSessionData(string sessionDataFileName);

        ////bool SaveOptions(string settingsFileName, Settings.Interfaces.IOptions optionsModel);
        bool SaveSessionData(string sessionDataFileName, Settings.Interfaces.IProfile model);

        /// <summary>
        /// Get a list of all supported languages in Edi.
        /// </summary>
        /// <returns></returns>
        IEnumerable<LanguageCollection> GetSupportedLanguages();

        #region properties
        Settings.Interfaces.IProfile SessionData { get; }

        int IconSizeMin { get; }
        int IconSizeMax { get; }

        int FontSizeMin { get; }
        int FontSizeMax { get; }

        /// <summary>
        /// Gets the default icon size for the application.
        /// </summary>
        int DefaultIconSize { get; }

        /// <summary>
        /// Gets the default font size for the application.
        /// </summary>
        int DefaultFontSize { get; }

        /// <summary>
        /// Gets the default fixed font size for the application.
        /// </summary>
        int DefaultFixedFontSize { get; }

        /// <summary>
        /// Gets the internal name and Uri source for all available themes.
        /// </summary>
        [XmlIgnore]
        IThemeInfos Themes { get; }
        #endregion properties
    }
}
