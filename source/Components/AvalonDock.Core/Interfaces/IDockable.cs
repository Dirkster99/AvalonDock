using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Represents the fundamental unit of dockable content in the layout system.
	/// </summary>
	public interface IDockable
	{
		string Id { get; set; }

		string Title { get; set; }

		object? Context { get; set; }

		IDockable? Owner { get; set; }

		IFactory? Factory { get; set; }

		bool CanClose { get; set; }

		bool CanPin { get; set; }

		bool CanFloat { get; set; }

		bool CanDrag { get; set; }

		bool CanDrop { get; set; }

		bool IsModified { get; set; }

		bool IsActive { get; set; }

		DockState DockState { get; set; }

		bool OnClose();

		void OnSelected();
	}
}