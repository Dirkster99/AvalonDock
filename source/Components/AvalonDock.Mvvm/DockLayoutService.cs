using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Default implementation of <see cref="IDockLayoutService"/>.
	/// Builds the layout tree automatically from registered <see cref="IToolbox"/> instances
	/// and raises <see cref="AnchorableStateChanged"/> whenever any toolbox's
	/// <see cref="IToolbox.IsOpen"/> property changes.
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

			var toolboxList = toolboxes?.Cast<IDockable>() ?? Enumerable.Empty<IDockable>();

			_toolDock = new ToolDock
			{
				Id = "Tools",
				Title = "Tools",
				VisibleDockables = new ObservableCollection<IDockable>(toolboxList)
			};

			_rootDock = new RootDock
			{
				Id = "Root",
				Title = "Root",
				VisibleDockables = new ObservableCollection<IDockable> { _documentDock, _toolDock }
			};

			foreach (var dockable in _toolDock.VisibleDockables)
			{
				if (dockable is INotifyPropertyChanged npc)
				{
					npc.PropertyChanged += OnToolboxPropertyChanged;
				}
			}
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

		private void OnToolboxPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(IToolbox.IsOpen))
			{
				AnchorableStateChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}