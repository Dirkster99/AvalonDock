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
		private readonly List<ToolDock> _toolDocks = new List<ToolDock>();
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

			var rootChildren = new ObservableCollection<IDockable> { _documentDock };

			// Group toolboxes by alignment so LayoutSyncBridge places them on the correct side
			var toolboxList = toolboxes?.ToList() ?? new List<IToolbox>();
			var groups = toolboxList.GroupBy(t => ZoneToAlignment(t.Zone));
			foreach (var group in groups)
			{
				var toolDock = new ToolDock
				{
					Id = $"Tools_{group.Key}",
					Title = $"Tools ({group.Key})",
					Alignment = group.Key,
					VisibleDockables = new ObservableCollection<IDockable>(group.Cast<IDockable>())
				};
				_toolDocks.Add(toolDock);
				rootChildren.Add(toolDock);
			}

			_rootDock = new RootDock
			{
				Id = "Root",
				Title = "Root",
				VisibleDockables = rootChildren
			};

			foreach (var toolDock in _toolDocks)
			{
				foreach (var dockable in toolDock.VisibleDockables!)
				{
					if (dockable is INotifyPropertyChanged npc)
					{
						npc.PropertyChanged += OnToolboxPropertyChanged;
					}
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
			_toolDocks.SelectMany(td => td.VisibleDockables ?? Enumerable.Empty<IDockable>());

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

		private static DockAlignment ZoneToAlignment(DockZone zone)
		{
			switch (zone)
			{
				case DockZone.LeftTop:
				case DockZone.LeftBottom:
					return DockAlignment.Left;
				case DockZone.RightTop:
				case DockZone.RightBottom:
					return DockAlignment.Right;
				case DockZone.BottomLeft:
				case DockZone.BottomRight:
					return DockAlignment.Bottom;
				default:
					return DockAlignment.Left;
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