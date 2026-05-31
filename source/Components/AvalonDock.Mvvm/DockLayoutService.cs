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
		private readonly Dictionary<ToolboxSide, List<IToolbox>> _lastOpenBySide = new Dictionary<ToolboxSide, List<IToolbox>>();

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

		/// <inheritdoc/>
		public void ToggleSide(ToolboxSide side)
		{
			var toolboxes = Anchorables
				.OfType<IToolbox>()
				.Where(t => t.Zone.ToSide() == side)
				.ToList();

			if (toolboxes.Count == 0)
			{
				return;
			}

			bool anyOpen = toolboxes.Any(t => t.IsOpen);

			if (anyOpen)
			{
				_lastOpenBySide[side] = toolboxes.Where(t => t.IsOpen).ToList();

				foreach (var t in toolboxes)
				{
					t.IsOpen = false;
				}
			}
			else
			{
				if (_lastOpenBySide.TryGetValue(side, out var lastOpen) && lastOpen.Count > 0)
				{
					foreach (var t in lastOpen)
					{
						t.IsOpen = true;
					}
				}
				else
				{
					toolboxes[0].IsOpen = true;
				}
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