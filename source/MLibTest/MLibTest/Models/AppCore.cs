namespace MLibTest.Models
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Class supplies a set of common static helper methodes that help
    /// localizing application specific items such as setting folders etc.
    /// </summary>
    public class AppCore
    {
        #region properties
        /// <summary>
        /// Get the name of the executing assembly (usually name of *.exe file)
        /// </summary>
        internal static string AssemblyTitle
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
        }

        //
        // Summary:
        //     Gets the path or UNC location of the loaded file that contains the manifest.
        //
        // Returns:
        //     The location of the loaded file that contains the manifest. If the loaded
        //     file was shadow-copied, the location is that of the file after being shadow-copied.
        //     If the assembly is loaded from a byte array, such as when using the System.Reflection.Assembly.Load(System.Byte[])
        //     method overload, the value returned is an empty string ("").
        internal static string AssemblyEntryLocation
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }

        /// <summary>
        /// Get a path to the directory where the user store his documents
        /// </summary>
        public static string MyDocumentsUserDir
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        public static string Company
        {
            get
            {
                return "MLibTest";
            }
        }
        public static string Application_Title
        {
            get
            {
                return "MLibTest";
            }
        }

        /// <summary>
        /// Get a path to the directory where the application
        /// can persist/load user data on session exit and re-start.
        /// </summary>
        public static string DirAppData
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                 System.IO.Path.DirectorySeparatorChar +
                                                 AppCore.Company;
            }
        }

////        /// <summary>
////        /// Get path and file name to application specific settings file
////        /// </summary>
////        public static string DirFileAppSettingsData
////        {
////            get
////            {
////                return System.IO.Path.Combine(AppCore.DirAppData,
////                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.settings", AppCore.AssemblyTitle));
////            }
////        }

        /// <summary>
        /// Get path and file name to application specific session file
        /// </summary>
        public static string DirFileAppSessionData
        {
            get
            {
                return System.IO.Path.Combine(AppCore.DirAppData,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.session", AppCore.AssemblyTitle));
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Create a dedicated directory to store program settings and session data
        /// </summary>
        /// <returns></returns>
        public static bool CreateAppDataFolder()
        {
            try
            {
                if (System.IO.Directory.Exists(AppCore.DirAppData) == false)
                    System.IO.Directory.CreateDirectory(AppCore.DirAppData);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
                return false;
            }

            return true;
        }
        #endregion methods
    }
}
