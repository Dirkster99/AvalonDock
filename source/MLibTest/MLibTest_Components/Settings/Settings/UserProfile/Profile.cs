namespace Settings.UserProfile
{
    using Settings.Interfaces;
    using SettingsModel.Models;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// This class implements the model of the user profile part
    /// of the application. Typically, users have implicit run-time
    /// settings that should be re-activated when the application
    /// is re-started at a later point in time (e.g. window size
    /// and position).
    /// 
    /// This class organizes these per user specific profile settings
    /// and is responsible for their storage (at program end) and
    /// retrieval at the start-up of the application.
    /// </summary>
    public class Profile : IProfile
    {
        #region fields
        protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion fields

        #region constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public Profile()
        {
            // Session Data
            WindowPosSz = new SerializableDictionary<string, ViewPosSizeModel>();
            WindowPosSz.Add(MainWindowName, new ViewPosSizeModel(ViewPosSizeModel.DefaultSize));

            LastActiveSolution = LastActiveTargetFile = string.Empty;

            LastActiveSourceFiles = new List<FileReference>();
        }
        #endregion constructor

        #region properties
        /// <summary>
        /// Gets the key name of the MainWindow item in the collection.
        /// Ths name can be used as key in the WindowPosSz property
        /// to read and write MainWindow position and size information.
        /// </summary>
        [XmlIgnore]
        public string MainWindowName
        {
            get { return "MainWindow"; }
        }

        /// <summary>
        /// Get/set position and size of MainWindow and other windows in the collection.
        /// </summary>
        [XmlElement(ElementName = "WindowPosSz")]
        public SerializableDictionary<string, ViewPosSizeModel> WindowPosSz { get; set; }

        /// <summary>
        /// Remember the last active solution file name and path of last session.
        /// 
        /// This can be useful when selecting active document in next session or
        /// determining a useful default path when there is no document currently open.
        /// </summary>
        [XmlAttribute(AttributeName = "LastActiveSolution")]
        public string LastActiveSolution { get; set; }

        /// <summary>
        /// Remember the last active path and name of last active document.
        /// 
        /// This can be useful when selecting active document in next session or
        /// determining a useful default path when there is no document currently open.
        /// </summary>
        [XmlArrayItem("LastActiveSourceFiles", IsNullable = true)]
        public List<FileReference> LastActiveSourceFiles { get; set; }

        /// <summary>
        /// Remember the last active path and name of last active document.
        /// 
        /// This can be useful when selecting active document in next session or
        /// determining a useful default path when there is no document currently open.
        /// </summary>
        [XmlAttribute(AttributeName = "LastActiveTargetFile")]
        public string LastActiveTargetFile { get; set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Checks the MainWindow for visibility when re-starting application
        /// (with different screen configuration).
        /// </summary>
        /// <param name="SystemParameters_VirtualScreenLeft"></param>
        /// <param name="SystemParameters_VirtualScreenTop"></param>
        public void CheckSettingsOnLoad(double SystemParameters_VirtualScreenLeft,
                                        double SystemParameters_VirtualScreenTop)
        {
            var defaultWindow = new ViewPosSizeModel(ViewPosSizeModel.DefaultSize);

            if (WindowPosSz == null)
            {
                WindowPosSz = new SerializableDictionary<string, ViewPosSizeModel>();
                WindowPosSz.Add(MainWindowName, defaultWindow);
            }
            else
            {
                ViewPosSizeModel win;
                if (WindowPosSz.TryGetValue(MainWindowName, out win) == true)
                {
                    if (win.DefaultConstruct == true)
                    {
                        WindowPosSz.Remove(MainWindowName);
                        WindowPosSz.Add(MainWindowName, defaultWindow);
                    }
                }
            }

            // Ensure window visibility on different screens and sizes...
            defaultWindow.SetValidPos(SystemParameters_VirtualScreenLeft,
                                      SystemParameters_VirtualScreenTop);
        }

        /// <summary>
        /// Updates or inserts the requested window pos size item in the collection.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public void UpdateInsertWindowPosSize(string windowName, ViewPosSizeModel model)
        {
            ViewPosSizeModel checkModel;
            if (WindowPosSz.TryGetValue(windowName, out checkModel) == true)
                WindowPosSz.Remove(windowName);

            WindowPosSz.Add(windowName, model);
        }

        /// <summary>
        /// Get the path of the file or empty string if file does not exists on disk.
        /// </summary>
        /// <returns></returns>
        public string GetLastActivePath()
        {
            try
            {
                if (System.IO.File.Exists(LastActiveSolution))
                    return System.IO.Path.GetDirectoryName(LastActiveSolution);
            }
            catch
            {
            }

            return string.Empty;
        }
        #endregion methods
    }
}
