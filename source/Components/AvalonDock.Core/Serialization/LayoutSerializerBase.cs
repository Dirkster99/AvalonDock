#nullable disable
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AvalonDock.Core.Serialization;
using AvalonDock.Core.Serialization.Dto;

namespace AvalonDock.Core
{
	/// <summary>
	/// Event args for the layout serialization callback, allowing the client
	/// to attach content to deserialized items or cancel their restoration.
	/// </summary>
	public class LayoutSerializationCallbackEventArgs : CancelEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutSerializationCallbackEventArgs"/> class.
		/// </summary>
		/// <param name="model">The model of the view being deserialized.</param>
		/// <param name="previousContent">The content from the previous layout, if available.</param>
		/// <param name="handling">
		/// What happens to the item when the handler supplies no content, taken from the serializer.
		/// </param>
		public LayoutSerializationCallbackEventArgs(
			ISerializableLayoutContent model,
			object previousContent,
			UnresolvedContentHandling handling = UnresolvedContentHandling.Remove)
		{
			Cancel = false;
			Model = model;
			Content = previousContent;
			UnresolvedContentHandling = handling;
		}

		/// <summary>Gets the model of the layout item being deserialized.</summary>
		public ISerializableLayoutContent Model { get; }

		/// <summary>Gets or sets the content to assign to the deserialized item.</summary>
		public object Content { get; set; }

		/// <summary>
		/// Gets or sets what happens to this item when the handler supplies no content.
		/// </summary>
		/// <remarks>
		/// Starts out at the serializer wide setting and can be changed for this one item. Only the
		/// application can tell a stored tool window that will never come back from one whose plugin
		/// simply has not loaded yet, so this is where that knowledge belongs: park the ones whose
		/// content is still expected with <see cref="UnresolvedContentHandling.Hide"/> and let the
		/// rest be removed. Ignored for documents, which have no hidden state, and when
		/// <see cref="CancelEventArgs.Cancel"/> is set.
		/// </remarks>
		public UnresolvedContentHandling UnresolvedContentHandling { get; set; }
	}

	/// <summary>
	/// Decides what happens to an item of a stored layout whose content cannot be resolved -
	/// typically a tool window that the application does not offer any more.
	/// </summary>
	public enum UnresolvedContentHandling
	{
		/// <summary>
		/// Drop the item from the layout. Nothing is left behind: no empty pane, no entry in the
		/// hidden list, and nothing to carry into the next saved layout.
		/// </summary>
		Remove,

		/// <summary>
		/// Hide the item, keeping it in <c>LayoutRoot.Hidden</c> together with the pane and index it
		/// was stored at. An application that supplies the content later - a plugin that loads after
		/// the layout was restored, say - can then show it again in its remembered position, at the
		/// price of carrying the entry through every save.
		/// </summary>
		Hide,
	}

	/// <summary>
	/// UI-independent base class for layout serialization/deserialization.
	/// Implements <see cref="ILayoutSerializer"/> with a template-method pattern:
	/// subclasses provide format-specific <see cref="SerializeCore"/> and
	/// <see cref="DeserializeCore"/>, while the base handles lifecycle and fixup.
	/// </summary>
	public abstract class LayoutSerializerBase : ILayoutSerializer
	{
		private ISerializableLayoutAnchorable[] _previousAnchorables;
		private ISerializableLayoutDocument[] _previousDocuments;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutSerializerBase"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		protected LayoutSerializerBase(IDockingManager manager)
		{
			Manager = manager ?? throw new ArgumentNullException(nameof(manager));
		}

		/// <summary>
		/// Raised during deserialization to let the client attach content to layout items.
		/// </summary>
		public event EventHandler<LayoutSerializationCallbackEventArgs> LayoutSerializationCallback;

		/// <summary>Gets the docking manager whose layout is being serialized.</summary>
		public IDockingManager Manager { get; }

		/// <summary>
		/// Gets or sets what happens to a stored anchorable whose content cannot be resolved.
		/// Defaults to <see cref="UnresolvedContentHandling.Remove"/>.
		/// </summary>
		/// <remarks>
		/// Documents are always removed: a document has no hidden state to be parked in. Set this to
		/// <see cref="UnresolvedContentHandling.Hide"/> to get the AvalonDock v4 behaviour for
		/// anchorables, where an unresolved tool window stayed in <c>LayoutRoot.Hidden</c>.
		/// </remarks>
		public UnresolvedContentHandling UnresolvedContentHandling { get; set; } = UnresolvedContentHandling.Remove;

		/// <inheritdoc/>
		public void Serialize(Stream stream)
		{
			var dto = Manager.DtoMapper.ToDto(Manager.Layout);
			SerializeCore(stream, dto);
		}

		/// <inheritdoc/>
		public void Deserialize(Stream stream)
		{
			try
			{
				StartDeserialization();
				CaptureCurrentState();
				var dto = DeserializeCore(stream);
				var layout = Manager.DtoMapper.FromDto(dto);
				FixupLayout(layout);
				Manager.Layout = layout;
			}
			finally
			{
				EndDeserialization();
			}
		}

		/// <summary>Serializes the current docking layout to a <see cref="TextWriter"/>.</summary>
		/// <param name="writer">The text writer to write to.</param>
		/// <remarks>
		/// Forwards to <see cref="LayoutSerializerExtensions"/>, which carries the one implementation.
		/// The method exists here as well because an instance method wins overload resolution over an
		/// extension method, so without it a caller holding a concrete serializer would not see the
		/// extension at all.
		/// </remarks>
		public void Serialize(TextWriter writer) => LayoutSerializerExtensions.Serialize(this, writer);

		/// <summary>Serializes the current docking layout to a <see cref="StreamWriter"/>.</summary>
		/// <param name="writer">The stream writer to write to.</param>
		public void Serialize(StreamWriter writer)
		{
			Serialize((TextWriter)writer);
		}

		/// <summary>Deserializes a docking layout from a <see cref="TextReader"/> and applies fixup.</summary>
		/// <param name="reader">The text reader to read from.</param>
		/// <remarks>
		/// Forwards to <see cref="LayoutSerializerExtensions"/> for the same reason as
		/// <see cref="Serialize(TextWriter)"/>.
		/// </remarks>
		public void Deserialize(TextReader reader) => LayoutSerializerExtensions.Deserialize(this, reader);

		/// <summary>Deserializes a docking layout from a <see cref="StreamReader"/> and applies fixup.</summary>
		/// <param name="reader">The stream reader to read from.</param>
		public void Deserialize(StreamReader reader)
		{
			Deserialize((TextReader)reader);
		}

		/// <inheritdoc/>
		public void Serialize(string filepath)
		{
			using var stream = File.Create(filepath);
			Serialize(stream);
		}

		/// <inheritdoc/>
		public void Deserialize(string filepath)
		{
			using var stream = File.OpenRead(filepath);
			Deserialize(stream);
		}

		/// <summary>Writes the layout DTO to the stream in the concrete format (XML, JSON, etc.).</summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="dto">The layout root DTO to serialize.</param>
		protected abstract void SerializeCore(Stream stream, LayoutRootDto dto);

		/// <summary>Reads a layout DTO from the stream in the concrete format.</summary>
		/// <param name="stream">The stream to read from.</param>
		/// <returns>The deserialized layout root DTO.</returns>
		protected abstract LayoutRootDto DeserializeCore(Stream stream);

		/// <summary>
		/// Fixes up a deserialized layout: reconnects previous containers,
		/// restores anchorable/document content via callback or prior state.
		/// </summary>
		/// <param name="layout">The deserialized layout root to fix up.</param>
		/// <remarks>
		/// An item of the stored layout whose content cannot be resolved - typically a tool window that
		/// the application does not offer any more - is dropped from the layout. Leaving it in place
		/// would show an empty pane or tab carrying nothing but the stored title, and every save/load
		/// cycle would carry that ghost forward. See <see cref="UnresolvedContentHandling"/> to keep
		/// unresolved anchorables in the hidden list instead.
		/// </remarks>
		protected virtual void FixupLayout(ISerializableLayoutRoot layout)
		{
			// Fix cached root references
			foreach (var element in layout.Descendents())
				element.FixCachedRootOnDeserialize();

			// Reconnect previous containers
			foreach (var lcToAttach in layout.Descendents().OfType<ISerializablePreviousContainer>()
				.Where(lc => lc.PreviousContainerId != null))
			{
				var pane = layout.Descendents().OfType<ISerializableLayoutPane>()
					.FirstOrDefault(p => p.Id == lcToAttach.PreviousContainerId);
				if (pane == null)
				{
					lcToAttach.PreviousContainerId = null;
					continue;
				}

				lcToAttach.PreviousContainer = pane as ISerializableLayoutContainer;
			}

			// Restore anchorable content
			foreach (var lcToFix in layout.Descendents().OfType<ISerializableLayoutAnchorable>()
				.Where(lc => lc.Content == null).ToArray())
			{
				ISerializableLayoutAnchorable previous = null;
				if (lcToFix.ContentId != null)
					previous = _previousAnchorables.FirstOrDefault(a => a.ContentId == lcToFix.ContentId);

				if (previous != null && previous.Title != null)
					lcToFix.Title = previous.Title;

				if (LayoutSerializationCallback != null)
				{
					var args = new LayoutSerializationCallbackEventArgs(
						lcToFix, previous?.Content, UnresolvedContentHandling);
					LayoutSerializationCallback(this, args);

					// A cancelled anchorable is dropped rather than closed. Closing an auto hidden
					// anchorable restores its whole anchor group - siblings included - to the docked
					// area on the way out, which is not what refusing a stored item should do.
					if (args.Cancel)
						lcToFix.RemoveFromLayout();
					else if (args.Content != null)
						lcToFix.Content = args.Content;
					else if (args.Model.Content == null)
						DiscardUnresolved(lcToFix, args.UnresolvedContentHandling);
				}
				else if (previous == null)
				{
					DiscardUnresolved(lcToFix, UnresolvedContentHandling);
				}
				else
				{
					lcToFix.Content = previous.Content;
					lcToFix.IconSource = previous.IconSource;
				}
			}

			// Restore document content
			foreach (var lcToFix in layout.Descendents().OfType<ISerializableLayoutDocument>()
				.Where(lc => lc.Content == null).ToArray())
			{
				ISerializableLayoutDocument previous = null;
				if (lcToFix.ContentId != null)
					previous = _previousDocuments.FirstOrDefault(a => a.ContentId == lcToFix.ContentId);

				if (LayoutSerializationCallback != null)
				{
					var args = new LayoutSerializationCallbackEventArgs(lcToFix, previous?.Content);
					LayoutSerializationCallback(this, args);
					if (args.Cancel)
						lcToFix.Close();
					else if (args.Content != null)
						lcToFix.Content = args.Content;
					else if (args.Model.Content == null)
						lcToFix.Close();
				}
				else if (previous == null)
				{
					lcToFix.Close();
				}
				else
				{
					lcToFix.Content = previous.Content;
					lcToFix.IconSource = previous.IconSource;
				}
			}

			layout.CollectGarbage();
		}

		/// <summary>
		/// Disposes of an anchorable of the stored layout whose content could not be resolved.
		/// </summary>
		/// <param name="anchorable">The unresolved anchorable.</param>
		/// <param name="handling">What to do with it.</param>
		private static void DiscardUnresolved(
			ISerializableLayoutAnchorable anchorable,
			UnresolvedContentHandling handling)
		{
			if (handling == UnresolvedContentHandling.Hide)
				anchorable.HideAnchorable(false);
			else
				anchorable.RemoveFromLayout();
		}

		private void CaptureCurrentState()
		{
			_previousAnchorables = Manager.Layout?.Descendents()
				.OfType<ISerializableLayoutAnchorable>().ToArray()
				?? Array.Empty<ISerializableLayoutAnchorable>();
			_previousDocuments = Manager.Layout?.Descendents()
				.OfType<ISerializableLayoutDocument>().ToArray()
				?? Array.Empty<ISerializableLayoutDocument>();
		}

		private void StartDeserialization()
		{
			Manager.SuspendDocumentsSourceBinding = true;
			Manager.SuspendAnchorablesSourceBinding = true;
		}

		private void EndDeserialization()
		{
			Manager.SuspendDocumentsSourceBinding = false;
			Manager.SuspendAnchorablesSourceBinding = false;
		}
	}
}