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
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentEventArgs"/> class.
		/// </summary>
		/// <param name="document">The document involved in the event.</param>
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
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentCancelEventArgs"/> class.
		/// </summary>
		/// <param name="document">The document being closed.</param>
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
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorableEventArgs"/> class.
		/// </summary>
		/// <param name="anchorable">The anchorable involved in the event.</param>
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
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorableCancelEventArgs"/> class.
		/// </summary>
		/// <param name="anchorable">The anchorable being closed or hidden.</param>
		public AnchorableCancelEventArgs(ISerializableLayoutAnchorable anchorable)
		{
			Anchorable = anchorable ?? throw new ArgumentNullException(nameof(anchorable));
		}

		/// <summary>Gets the anchorable being closed or hidden.</summary>
		public ISerializableLayoutAnchorable Anchorable { get; }

		/// <summary>Gets or sets a value indicating whether to close (remove) instead of hide.</summary>
		public bool CloseInsteadOfHide { get; set; }
	}

	/// <summary>
	/// Event args for content floating/docking events.
	/// </summary>
	public class ContentEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentEventArgs"/> class.
		/// </summary>
		/// <param name="content">The content involved in the event.</param>
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
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentCancelEventArgs"/> class.
		/// </summary>
		/// <param name="content">The content involved in the event.</param>
		public ContentCancelEventArgs(ISerializableLayoutContent content)
		{
			Content = content ?? throw new ArgumentNullException(nameof(content));
		}

		/// <summary>Gets the content involved in the event.</summary>
		public ISerializableLayoutContent Content { get; }
	}
}