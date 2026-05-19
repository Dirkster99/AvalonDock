#nullable disable
using System;
using System.ComponentModel;
using AvalonDock.Core.Serialization;

namespace AvalonDock.Core.Events
{
	/// <summary>
	/// Event args for document lifecycle events (closing, closed).
	/// Uses Core serialization interfaces to avoid WPF coupling.
	/// </summary>
	public class DocumentEventArgs : EventArgs
	{
		public DocumentEventArgs(ISerializableLayoutDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		/// <summary>Gets the document involved in the event.</summary>
		public ISerializableLayoutDocument Document { get; }
	}

	/// <summary>
	/// Cancelable event args for document closing.
	/// </summary>
	public class DocumentCancelEventArgs : CancelEventArgs
	{
		public DocumentCancelEventArgs(ISerializableLayoutDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		/// <summary>Gets the document being closed.</summary>
		public ISerializableLayoutDocument Document { get; }
	}

	/// <summary>
	/// Event args for anchorable lifecycle events (closing, closed, hidden).
	/// </summary>
	public class AnchorableEventArgs : EventArgs
	{
		public AnchorableEventArgs(ISerializableLayoutAnchorable anchorable)
		{
			Anchorable = anchorable ?? throw new ArgumentNullException(nameof(anchorable));
		}

		/// <summary>Gets the anchorable involved in the event.</summary>
		public ISerializableLayoutAnchorable Anchorable { get; }
	}

	/// <summary>
	/// Cancelable event args for anchorable closing/hiding.
	/// </summary>
	public class AnchorableCancelEventArgs : CancelEventArgs
	{
		public AnchorableCancelEventArgs(ISerializableLayoutAnchorable anchorable)
		{
			Anchorable = anchorable ?? throw new ArgumentNullException(nameof(anchorable));
		}

		/// <summary>Gets the anchorable being closed or hidden.</summary>
		public ISerializableLayoutAnchorable Anchorable { get; }

		/// <summary>Gets or sets whether to close (remove) instead of hide.</summary>
		public bool CloseInsteadOfHide { get; set; }
	}

	/// <summary>
	/// Event args for content floating/docking events.
	/// </summary>
	public class ContentEventArgs : EventArgs
	{
		public ContentEventArgs(ISerializableLayoutContent content)
		{
			Content = content ?? throw new ArgumentNullException(nameof(content));
		}

		/// <summary>Gets the content involved in the event.</summary>
		public ISerializableLayoutContent Content { get; }
	}

	/// <summary>
	/// Cancelable event args for content about to float or dock.
	/// </summary>
	public class ContentCancelEventArgs : CancelEventArgs
	{
		public ContentCancelEventArgs(ISerializableLayoutContent content)
		{
			Content = content ?? throw new ArgumentNullException(nameof(content));
		}

		/// <summary>Gets the content involved in the event.</summary>
		public ISerializableLayoutContent Content { get; }
	}
}