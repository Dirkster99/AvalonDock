namespace Settings.Interfaces
{
    using Settings.ProgramSettings;
    using System.Collections.Generic;

    public interface IOptions
    {
        #region properties
        bool IsDirty { get; set; }
        string LanguageSelected { get; set; }
        bool ReloadOpenFilesOnAppStart { get; set; }
        string SourceFilePath { get; set; }

        string DefaultSourceLanguage { get; set; }
        string DefaultTargetLanguage { get; set; }

        string DefaultDefaultSourceLanguage { get; }
        string DefaultDefaultTargetLanguage { get; }

        int DefaultIconSize { get; }
        int IconSizeMin { get; }
        int IconSizeMax { get; }

        int DefaultFontSize { get; }
        int FontSizeMin { get; }
        int FontSizeMax { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Reset the dirty flag (e.g. after saving program options when they where edit).
        /// </summary>
        /// <param name="flag"></param>
        void SetDirtyFlag(bool flag);

        void SetIconSize(int size);
        void SetFontSize(int size);
        #endregion methods
    }
}
