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
		public LayoutSerializationCallbackEventArgs(ISerializableLayoutContent model, object previousContent)
		{
			Cancel = false;
			Model = model;
			Content = previousContent;
		}

		/// <summary>Gets the model of the layout item being deserialized.</summary>
		public ISerializableLayoutContent Model { get; }

		/// <summary>Gets or sets the content to assign to the deserialized item.</summary>
		public object Content { get; set; }
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

		/// <inheritdoc/>
		public virtual void Serialize(string filepath)
		{
			using var stream = File.Create(filepath);
			Serialize(stream);
		}

		/// <inheritdoc/>
		public virtual void Deserialize(string filepath)
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
					var args = new LayoutSerializationCallbackEventArgs(lcToFix, previous?.Content);
					LayoutSerializationCallback(this, args);
					if (args.Cancel)
						lcToFix.Close();
					else if (args.Content != null)
						lcToFix.Content = args.Content;
					else if (args.Model.Content != null)
						lcToFix.HideAnchorable(false);
				}
				else if (previous == null)
				{
					lcToFix.HideAnchorable(false);
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
					else if (args.Model.Content != null)
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