#nullable disable
using System;
using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// UI-agnostic interface for floating window management.
	/// Controls creating, enumerating, and closing floating windows.
	/// </summary>
	public interface IFloatingWindowService
	{
		/// <summary>Floats the content with the given content ID into a new floating window.</summary>
		/// <param name="contentId">The content ID of the document or anchorable to float.</param>
		/// <returns>True if the content was found and floated; false otherwise.</returns>
		bool Float(string contentId);

		/// <summary>Gets the number of currently open floating windows.</summary>
		int FloatingWindowCount { get; }

		/// <summary>Closes all floating windows, docking their content back.</summary>
		void CloseAllFloatingWindows();

		/// <summary>Raised when a floating window is created.</summary>
		event EventHandler FloatingWindowCreated;

		/// <summary>Raised when a floating window is closed.</summary>
		event EventHandler FloatingWindowClosed;
	}
}