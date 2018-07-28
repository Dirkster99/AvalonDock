namespace Settings.Interfaces
{
    using Settings.UserProfile;
    using SettingsModel.Models;
    using System;
    using System.Collections.Generic;

    public interface IProfile
    {
        #region properties
        string GetLastActivePath();
        string LastActiveSolution { get; set; }


        string LastActiveTargetFile { get; set; }

        List<FileReference> LastActiveSourceFiles { get; set; }

        /// <summary>
        /// Gets the key name of the MainWindow item in the collection.
        /// Ths name can be used as key in the WindowPosSz property
        /// to read and write MainWindow position and size information.
        /// </summary>
        string MainWindowName { get; }

        /// <summary>
        /// Gets a collection of window position and size items.
        /// </summary>
        SerializableDictionary<string, ViewPosSizeModel> WindowPosSz { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Checks the MainWindow for visibility when re-starting application
        /// (with different screen configuration).
        /// </summary>
        /// <param name="SystemParameters_VirtualScreenLeft"></param>
        /// <param name="SystemParameters_VirtualScreenTop"></param>
        void CheckSettingsOnLoad(double SystemParameters_VirtualScreenLeft, double SystemParameters_VirtualScreenTop);

        /// <summary>
        /// Updates or inserts the requested window pos size item in the collection.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        void UpdateInsertWindowPosSize(string windowName, ViewPosSizeModel model);
        #endregion methods
    }
}
