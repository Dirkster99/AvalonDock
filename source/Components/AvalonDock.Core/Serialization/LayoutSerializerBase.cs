#nullable disable
using System;
using System.ComponentModel;
using System.Linq;
using AvalonDock.Core.Serialization;

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
	/// Works against <see cref="ISerializableDockingManager"/> and related interfaces.
	/// </summary>
	public abstract class LayoutSerializerBase
	{
		private ISerializableLayoutAnchorable[] _previousAnchorables;
		private ISerializableLayoutDocument[] _previousDocuments;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutSerializerBase"/> class.
		/// </summary>
		/// <param name="manager">The docking manager to serialize.</param>
		protected LayoutSerializerBase(ISerializableDockingManager manager)
		{
			Manager = manager ?? throw new ArgumentNullException(nameof(manager));
			_previousAnchorables = Manager.Layout.Descendents()
				.OfType<ISerializableLayoutAnchorable>().ToArray();
			_previousDocuments = Manager.Layout.Descendents()
				.OfType<ISerializableLayoutDocument>().ToArray();
		}

		/// <summary>
		/// Raised during deserialization to let the client attach content to layout items.
		/// </summary>
		public event EventHandler<LayoutSerializationCallbackEventArgs> LayoutSerializationCallback;

		/// <summary>Gets the docking manager being serialized.</summary>
		public ISerializableDockingManager Manager { get; }

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

		/// <summary>Suspends source bindings before deserialization.</summary>
		protected void StartDeserialization()
		{
			Manager.SuspendDocumentsSourceBinding = true;
			Manager.SuspendAnchorablesSourceBinding = true;
		}

		/// <summary>Resumes source bindings after deserialization.</summary>
		protected void EndDeserialization()
		{
			Manager.SuspendDocumentsSourceBinding = false;
			Manager.SuspendAnchorablesSourceBinding = false;
		}
	}
}