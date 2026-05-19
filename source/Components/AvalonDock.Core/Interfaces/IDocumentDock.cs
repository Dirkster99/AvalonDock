namespace AvalonDock.Core
{
	/// <summary>
	/// Represents a document container dock.
	/// </summary>
	public interface IDocumentDock : IDock
	{
		/// <summary>Gets or sets a value indicating whether new documents can be created in this dock.</summary>
		bool CanCreateDocument { get; set; }
	}

	/// <summary>
	/// Represents a tool/anchorable container dock.
	/// </summary>
	public interface IToolDock : IDock
	{
		/// <summary>Gets or sets the alignment/side where this tool dock is placed.</summary>
		DockAlignment Alignment { get; set; }

		/// <summary>Gets or sets a value indicating whether this tool dock is expanded.</summary>
		bool IsExpanded { get; set; }

		/// <summary>Gets or sets a value indicating whether this tool dock uses auto-hide behavior.</summary>
		bool AutoHide { get; set; }
	}
}