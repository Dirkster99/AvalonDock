namespace MLibTest.Demos.ViewModels.Interfaces
{
	using MLibTest.Demos.ViewModels.AD;
	using System;
	using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the interface to the <see cref="WorkSpaceViewModel"/> which implements
    /// AvalonDock demo specific properties, events and methods.
    /// </summary>
    internal interface IWorkSpaceViewModel
	{
		/// <summary>
		/// Event is raised when AvalonDock (or the user) selects a new document.
		/// </summary>
		event EventHandler ActiveDocumentChanged;

		/// <summary>
		/// Gets/sets the currently active document.
		/// </summary>
		DocumentViewModel ActiveDocument { get; set; }

		/// <summary>
		/// Gets an enumeration of all currently available tool window viewmodels.
		/// </summary>
		IEnumerable<ToolViewModel> Tools { get; }

		#region methods
		/// <summary>
		/// Checks if a document can be closed and asks the user whether
		/// to save before closing if the document appears to be dirty.
		/// </summary>
		/// <param name="fileToClose"></param>
		void Close(DocumentViewModel fileToClose);

		/// <summary>
		/// Saves a document and resets the dirty flag.
		/// </summary>
		/// <param name="fileToSave"></param>
		/// <param name="saveAsFlag"></param>
		void Save(DocumentViewModel fileToSave, bool saveAsFlag = false);

		/// <summary>
		/// Open a file and return its content in a viewmodel.
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		Task<DocumentViewModel> OpenAsync(string filepath);
		#endregion methods
	}
}
