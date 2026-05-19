#nullable disable
using System;
using System.Collections;
using AvalonDock.Core.Events;
using AvalonDock.Core.Serialization;

namespace AvalonDock.Core
{
	/// <summary>
	/// UI-agnostic interface for the docking manager.
	/// Provides layout management, content sources, active content tracking,
	/// and lifecycle events without requiring WPF references.
	/// </summary>
	/// <remarks>
	/// <para>Implemented by the WPF <c>DockingManager</c> class.</para>
	/// <para>ViewModels can depend on this interface to interact with the docking
	/// system without coupling to WPF controls.</para>
	/// </remarks>
	public interface IDockingManager : ISerializableDockingManager
	{
		/// <summary>
		/// Gets or sets the MVVM layout model. When set, the docking manager
		/// syncs this ViewModel tree with the internal WPF layout automatically.
		/// If null, the docking manager operates in classic (v4.x) mode.
		/// </summary>
		IRootDock DockLayout { get; set; }

		/// <summary>Gets or sets the currently active content (selected document or anchorable).</summary>
		object ActiveContent { get; set; }

		/// <summary>Gets or sets the source collection for documents.</summary>
		IEnumerable DocumentsSource { get; set; }

		/// <summary>Gets or sets the source collection for anchorables (tool windows).</summary>
		IEnumerable AnchorablesSource { get; set; }

		/// <summary>Gets or sets the delay in milliseconds before auto-hiding a tool window.</summary>
		int AutoHideDelay { get; set; }

		/// <summary>Gets or sets a value indicating whether mixed orientation (horizontal + vertical splits) is allowed.</summary>
		bool AllowMixedOrientation { get; set; }

		/// <summary>Raised when the active content changes.</summary>
		event EventHandler ActiveContentChanged;

		/// <summary>Raised after the layout has changed.</summary>
		event EventHandler LayoutChanged;

		/// <summary>Raised before the layout is about to change.</summary>
		event EventHandler LayoutChanging;

		/// <summary>Raised when a document is about to close. Can be cancelled.</summary>
		event EventHandler<DocumentCancelEventArgs> DocumentClosing;

		/// <summary>Raised after a document has been closed.</summary>
		event EventHandler<DocumentEventArgs> DocumentClosed;

		/// <summary>Raised when an anchorable is about to close. Can be cancelled.</summary>
		event EventHandler<AnchorableCancelEventArgs> AnchorableClosing;

		/// <summary>Raised after an anchorable has been closed.</summary>
		event EventHandler<AnchorableEventArgs> AnchorableClosed;

		/// <summary>Raised when an anchorable is about to be hidden. Can be cancelled.</summary>
		event EventHandler<AnchorableCancelEventArgs> AnchorableHiding;

		/// <summary>Raised after an anchorable has been hidden.</summary>
		event EventHandler<AnchorableEventArgs> AnchorableHidden;

		/// <summary>Raised when content is about to float. Can be cancelled.</summary>
		event EventHandler<ContentCancelEventArgs> ContentFloating;

		/// <summary>Raised after content has been floated.</summary>
		event EventHandler<ContentEventArgs> ContentFloated;

		/// <summary>Raised when content is about to dock. Can be cancelled.</summary>
		event EventHandler<ContentCancelEventArgs> ContentDocking;

		/// <summary>Raised after content has been docked.</summary>
		event EventHandler<ContentEventArgs> ContentDocked;
	}
}