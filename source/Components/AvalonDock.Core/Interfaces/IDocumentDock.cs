namespace AvalonDock.Core
{
	/// <summary>
	/// Represents a document container dock.
	/// </summary>
	public interface IDocumentDock : IDock
	{
		bool CanCreateDocument { get; set; }
	}

	/// <summary>
	/// Represents a tool/anchorable container dock.
	/// </summary>
	public interface IToolDock : IDock
	{
		DockAlignment Alignment { get; set; }

		bool IsExpanded { get; set; }

		bool AutoHide { get; set; }
	}
}