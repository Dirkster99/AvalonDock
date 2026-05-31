using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Default implementation of <see cref="IDockLayoutService"/>.
	/// Builds the layout tree automatically from registered <see cref="IToolbox"/> instances.
	/// </summary>
	public class DockLayoutService : IDockLayoutService
	{
		private readonly DocumentDock _documentDock;
		private readonly ToolDock _toolDock;
		private readonly RootDock _rootDock;

		/// <summary>
		/// Initializes a new instance of the <see cref="DockLayoutService"/> class.
		/// </summary>
		/// <param name="toolboxes">All registered toolbox instances (injected via DI).</param>
		public DockLayoutService(IEnumerable<IToolbox> toolboxes)
		{
			_documentDock = new DocumentDock
			{
				Id = "Documents",
				Title = "Documents",
				VisibleDockables = new ObservableCollection<IDockable>()
			};

			_toolDock = new ToolDock
			{
				Id = "Tools",
				Title = "Tools",
				VisibleDockables = new ObservableCollection<IDockable>(
					toolboxes?.Cast<IDockable>() ?? Enumerable.Empty<IDockable>())
			};

			_rootDock = new RootDock
			{
				Id = "Root",
				Title = "Root",
				VisibleDockables = new ObservableCollection<IDockable> { _documentDock, _toolDock }
			};
		}

		/// <inheritdoc/>
		public IRootDock Layout => _rootDock;

		/// <inheritdoc/>
		public IDockable? ActiveDockable
		{
			get => _rootDock.ActiveDockable;
			set => _rootDock.ActiveDockable = value;
		}

		/// <inheritdoc/>
		public IEnumerable<IDockable> Documents =>
			_documentDock.VisibleDockables ?? Enumerable.Empty<IDockable>();

		/// <inheritdoc/>
		public IEnumerable<IDockable> Anchorables =>
			_toolDock.VisibleDockables ?? Enumerable.Empty<IDockable>();

		/// <inheritdoc/>
		public event EventHandler? AnchorableStateChanged;

		/// <inheritdoc/>
		public void OpenDocument(IDockable document)
		{
			if (_documentDock.VisibleDockables != null
				&& !_documentDock.VisibleDockables.Contains(document))
			{
				document.Owner = _documentDock;
				_documentDock.VisibleDockables.Add(document);
			}

			_rootDock.ActiveDockable = document;
		}

		/// <inheritdoc/>
		public void CloseDocument(IDockable document)
		{
			_documentDock.VisibleDockables?.Remove(document);

			if (_rootDock.ActiveDockable == document)
			{
				_rootDock.ActiveDockable = _documentDock.VisibleDockables?.LastOrDefault();
			}
		}

		/// <inheritdoc/>
		public T? FindDocument<T>(Func<T, bool> predicate)
			where T : class, IDockable
		{
			return _documentDock.VisibleDockables?.OfType<T>().FirstOrDefault(predicate);
		}

		/// <inheritdoc/>
		public T OpenOrActivateDocument<T>(Func<T, bool> predicate, Func<T> factory)
			where T : class, IDockable
		{
			var existing = FindDocument(predicate);
			if (existing != null)
			{
				_rootDock.ActiveDockable = existing;
				return existing;
			}

			var doc = factory();
			OpenDocument(doc);
			return doc;
		}

		/// <inheritdoc/>
		public T? GetAnchorable<T>()
			where T : class, IToolbox
		{
			return _toolDock.VisibleDockables?.OfType<T>().FirstOrDefault();
		}

		/// <inheritdoc/>
		public void ShowAnchorable(IDockable anchorable)
		{
			if (anchorable is IToolbox toolbox && !toolbox.IsOpen)
			{
				toolbox.IsOpen = true;
				AnchorableStateChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <inheritdoc/>
		public void HideAnchorable(IDockable anchorable)
		{
			if (anchorable is IToolbox toolbox && toolbox.IsOpen)
			{
				toolbox.IsOpen = false;
				AnchorableStateChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <inheritdoc/>
		public bool IsAnchorableOpen(IDockable anchorable)
		{
			return anchorable is IToolbox toolbox && toolbox.IsOpen;
		}

		/// <inheritdoc/>
		public bool IsSideOpen(ToolboxSide side)
		{
			return Anchorables.OfType<IToolbox>()
				.Any(t => t.Zone.ToSide() == side && t.IsOpen);
		}
	}
}