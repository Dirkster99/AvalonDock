namespace MLibTest.Demos.ViewModels.Interfaces
{
    using AvalonDock.MVVMTestApp;
    using System;
    using System.Collections.Generic;

    internal interface IWorkSpaceViewModel
    {
        event EventHandler ActiveDocumentChanged;

        FileViewModel ActiveDocument { get; set; }

        IEnumerable<ToolViewModel> Tools { get; }

        #region methods
        void Close(FileViewModel fileToClose);

        void Save(FileViewModel fileToSave, bool saveAsFlag = false);
        #endregion methods
    }
}
