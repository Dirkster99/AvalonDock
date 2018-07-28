namespace Settings.Internal
{
    using Settings.Interfaces;
    using Settings.ProgramSettings;
    using MLib.Interfaces;
    using Settings.UserProfile;
    using SettingsModel.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// This class keeps track of program options and user profile (session) data.
    /// Both data items can be added and are loaded on application start to restore
    /// the program state of the last user session or to implement the default
    /// application state when starting the application for the very first time.
    /// </summary>
    internal class SettingsManagerImpl : ISettingsManager
    {
        #region fields
        protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOptionsPanel mSettingsDataPanel = null;

        private IProfile mSessionData = null;
        #endregion fields

        #region constructor
        /// <summary>
        /// Class cosntructor
        /// </summary>
        public SettingsManagerImpl(IThemeInfos themeinfos)
            : this()
        {
            Themes = themeinfos;
        }

        /// <summary>
        /// Hidden default constructor.
        /// </summary>
        protected SettingsManagerImpl()
        {
            mSettingsDataPanel = new OptionsPanel();
            SessionData = new Profile();
        }
        #endregion constructor

        #region properties
        /// <summary>
        /// Implement <seealso cref="IOptionsPanel"/> method to query options from model container.
        /// </summary>
        public IEngine Options
        {
            get
            {
                return mSettingsDataPanel.Options;
            }
        }

        public IProfile SessionData
        {
            get
            {
                if (mSessionData == null)
                    mSessionData = new Profile();

                return mSessionData;
            }

            private set
            {
                if (mSessionData != value)
                    mSessionData = value;
            }
        }

        #region min max definitions for useful option values
        /// <summary>
        /// Gets the minimum font size that should be used in this application.
        /// </summary>
        [XmlIgnore]
        public int IconSizeMin
        {
            get
            {
                return 16;
            }
        }

        /// <summary>
        /// Gets the maximum icon size that should be used in this application.
        /// </summary>
        [XmlIgnore]
        public int IconSizeMax
        {
            get
            {
                return 16*8;
            }
        }

        /// <summary>
        /// Gets the minimum font size that should be used in this application.
        /// </summary>
        [XmlIgnore]
        public int FontSizeMin
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Gets the maximum font size that should be used in this application.
        /// </summary>
        [XmlIgnore]
        public int FontSizeMax
        {
            get
            {
                return 32;
            }
        }

        /// <summary>
        /// Gets the default icon size for the application.
        /// </summary>
        [XmlIgnore]
        public int DefaultIconSize
        {
            get
            {
                return 32;
            }
        }

        /// <summary>
        /// Gets the default font size for the application.
        /// </summary>
        [XmlIgnore]
        public int DefaultFontSize
        {
            get
            {
                return 14;
            }
        }

        /// <summary>
        /// Gets the default fixed font size for the application.
        /// </summary>
        [XmlIgnore]
        public int DefaultFixedFontSize
        {
            get
            {
                return 12;
            }
        }
        #endregion min max definitions for useful option values

        /// <summary>
        /// Gets the internal name and Uri source for all available themes.
        /// </summary>
        [XmlIgnore]
        public IThemeInfos Themes { get; private set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Get a list of all supported languages in Edi.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LanguageCollection> GetSupportedLanguages()
        {
            List<LanguageCollection> ret = new List<LanguageCollection>();

            ret.Add(new LanguageCollection() { Language = "en", Locale = "US", Name = "English (English)" });
            ret.Add(new LanguageCollection() { Language = "de", Locale = "DE", Name = "Deutsch (German)" });

            return ret;
        }

        /// <summary>
        /// Determine whether program options are valid and corrext
        /// settings if they appear to be invalid on current system
        /// </summary>
        public void CheckSettingsOnLoad(double SystemParameters_VirtualScreenLeft,
                                        double SystemParameters_VirtualScreenTop)
        {
            //// Dirkster: Not sure whether this is working correctly yet...
            //// this.SessionData.CheckSettingsOnLoad(SystemParameters_VirtualScreenLeft,
            ////                                      SystemParameters_VirtualScreenTop);
        }

        #region Load Save UserSessionData
        /// <summary>
        /// Save program options into persistence.
        /// See <seealso cref="SaveOptions"/> to save program options on program end.
        /// </summary>
        /// <param name="sessionDataFileName"></param>
        /// <returns></returns>
        public void LoadSessionData(string sessionDataFileName)
        {
            Profile profileDataModel = null;

            try
            {
                if (System.IO.File.Exists(sessionDataFileName))
                {
                    FileStream readFileStream = null;
                    try
                    {
                        // Create a new file stream for reading the XML file
                        readFileStream = new System.IO.FileStream(sessionDataFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                        // Create a new XmlSerializer instance with the type of the test class
                        XmlSerializer serializerObj = new XmlSerializer(typeof(Profile));

                        // Load the object saved above by using the Deserialize function
                        profileDataModel = (Profile)serializerObj.Deserialize(readFileStream);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                    finally
                    {
                        // Cleanup
                        if (readFileStream != null)
                            readFileStream.Close();
                    }
                }

                SessionData = profileDataModel;
            }
            catch (Exception exp)
            {
                logger.Error(exp);
            }
            finally
            {
                if (profileDataModel == null)
                    profileDataModel = new Profile();  // Just get the defaults if serilization wasn't working here...
            }
        }

        /// <summary>
        /// Save program options into persistence.
        /// See <seealso cref="LoadOptions"/> to load program options on program start.
        /// </summary>
        /// <param name="sessionDataFileName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SaveSessionData(string sessionDataFileName, IProfile model)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.NewLineOnAttributes = true;
            xws.Indent = true;
            xws.IndentChars = "  ";
            xws.Encoding = System.Text.Encoding.UTF8;

            // Create a new file stream to write the serialized object to a file
            XmlWriter xw = null;
            try
            {
                xw = XmlWriter.Create(sessionDataFileName, xws);

                // Create a new XmlSerializer instance with the type of the test class
                XmlSerializer serializerObj = new XmlSerializer(typeof(Profile));

                serializerObj.Serialize(xw, model);

                return true;
            }
            finally
            {
                if (xw != null)
                    xw.Close(); // Cleanup

            }
        }
        #endregion Load Save UserSessionData
        #endregion methods
    }
}
