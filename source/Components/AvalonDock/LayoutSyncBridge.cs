using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using AvalonDock.Core;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Represents the layout Sync Bridge.
	/// </summary>
	internal sealed class LayoutSyncBridge
	{
		private readonly DockingManager _manager;
		private readonly IRootDock _rootDock;
		private ObservableCollection<object> _documentModels;
		private ObservableCollection<object> _anchorableModels;
		private bool _isSyncing;
		private readonly Dictionary<object, AnchorSide> _contentToSide = new Dictionary<object, AnchorSide>();

		/// <summary>
		/// Gets a read-only view of the content-to-side mapping populated during tree walk.
		/// </summary>
		internal IReadOnlyDictionary<object, AnchorSide> ContentToSideMap => _contentToSide;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutSyncBridge"/> class.
		/// </summary>
		/// <param name="manager">The manager.</param>
		/// <param name="rootDock">The root Dock.</param>
		public LayoutSyncBridge(DockingManager manager, IRootDock rootDock)
		{
			_manager = manager ?? throw new ArgumentNullException(nameof(manager));
			_rootDock = rootDock ?? throw new ArgumentNullException(nameof(rootDock));
		}

		/// <summary>
		/// Executes the attach operation.
		/// </summary>
		public void Attach()
		{
			_documentModels = new ObservableCollection<object>();
			_anchorableModels = new ObservableCollection<object>();

			CollectDockables(_rootDock);

			_manager.DocumentsSource = _documentModels;
			_manager.AnchorablesSource = _anchorableModels;

			SyncActiveContentToWpf();

			SubscribeToMvvm();
			SubscribeToWpf();
		}

		/// <summary>
		/// Executes the detach operation.
		/// </summary>
		public void Detach()
		{
			UnsubscribeFromWpf();
			UnsubscribeFromMvvm();

			_manager.DocumentsSource = null;
			_manager.AnchorablesSource = null;

			_contentToSide.Clear();
			_documentModels = null;
			_anchorableModels = null;
		}

		private void CollectDockables(IDockable node)
		{
			if (node is IDocumentDock docDock && docDock.VisibleDockables != null)
			{
				foreach (var child in docDock.VisibleDockables)
				{
					if (!_documentModels.Contains(child))
						_documentModels.Add(child);
				}

				SubscribeCollection(docDock.VisibleDockables, OnDocumentCollectionChanged);
			}
			else if (node is IToolDock toolDock && toolDock.VisibleDockables != null)
			{
				var side = AlignmentToAnchorSide(toolDock.Alignment);
				foreach (var child in toolDock.VisibleDockables)
				{
					if (!_anchorableModels.Contains(child))
						_anchorableModels.Add(child);
					_contentToSide[child] = side;
				}

				SubscribeCollection(toolDock.VisibleDockables, OnAnchorableCollectionChanged);
			}

			if (node is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child is IDock)
						CollectDockables(child);
				}
			}
		}

		private void SubscribeCollection(IList<IDockable> list, NotifyCollectionChangedEventHandler handler)
		{
			if (list is INotifyCollectionChanged ncc)
				ncc.CollectionChanged += handler;
		}

		private void UnsubscribeCollection(IList<IDockable> list, NotifyCollectionChangedEventHandler handler)
		{
			if (list is INotifyCollectionChanged ncc)
				ncc.CollectionChanged -= handler;
		}

		private void SubscribeToMvvm()
		{
			if (_rootDock is INotifyPropertyChanged npc)
				npc.PropertyChanged += OnRootDockPropertyChanged;
		}

		private void UnsubscribeFromMvvm()
		{
			if (_rootDock is INotifyPropertyChanged npc)
				npc.PropertyChanged -= OnRootDockPropertyChanged;

			UnsubscribeAllCollections(_rootDock);
		}

		private void UnsubscribeAllCollections(IDockable node)
		{
			if (node is IDocumentDock docDock && docDock.VisibleDockables != null)
				UnsubscribeCollection(docDock.VisibleDockables, OnDocumentCollectionChanged);
			else if (node is IToolDock toolDock && toolDock.VisibleDockables != null)
				UnsubscribeCollection(toolDock.VisibleDockables, OnAnchorableCollectionChanged);

			if (node is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child is IDock)
						UnsubscribeAllCollections(child);
				}
			}
		}

		private void SubscribeToWpf()
		{
			_manager.ActiveContentChanged += OnWpfActiveContentChanged;
			_manager.DocumentClosed += OnWpfDocumentClosed;
		}

		private void UnsubscribeFromWpf()
		{
			_manager.ActiveContentChanged -= OnWpfActiveContentChanged;
			_manager.DocumentClosed -= OnWpfDocumentClosed;
		}

		private void OnDocumentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_isSyncing || _documentModels == null)
				return;

			_isSyncing = true;
			try
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						foreach (var item in e.NewItems)
						{
							if (!_documentModels.Contains(item))
								_documentModels.Add(item);
						}

						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (var item in e.OldItems)
							_documentModels.Remove(item);
						break;
					case NotifyCollectionChangedAction.Reset:
						RebuildDocuments();
						break;
				}
			}
			finally
			{
				_isSyncing = false;
			}
		}

		private void OnAnchorableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_isSyncing || _anchorableModels == null)
				return;

			_isSyncing = true;
			try
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						var addSide = FindSideForSender(sender);
						foreach (var item in e.NewItems)
						{
							if (!_anchorableModels.Contains(item))
								_anchorableModels.Add(item);
							if (addSide.HasValue)
								_contentToSide[item] = addSide.Value;
						}

						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (var item in e.OldItems)
							_anchorableModels.Remove(item);
						break;
					case NotifyCollectionChangedAction.Reset:
						RebuildAnchorables();
						break;
				}
			}
			finally
			{
				_isSyncing = false;
			}
		}

		private void OnRootDockPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_isSyncing)
				return;

			if (e.PropertyName == nameof(IRootDock.ActiveDockable)
				|| e.PropertyName == nameof(IDock.ActiveDockable))
			{
				SyncActiveContentToWpf();
			}
		}

		private void OnWpfActiveContentChanged(object sender, EventArgs e)
		{
			if (_isSyncing)
				return;

			_isSyncing = true;
			try
			{
				var active = _manager.ActiveContent;
				if (active is IDockable dockable)
					_rootDock.ActiveDockable = dockable;
				else
					FindAndSetActiveDockable(active);
			}
			finally
			{
				_isSyncing = false;
			}
		}

		private void OnWpfDocumentClosed(object sender, DocumentClosedEventArgs e)
		{
			if (_isSyncing)
				return;

			_isSyncing = true;
			try
			{
				var content = e.Document.Content;
				_documentModels?.Remove(content);

				if (content is IDockable dockable)
					RemoveDockableFromTree(_rootDock, dockable, isDocument: true);
			}
			finally
			{
				_isSyncing = false;
			}
		}

		private void RemoveDockableFromTree(IDockable node, IDockable target, bool isDocument)
		{
			if (isDocument && node is IDocumentDock docDock)
			{
				docDock.VisibleDockables?.Remove(target);
				return;
			}

			if (!isDocument && node is IToolDock toolDock)
			{
				toolDock.VisibleDockables?.Remove(target);
				return;
			}

			if (node is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child is IDock)
						RemoveDockableFromTree(child, target, isDocument);
				}
			}
		}

		private void SyncActiveContentToWpf()
		{
			if (_isSyncing)
				return;

			_isSyncing = true;
			try
			{
				var activeDockable = _rootDock.ActiveDockable;
				if (activeDockable != null)
					_manager.ActiveContent = activeDockable;
			}
			finally
			{
				_isSyncing = false;
			}
		}

		private void FindAndSetActiveDockable(object content)
		{
			if (content == null)
			{
				_rootDock.ActiveDockable = null;
				return;
			}

			var match = _documentModels?.OfType<IDockable>().FirstOrDefault(d => d.Context == content)
				?? _anchorableModels?.OfType<IDockable>().FirstOrDefault(d => d.Context == content);

			_rootDock.ActiveDockable = match;
		}

		private void RebuildDocuments()
		{
			_documentModels.Clear();
			RebuildDocumentsFromTree(_rootDock);
		}

		private void RebuildDocumentsFromTree(IDockable node)
		{
			if (node is IDocumentDock docDock && docDock.VisibleDockables != null)
			{
				foreach (var child in docDock.VisibleDockables)
				{
					if (!_documentModels.Contains(child))
						_documentModels.Add(child);
				}
			}

			if (node is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child is IDock)
						RebuildDocumentsFromTree(child);
				}
			}
		}

		private void RebuildAnchorables()
		{
			_anchorableModels.Clear();
			RebuildAnchorablesFromTree(_rootDock);
		}

		private void RebuildAnchorablesFromTree(IDockable node)
		{
			if (node is IToolDock toolDock && toolDock.VisibleDockables != null)
			{
				var side = AlignmentToAnchorSide(toolDock.Alignment);
				foreach (var child in toolDock.VisibleDockables)
				{
					if (!_anchorableModels.Contains(child))
						_anchorableModels.Add(child);
					_contentToSide[child] = side;
				}
			}

			if (node is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child is IDock)
						RebuildAnchorablesFromTree(child);
				}
			}
		}

		private AnchorSide? FindSideForSender(object sender)
		{
			return FindToolDockForCollection(_rootDock, sender) is IToolDock td
				? (AnchorSide?)AlignmentToAnchorSide(td.Alignment)
				: null;
		}

		private static IToolDock FindToolDockForCollection(IDockable node, object sender)
		{
			if (node is IToolDock toolDock && toolDock.VisibleDockables == sender)
				return toolDock;

			if (node is IDock dock && dock.VisibleDockables != null)
			{
				foreach (var child in dock.VisibleDockables)
				{
					if (child is IDock)
					{
						var found = FindToolDockForCollection(child, sender);
						if (found != null)
							return found;
					}
				}
			}

			return null;
		}

		private static AnchorSide AlignmentToAnchorSide(DockAlignment alignment)
		{
			switch (alignment)
			{
				case DockAlignment.Left: return AnchorSide.Left;
				case DockAlignment.Right: return AnchorSide.Right;
				case DockAlignment.Top: return AnchorSide.Top;
				case DockAlignment.Bottom: return AnchorSide.Bottom;
				default: return AnchorSide.Right;
			}
		}
	}
}