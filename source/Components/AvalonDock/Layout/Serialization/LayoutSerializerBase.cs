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
using System.Windows.Threading;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

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
		protected LayoutSerializerBase(DockingManager manager) : this(manager, Application.Current.Dispatcher)
		{
		}

		/// <summary>
		/// Class constructor from <see cref="DockingManager"/> instance.
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="staDispatcher">A <see cref="System.Windows.Threading.Dispatcher"/> that dispatches to the STA thread.</param>
		protected LayoutSerializerBase(DockingManager manager, Dispatcher staDispatcher)
		{
			Manager = manager ?? throw new ArgumentNullException(nameof(manager));
			Manager.SuspendDocumentsSourceBinding = true;
			Manager.SuspendAnchorablesSourceBinding = true;

			_previousAnchorables = Manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
			_previousDocuments = Manager.Layout.Descendents().OfType<LayoutDocument>().ToArray();
			_layoutRestore = new List<LayoutRestoreDelegate>();
			Dispatcher = staDispatcher;
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

		private readonly List<LayoutRestoreDelegate> _layoutRestore;

		#endregion Events

		#region Properties

		/// <summary>
		/// Gets the <see cref="DockingManager"/> root of the docking library.
		/// </summary>
		public DockingManager Manager { get; }

		protected Dispatcher Dispatcher { get; }

		#endregion Properties

		#region Private Methods


		private static IEnumerable<ILayoutPreviousContainer> GetDescendentsWithPreviousContainerId(LayoutRoot layoutRoot)
		{
			return layoutRoot
				.Descendents()
				.OfType<ILayoutPreviousContainer>()
				.Where(lc => lc.PreviousContainerId != null);
		}
		private static IEnumerable<T> GetDescendentsWithoutContent<T>(LayoutRoot layout)
			where T : LayoutContent
		{
			return layout
				.Descendents()
				.OfType<T>()
				.Where(lc => lc.Content == null);
		}

		private async Task HandleLayoutRestoreFallback(
			LayoutContent previousContent,
			LayoutContent lcToFix,
			Action onPreviousContentNull)
		{
			if (previousContent == null)
			{
				await Dispatcher.InvokeAsync(onPreviousContentNull);
			}
			else
			{
				lcToFix.Content = previousContent.Content;
				lcToFix.IconSource = previousContent.IconSource;
			}
		}

		private Task HandleLayoutRestoreFallbackOfAnchorable(
			LayoutAnchorable previousAnchorable,
			LayoutAnchorable lcToFix)
			=> HandleLayoutRestoreFallback(
				previousAnchorable,
				lcToFix,
				() => lcToFix.Hide(false));
		private Task HandleLayoutRestoreFallbackOfDocument(
			LayoutDocument previousAnchorable,
			LayoutDocument lcToFix)
			=> HandleLayoutRestoreFallback(
				previousAnchorable,
				lcToFix,
				// ReSharper disable once ConvertClosureToMethodGroup
				() => lcToFix.Close());

		private async Task<bool> TryHandleLayoutRestoreByUserCallback(
			LayoutContent previousContent,
			LayoutContent lcToFix,
			Action onNoContentAvailable)
		{
			if (!_layoutRestore.Any())
				return false;
			// Ask client application via callback if item should be deserialized
			var eventArgs = await RaiseLayoutRestoreAsync(lcToFix, previousContent);
			// Close anchorable if client app decided to cancel it
			if (eventArgs.Cancel)
			{
				await Dispatcher.InvokeAsync(lcToFix.Close);
			}
			// update anchorable content if client provided content
			else if (eventArgs.Content != null)
			{
				await Dispatcher.InvokeAsync(() => lcToFix.Content = eventArgs.Content);
			}
			// If client has not provided any content and
			// has not explicitly set the content on the LayoutContent
			// then hide the anchorable
			else if (eventArgs.Model.Content != null)
			{
				await Dispatcher.InvokeAsync(onNoContentAvailable);
			}

			return true;
		}

		private Task<bool> TryHandleLayoutRestoreOfAnchorableByUserCallback(
			LayoutAnchorable previousAnchorable,
			LayoutAnchorable lcToFix)
			=> TryHandleLayoutRestoreByUserCallback(
				previousAnchorable,
				lcToFix,
				() => lcToFix.Hide(false));
		private Task<bool> TryHandleLayoutRestoreOfDocumentByUserCallback(
			LayoutDocument previousDocument,
			LayoutDocument lcToFix)
			=> TryHandleLayoutRestoreByUserCallback(
				previousDocument,
				lcToFix,
				// ReSharper disable once ConvertClosureToMethodGroup
				() => lcToFix.Close());


		private LayoutAnchorable GetPreviousAnchorableByContentIdOrDefault(string contentId)
			=> contentId != null
				? _previousAnchorables.FirstOrDefault(a => a.ContentId == contentId)
				: null;
		private LayoutDocument GetPreviousDocumentByContentIdOrDefault(string contentId)
			=> contentId != null
				? _previousDocuments.FirstOrDefault(a => a.ContentId == contentId)
				: null;

		#endregion Private Methods

		#region Protected Methods

		protected async Task<LayoutRestoreEventArgs> RaiseLayoutRestoreAsync(
			LayoutContent layoutContent,
			object content)
		{
			var eventArgs = new LayoutRestoreEventArgs(layoutContent, content);
			foreach (var callback in _layoutRestore)
			{
				await callback(this, eventArgs);
				if (eventArgs.Cancel || eventArgs.Handled)
				{
					break;
				}
			}

			return eventArgs;
		}

		/// <summary>Performs all required actions for deserialization.</summary>
		/// <param name="function">
		/// A function, returning the deserialized <see cref="LayoutRoot"/>,
		/// that is supposed to call a deserialize method.
		/// </param>
		protected async Task DeserializeCommon(Func<LayoutRoot> function)
		{
			var layout = function();
			await FixupLayout(layout);
			Manager.Layout = layout;
		}

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
		/// Uses first occurrence where <see cref="ILayoutPaneSerializable.Id"/>
		/// is equivalent to <see cref="ILayoutPreviousContainer.PreviousContainerId"/>.
		/// </remarks>
		/// <param name="layoutRoot"></param>
		protected virtual async Task FixupPreviousContainerReference(LayoutRoot layoutRoot)
		{
			foreach (var lcToAttach in GetDescendentsWithPreviousContainerId(layoutRoot))
			{
				var paneContainerToAttach = layoutRoot.Descendents()
					.OfType<ILayoutPaneSerializable>()
					.FirstOrDefault(lps => lps.Id == lcToAttach.PreviousContainerId);
				if (!(paneContainerToAttach is ILayoutContainer layoutContainer))
				{
					throw new ArgumentException($"Unable to find a pane with id ='{lcToAttach.PreviousContainerId}'");
				}

				await Dispatcher.InvokeAsync(() => lcToAttach.PreviousContainer = layoutContainer);
			}
		}

		protected virtual async Task ReapplyAnchorablesContent(LayoutRoot layout)
		{
			foreach (var lcToFix in GetDescendentsWithoutContent<LayoutAnchorable>(layout)
				         .ToArray())
			{
				var previousAnchorable = GetPreviousAnchorableByContentIdOrDefault(lcToFix.ContentId);

				if (await TryHandleLayoutRestoreOfAnchorableByUserCallback(previousAnchorable, lcToFix))
					continue;
				await HandleLayoutRestoreFallbackOfAnchorable(previousAnchorable, lcToFix);
			}
		}

		protected virtual async Task ReapplyDocumentsContent(LayoutRoot layout)
		{
			foreach (var lcToFix in GetDescendentsWithoutContent<LayoutDocument>(layout)
				         .ToArray())
			{
				var previousDocument = GetPreviousDocumentByContentIdOrDefault(lcToFix.ContentId);
				if (await TryHandleLayoutRestoreOfDocumentByUserCallback(lcToFix, previousDocument))
					continue;
				await HandleLayoutRestoreFallbackOfDocument(previousDocument, lcToFix);
			}
		}

		protected virtual async Task FixupLayout(LayoutRoot layout)
		{
			await FixupPreviousContainerReference(layout);
			await ReapplyAnchorablesContent(layout);
			await ReapplyDocumentsContent(layout);

			await Dispatcher.InvokeAsync(layout.CollectGarbage);
		}

		#endregion Methods

		#region IDisposable

		void IDisposable.Dispose() => this.Dispose();

		#endregion IDisposable
	}
}