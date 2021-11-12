/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;

namespace AvalonDock.Layout.Serialization
{
	/// <summary>Implements a base class for the layout serialization/deserialization of the docking framework.</summary>
	public abstract class LayoutSerializer
	{
		#region fields

		private LayoutAnchorable[] _previousAnchorables = null;
		private LayoutDocument[] _previousDocuments = null;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor from <see cref="DockingManager"/> instance.
		/// </summary>
		/// <param name="manager"></param>
		public LayoutSerializer(DockingManager manager)
		{
			Manager = manager ?? throw new ArgumentNullException(nameof(manager));
			_previousAnchorables = Manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
			_previousDocuments = Manager.Layout.Descendents().OfType<LayoutDocument>().ToArray();
		}

		#endregion Constructors

		#region Events

		/// <summary>Raises an event when the layout serializer is about to deserialize an item to ask the
		/// client application whether the item should be deserialized and re-displayed and what content
		/// should be used if so.
		/// </summary>
		public event EventHandler<LayoutSerializationCallbackEventArgs> LayoutSerializationCallback;

		#endregion Events

		#region Properties

		/// <summary>
		/// Gets the <see cref="DockingManager"/> root of the docking library.
		/// </summary>
		public DockingManager Manager { get; }

		#endregion Properties

		#region Methods

		protected virtual void FixupLayout(LayoutRoot layout)
		{
			//fix container panes
			foreach (var lcToAttach in layout.Descendents().OfType<ILayoutPreviousContainer>().Where(lc => lc.PreviousContainerId != null))
			{
				var paneContainerToAttach = layout.Descendents().OfType<ILayoutPaneSerializable>().FirstOrDefault(lps => lps.Id == lcToAttach.PreviousContainerId);
				if (paneContainerToAttach == null)
					throw new ArgumentException($"Unable to find a pane with id ='{lcToAttach.PreviousContainerId}'");
				lcToAttach.PreviousContainer = paneContainerToAttach as ILayoutContainer;
			}

			//now fix the content of the layout anchorable contents
			foreach (var lcToFix in layout.Descendents().OfType<LayoutAnchorable>().Where(lc => lc.Content == null).ToArray())
			{
				LayoutAnchorable previousAchorable = null;            //try find the content in replaced layout
				if (lcToFix.ContentId != null)
					previousAchorable = _previousAnchorables.FirstOrDefault(a => a.ContentId == lcToFix.ContentId);

				if (previousAchorable != null && previousAchorable.Title != null)
					lcToFix.Title = previousAchorable.Title;

				if (LayoutSerializationCallback != null)
				{
					// Ask client application via callback if item should be deserialized
					var args = new LayoutSerializationCallbackEventArgs(lcToFix, previousAchorable?.Content);
					LayoutSerializationCallback(this, args);
					if (args.Cancel)
						lcToFix.Close();
					else if (args.Content != null)
						lcToFix.Content = args.Content;
					else if (args.Model.Content != null)
						lcToFix.Hide(false);           // hide layoutanchorable if client app supplied no content
				}
				else if (previousAchorable == null)  // No Callback and no provious document -> skip this
					lcToFix.Hide(false);
				else
				{   // No Callback but previous anchoreable available -> load content from previous document
					lcToFix.Content = previousAchorable.Content;
					lcToFix.IconSource = previousAchorable.IconSource;
				}
			}

			//now fix the content of the layout document contents
			foreach (var lcToFix in layout.Descendents().OfType<LayoutDocument>().Where(lc => lc.Content == null).ToArray())
			{
				LayoutDocument previousDocument = null;               //try find the content in replaced layout
				if (lcToFix.ContentId != null)
					previousDocument = _previousDocuments.FirstOrDefault(a => a.ContentId == lcToFix.ContentId);

				if (LayoutSerializationCallback != null)
				{
					// Ask client application via callback if this realy should be deserialized
					var args = new LayoutSerializationCallbackEventArgs(lcToFix, previousDocument?.Content);
					LayoutSerializationCallback(this, args);

					if (args.Cancel)
						lcToFix.Close();
					else if (args.Content != null)
						lcToFix.Content = args.Content;
					else if (args.Model.Content != null)  // Close document if client app supplied no content
						lcToFix.Close();
				}
				else if (previousDocument == null)  // No Callback and no provious document -> skip this
					lcToFix.Close();
				else
				{   // No Callback but previous document available -> load content from previous document
					lcToFix.Content = previousDocument.Content;
					lcToFix.IconSource = previousDocument.IconSource;
				}
			}

			layout.CollectGarbage();
		}

		protected void StartDeserialization()
		{
			Manager.SuspendDocumentsSourceBinding = true;
			Manager.SuspendAnchorablesSourceBinding = true;
		}

		protected void EndDeserialization()
		{
			Manager.SuspendDocumentsSourceBinding = false;
			Manager.SuspendAnchorablesSourceBinding = false;
		}

		#endregion Methods
	}
}