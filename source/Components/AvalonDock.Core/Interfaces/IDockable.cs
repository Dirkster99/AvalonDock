using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Represents the fundamental unit of dockable content in the layout system.
	/// </summary>
	public interface IDockable
	{
		/// <summary>Gets or sets the unique identifier for this dockable.</summary>
		string Id { get; set; }

		/// <summary>Gets or sets the display title.</summary>
		string Title { get; set; }

		/// <summary>Gets or sets the data context for this dockable.</summary>
		object? Context { get; set; }

		/// <summary>Gets or sets the parent dockable that owns this one.</summary>
		IDockable? Owner { get; set; }

		/// <summary>Gets or sets the factory that created this dockable.</summary>
		IFactory? Factory { get; set; }

		/// <summary>Gets or sets a value indicating whether this dockable can be closed.</summary>
		bool CanClose { get; set; }

		/// <summary>Gets or sets a value indicating whether this dockable can be pinned.</summary>
		bool CanPin { get; set; }

		/// <summary>Gets or sets a value indicating whether this dockable can be floated.</summary>
		bool CanFloat { get; set; }

		/// <summary>Gets or sets a value indicating whether this dockable can be dragged.</summary>
		bool CanDrag { get; set; }

		/// <summary>Gets or sets a value indicating whether other dockables can be dropped onto this one.</summary>
		bool CanDrop { get; set; }

		/// <summary>Gets or sets a value indicating whether the content has been modified.</summary>
		bool IsModified { get; set; }

		/// <summary>Gets or sets a value indicating whether this dockable is currently active.</summary>
		bool IsActive { get; set; }

		/// <summary>Gets or sets the current docking state.</summary>
		DockState DockState { get; set; }

		/// <summary>Called when this dockable is about to close.</summary>
		/// <returns>True if the close should proceed; false to cancel.</returns>
		bool OnClose();

		/// <summary>Called when this dockable is selected.</summary>
		void OnSelected();
	}
}