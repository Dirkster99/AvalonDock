/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AvalonDock.Layout.Serialization
{
	/// <summary>Implements a base class for the layout serialization/deserialization of the docking framework.</summary>
	public abstract class LayoutSerializerBase : IDisposable
	{
		#region Properties

		#region PreviousAnchorables
		protected IEnumerable<LayoutAnchorable> PreviousAnchorables => _previousAnchorables;
		private readonly LayoutAnchorable[] _previousAnchorables = null;

		#endregion PreviousAnchorables

		#region PreviousDocuments

		protected IEnumerable<LayoutDocument> PreviousDocuments => _previousDocuments;
		private readonly LayoutDocument[] _previousDocuments = null;

		#endregion PreviousDocuments

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor from <see cref="DockingManager"/> instance.
		/// </summary>
		/// <param name="manager"></param>
		public LayoutSerializerBase(DockingManager manager)
		{
			Manager = manager ?? throw new ArgumentNullException(nameof(manager));
			Manager.SuspendDocumentsSourceBinding = true;
			Manager.SuspendAnchorablesSourceBinding = true;

			_previousAnchorables = Manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
			_previousDocuments = Manager.Layout.Descendents().OfType<LayoutDocument>().ToArray();
			_layoutRestore = new List<LayoutRestoreDelegate>();
		}

		#endregion Constructors

		#region Delegates

		/// <summary>
		/// Method descriptor for <see cref="LayoutRestore"/>.
		/// </summary>
		/// <param name="sender">The layout serializer.</param>
		/// <param name="e">An instance of <see cref="LayoutRestoreEventArgs"/> that allows to cancel the creation and/or the further processing.</param>
		/// <returns></returns>
		public delegate Task LayoutRestoreDelegate(object sender, LayoutRestoreEventArgs e);

		#endregion

		#region Events


		/// <summary>
		/// Raises an event when the layout serializer is about to deserialize an item to ask the
		/// client application whether the item should be deserialized and re-displayed and what content
		/// should be used if so.
		/// </summary>
		public event LayoutRestoreDelegate LayoutRestore
		{
			add => _layoutRestore.Add(value);
			remove => _layoutRestore.Remove(value);
		}
		private List<LayoutRestoreDelegate> _layoutRestore;

		#endregion Events

		#region Properties

		/// <summary>
		/// Gets the <see cref="DockingManager"/> root of the docking library.
		/// </summary>
		public DockingManager Manager { get; }

		#endregion Properties

		#region Protected Methods

		protected virtual void Dispose()
		{
			Manager.SuspendDocumentsSourceBinding = false;
			Manager.SuspendAnchorablesSourceBinding = false;
		}

		/// <summary>
		/// Fixes the <see cref="ILayoutPreviousContainer.PreviousContainer"/> reference after
		/// deserializing the <paramref name="layoutRoot"/> to point towards the matching container again.
		/// </summary>
		/// <remarks>
		/// Uses first occurance where <see cref="ILayoutPaneSerializable.Id"/>
		/// is equivalent to <see cref="ILayoutPreviousContainer.PreviousContainerId"/>.
		/// </remarks>
		/// <param name="layoutRoot"></param>
		protected virtual async Task FixupPreviousContainerReference(LayoutRoot layoutRoot)
		{
			foreach (var lcToAttach in layoutRoot.Descendents()
				.OfType<ILayoutPreviousContainer>().Where(lc => lc.PreviousContainerId != null))
			{
				var paneContainerToAttach = layoutRoot.Descendents()
					.OfType<ILayoutPaneSerializable>().FirstOrDefault(lps => lps.Id == lcToAttach.PreviousContainerId);
				if (!(paneContainerToAttach is ILayoutContainer layoutContainer))
				{
					throw new ArgumentException($"Unable to find a pane with id ='{lcToAttach.PreviousContainerId}'");
				}
				await Application.Current.Dispatcher.InvokeAsync(() => lcToAttach.PreviousContainer = layoutContainer);
			}
		}

		protected virtual async Task ReapplyAnchorablesContent(LayoutRoot layout)
		{
			foreach (var lcToFix in layout.Descendents().OfType<LayoutAnchorable>().Where(lc => lc.Content == null).ToArray())
			{
				// Try find the content in replaced layout
				LayoutAnchorable previousAchorable = null;
				if (lcToFix.ContentId != null)
				{
					previousAchorable = _previousAnchorables.FirstOrDefault(a => a.ContentId == lcToFix.ContentId);
				}

				if (_layoutRestore.Any())
				{
					// Ask client application via callback if item should be deserialized
					var eventArgs = new LayoutRestoreEventArgs(lcToFix, previousAchorable?.Content);
					foreach (var callback in _layoutRestore)
					{
						await callback(this, eventArgs);
						if (eventArgs.Cancel || eventArgs.Handled)
						{
							break;
						}
					}
					// Close anchorable if client app decided to cancel it
					if (eventArgs.Cancel)
					{
						await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Close());
					}
					// update anchorable content if client provided content
					else if (eventArgs.Content != null)
					{
						await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Content = eventArgs.Content);
					}
					// If client has not provided any content and
					// has not explicitly set the content on the LayoutContent
					// then hide the anchorable
					else if (eventArgs.Model.Content != null)
					{
						await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Hide(false));
					}
				}
				// Ensure a previousAnchorable exists, otherwise hide this (skip)
				else if (previousAchorable == null)
				{
					await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Hide(false));
				}
				// Load content from previous anchorable
				else
				{
					lcToFix.Content = previousAchorable.Content;
					lcToFix.IconSource = previousAchorable.IconSource;
				}
			}
		}
		protected virtual async Task ReapplyDocumentsContent(LayoutRoot layout)
		{
			foreach (var lcToFix in layout.Descendents().OfType<LayoutDocument>().Where(lc => lc.Content == null).ToArray())
			{
				// Try find the content in replaced layout
				LayoutDocument previousDocument = null;
				if (lcToFix.ContentId != null)
				{
					previousDocument = _previousDocuments.FirstOrDefault(a => a.ContentId == lcToFix.ContentId);
				}

				if (_layoutRestore.Any())
				{
					// Ask client application via callback if item should be deserialized
					var eventArgs = new LayoutRestoreEventArgs(lcToFix, previousDocument?.Content);
					foreach (var callback in _layoutRestore)
					{
						await callback(this, eventArgs);
						if (eventArgs.Cancel || eventArgs.Handled)
						{
							break;
						}
					}
					// Close anchorable if client app decided to cancel it
					if (eventArgs.Cancel)
					{
						await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Close());
					}
					// update anchorable content if client provided content
					else if (eventArgs.Content != null)
					{
						await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Content = eventArgs.Content);
					}
					// If client has not provided any content and
					// has not explicitly set the content on the LayoutContent
					// then hide the anchorable
					else if (eventArgs.Model.Content != null)
					{
						await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Close());
					}
				}
				// Ensure a previousAnchorable exists, otherwise hide this (skip)
				else if (previousDocument == null)
				{
					await Application.Current.Dispatcher.InvokeAsync(() => lcToFix.Close());
					
				}
				// Load content from previous anchorable
				else
				{
					lcToFix.Content = previousDocument.Content;
					lcToFix.IconSource = previousDocument.IconSource;
				}
			}
		}
		protected virtual async Task FixupLayout(LayoutRoot layout)
		{
			await FixupPreviousContainerReference(layout);
			await ReapplyAnchorablesContent(layout);
			await ReapplyDocumentsContent(layout);

			await Application.Current.Dispatcher.InvokeAsync(() => layout.CollectGarbage());
		}

		#endregion Methods

		#region IDisposable

		void IDisposable.Dispose() => this.Dispose();

		#endregion IDisposable
	}
}