/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout root.
	/// </summary>
	[ContentProperty(nameof(RootPanel))]
	[Serializable]
	public class LayoutRoot : LayoutElement, ILayoutContainer, ILayoutRoot, IXmlSerializable, Core.Serialization.ISerializableLayoutRoot
	{
		private LayoutPanel _rootPanel;
		private LayoutAnchorSide _topSide = null;
		private LayoutAnchorSide _rightSide;
		private LayoutAnchorSide _leftSide = null;
		private LayoutAnchorSide _bottomSide = null;

		private ObservableCollection<LayoutFloatingWindow> _floatingWindows = null;
		private ObservableCollection<LayoutAnchorable> _hiddenAnchorables = null;

		[field: NonSerialized]
		private WeakReference _activeContent = null;

		private bool _activeContentSet = false;

		[field: NonSerialized]
		private WeakReference _lastFocusedDocument = null;

		[NonSerialized]
		private DockingManager _manager = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutRoot"/> class.
		/// </summary>
		public LayoutRoot()
		{
			RightSide = new LayoutAnchorSide();
			LeftSide = new LayoutAnchorSide();
			TopSide = new LayoutAnchorSide();
			BottomSide = new LayoutAnchorSide();
			RootPanel = new LayoutPanel(new LayoutDocumentPane());
		}

		/// <summary>
		/// Occurs when the updated event is raised.
		/// </summary>
		public event EventHandler Updated;

		/// <summary>
		/// Occurs when the element added event is raised.
		/// </summary>
		public event EventHandler<LayoutElementEventArgs> ElementAdded;

		/// <summary>
		/// Occurs when the element removed event is raised.
		/// </summary>
		public event EventHandler<LayoutElementEventArgs> ElementRemoved;

		/// <summary>
		/// Gets or sets the root panel.
		/// </summary>
		public LayoutPanel RootPanel
		{
			get => _rootPanel;
			set
			{
				if (_rootPanel == value) return;
				RaisePropertyChanging(nameof(RootPanel));
				var activeContent = ActiveContent;
				var activeRoot = activeContent?.Root;
				if (_rootPanel != null && _rootPanel.Parent == this) _rootPanel.Parent = null;
				_rootPanel = value ?? new LayoutPanel(new LayoutDocumentPane());
				_rootPanel.Parent = this;
				if (ActiveContent == null && activeRoot == this && activeContent != null)
				{
					ActiveContent = activeContent;
					if (ActiveContent != activeContent)
					{
						ActiveContent = activeContent;
					}
				}

				RaisePropertyChanged(nameof(RootPanel));
			}
		}

		/// <summary>
		/// Gets or sets the top side.
		/// </summary>
		public LayoutAnchorSide TopSide
		{
			get => _topSide;
			set
			{
				if (_topSide == value) return;
				RaisePropertyChanging(nameof(TopSide));
				_topSide = value;
				if (_topSide != null) _topSide.Parent = this;
				RaisePropertyChanged(nameof(TopSide));
			}
		}

		/// <summary>
		/// Gets or sets the right side.
		/// </summary>
		public LayoutAnchorSide RightSide
		{
			get => _rightSide;
			set
			{
				if (_rightSide == value) return;
				RaisePropertyChanging(nameof(RightSide));
				_rightSide = value;
				if (_rightSide != null) _rightSide.Parent = this;
				RaisePropertyChanged(nameof(RightSide));
			}
		}

		/// <summary>
		/// Gets or sets the left side.
		/// </summary>
		public LayoutAnchorSide LeftSide
		{
			get => _leftSide;
			set
			{
				if (value == _leftSide) return;
				RaisePropertyChanging(nameof(LeftSide));
				_leftSide = value;
				if (_leftSide != null) _leftSide.Parent = this;
				RaisePropertyChanged(nameof(LeftSide));
			}
		}

		/// <summary>
		/// Gets or sets the bottom side.
		/// </summary>
		public LayoutAnchorSide BottomSide
		{
			get => _bottomSide;
			set
			{
				if (value == _bottomSide) return;
				RaisePropertyChanging(nameof(BottomSide));
				_bottomSide = value;
				if (_bottomSide != null) _bottomSide.Parent = this;
				RaisePropertyChanged(nameof(BottomSide));
			}
		}

		/// <summary>
		/// Gets the floating windows.
		/// </summary>
		public ObservableCollection<LayoutFloatingWindow> FloatingWindows
		{
			get
			{
				if (_floatingWindows == null)
				{
					_floatingWindows = new ObservableCollection<LayoutFloatingWindow>();
					_floatingWindows.CollectionChanged += _floatingWindows_CollectionChanged;
				}

				return _floatingWindows;
			}
		}

		/// <summary>
		/// Gets the hidden.
		/// </summary>
		public ObservableCollection<LayoutAnchorable> Hidden
		{
			get
			{
				if (_hiddenAnchorables == null)
				{
					_hiddenAnchorables = new ObservableCollection<LayoutAnchorable>();
					_hiddenAnchorables.CollectionChanged += _hiddenAnchorables_CollectionChanged;
				}

				return _hiddenAnchorables;
			}
		}

		/// <summary>
		/// Gets the children.
		/// </summary>
		public IEnumerable<ILayoutElement> Children
		{
			get
			{
				if (RootPanel != null)
					yield return RootPanel;
				if (_floatingWindows != null)
				{
					foreach (var floatingWindow in _floatingWindows)
						yield return floatingWindow;
				}

				if (TopSide != null)
					yield return TopSide;
				if (RightSide != null)
					yield return RightSide;
				if (BottomSide != null)
					yield return BottomSide;
				if (LeftSide != null)
					yield return LeftSide;
				if (_hiddenAnchorables != null)
				{
					foreach (var hiddenAnchorable in _hiddenAnchorables)
						yield return hiddenAnchorable;
				}
			}
		}

		/// <summary>
		/// Gets the children count.
		/// </summary>
		public int ChildrenCount => 5 + (_floatingWindows?.Count ?? 0) + (_hiddenAnchorables?.Count ?? 0);

		/// <summary>
		/// Gets or sets the active content.
		/// </summary>
		[XmlIgnore]
		public LayoutContent ActiveContent
		{
			get
			{
				return _activeContent.GetValueOrDefault<LayoutContent>();
			}
			set
			{
				var currentValue = ActiveContent;
				if (currentValue != value)
				{
					InternalSetActiveContent(currentValue, value);
				}
			}
		}

		/// <summary>
		/// Gets the last focused document.
		/// </summary>
		[XmlIgnore]
		public LayoutContent LastFocusedDocument
		{
			get => _lastFocusedDocument.GetValueOrDefault<LayoutContent>();
			private set
			{
				var currentValue = LastFocusedDocument;
				if (currentValue != value)
				{
					RaisePropertyChanging(nameof(LastFocusedDocument));
					if (currentValue != null) currentValue.IsLastFocusedDocument = false;
					_lastFocusedDocument = new WeakReference(value);
					currentValue = LastFocusedDocument;
					if (currentValue != null) currentValue.IsLastFocusedDocument = true;
					RaisePropertyChanged(nameof(LastFocusedDocument));
				}
			}
		}

		/// <summary>
		/// Gets the manager.
		/// </summary>
		[XmlIgnore]
		public DockingManager Manager
		{
			get => _manager;
			internal set
			{
				if (value == _manager) return;
				RaisePropertyChanging(nameof(Manager));
				_manager = value;
				RaisePropertyChanged(nameof(Manager));
			}
		}
#if TRACE
		/// <inheritdoc/>
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("RootPanel()");

			RootPanel.ConsoleDump(tab + 1);

			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("FloatingWindows()");

			foreach (var fw in FloatingWindows)
				fw.ConsoleDump(tab + 1);

			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("Hidden()");

			foreach (var hidden in Hidden)
				hidden.ConsoleDump(tab + 1);
		}
#endif

		/// <summary>
		/// Removes the child.
		/// </summary>
		/// <param name="element">The layout element.</param>
		public void RemoveChild(ILayoutElement element)
		{
			if (element == RootPanel)
				RootPanel = null;
			else if (_floatingWindows != null && _floatingWindows.Contains(element))
				_floatingWindows.Remove(element as LayoutFloatingWindow);
			else if (_hiddenAnchorables != null && _hiddenAnchorables.Contains(element))
				_hiddenAnchorables.Remove(element as LayoutAnchorable);
			else if (element == TopSide)
				TopSide = null;
			else if (element == RightSide)
				RightSide = null;
			else if (element == BottomSide)
				BottomSide = null;
			else if (element == LeftSide)
				LeftSide = null;
		}

		/// <summary>
		/// Replaces the child.
		/// </summary>
		/// <param name="oldElement">The existing layout element.</param>
		/// <param name="newElement">The replacement layout element.</param>
		public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			if (oldElement == RootPanel)
			{
				RootPanel = (LayoutPanel)newElement;
			}
			else if (_floatingWindows != null && _floatingWindows.Contains(oldElement))
			{
				var index = _floatingWindows.IndexOf(oldElement as LayoutFloatingWindow);
				_floatingWindows.Remove(oldElement as LayoutFloatingWindow);
				_floatingWindows.Insert(index, newElement as LayoutFloatingWindow);
			}
			else if (_hiddenAnchorables != null && _hiddenAnchorables.Contains(oldElement))
			{
				var index = _hiddenAnchorables.IndexOf(oldElement as LayoutAnchorable);
				_hiddenAnchorables.Remove(oldElement as LayoutAnchorable);
				_hiddenAnchorables.Insert(index, newElement as LayoutAnchorable);
			}
			else if (oldElement == TopSide)
			{
				TopSide = (LayoutAnchorSide)newElement;
			}
			else if (oldElement == RightSide)
			{
				RightSide = (LayoutAnchorSide)newElement;
			}
			else if (oldElement == BottomSide)
			{
				BottomSide = (LayoutAnchorSide)newElement;
			}
			else if (oldElement == LeftSide)
			{
				LeftSide = (LayoutAnchorSide)newElement;
			}
		}

		/// <summary>
		/// Collects the garbage.
		/// </summary>
		public void CollectGarbage()
		{
			var exitFlag = true;

			do
			{
				exitFlag = true;

				// for each content that references via PreviousContainer a disconnected Pane set the property to null
				foreach (var content in this.Descendents().OfType<ILayoutPreviousContainer>().Where(c => c.PreviousContainer != null &&
					(c.PreviousContainer.Parent == null || c.PreviousContainer.Parent.Root != this)))
				{
					content.PreviousContainer = null;
				}

				// for each pane that is empty
				foreach (var emptyPane in this.Descendents().OfType<ILayoutPane>().Where(p => p.ChildrenCount == 0))
				{
					// ...set null any reference coming from contents not yet hosted in a floating window
					foreach (var contentReferencingEmptyPane in this.Descendents().OfType<LayoutContent>()
						.Where(c => ((ILayoutPreviousContainer)c).PreviousContainer == emptyPane))
					{
						if (contentReferencingEmptyPane is LayoutAnchorable anchorable &&
							!anchorable.IsVisible)
							continue;

						((ILayoutPreviousContainer)contentReferencingEmptyPane).PreviousContainer = null;
						contentReferencingEmptyPane.PreviousContainerIndex = -1;
					}

					// ...if this pane is the only documentpane present in the layout of the main window (not floating) then skip it
					if (emptyPane is LayoutDocumentPane &&
						 emptyPane.FindParent<LayoutDocumentFloatingWindow>() == null &&
						 this.Descendents().OfType<LayoutDocumentPane>().Count(c => c != emptyPane && c.FindParent<LayoutDocumentFloatingWindow>() == null) == 0)
						continue;

					// ...if this empty pane is not referenced by anyone, then remove it from its parent container
					if (!this.Descendents().OfType<ILayoutPreviousContainer>().Any(c => c.PreviousContainer == emptyPane))
					{
						var parentGroup = emptyPane.Parent;
						parentGroup.RemoveChild(emptyPane);
						exitFlag = false;
						break;
					}
				}

				if (!exitFlag)
				{
					// removes any empty anchorable pane group
					foreach (var emptyLayoutAnchorablePaneGroup in this.Descendents().OfType<LayoutAnchorablePaneGroup>().Where(p => p.ChildrenCount == 0))
					{
						var parentGroup = emptyLayoutAnchorablePaneGroup.Parent;
						parentGroup.RemoveChild(emptyLayoutAnchorablePaneGroup);
						exitFlag = false;
						break;
					}
				}

				if (!exitFlag)
				{
					// removes any empty layout panel
					foreach (var emptyLayoutPanel in this.Descendents().OfType<LayoutPanel>().Where(p => p.ChildrenCount == 0))
					{
						var parentGroup = emptyLayoutPanel.Parent;
						parentGroup.RemoveChild(emptyLayoutPanel);
						exitFlag = false;
						break;
					}

					foreach (var emptyLayoutDocumentPane in this.Descendents().OfType<LayoutDocumentPane>().Where(p => p.ChildrenCount == 0))
					{
						var parentGroup = emptyLayoutDocumentPane.Parent;
						if (!(parentGroup.Parent is LayoutDocumentFloatingWindow)) continue;
						var index = RootPanel.IndexOfChild(this.Descendents().OfType<LayoutDocumentPaneGroup>().First());
						parentGroup.RemoveChild(emptyLayoutDocumentPane);
						if (!this.Descendents().OfType<LayoutDocumentPane>().Any())
						{
							// Now the last Pane container is deleted, at least one is required for documents to be added.
							// We did not want to keep an empty window floating, but add a new one to the main window
							RootPanel.Children.Insert(index < 0 ? 0 : index, emptyLayoutDocumentPane);
						}

						exitFlag = false;
						break;
					}
				}

				if (!exitFlag)
				{
					// removes any empty floating window
					foreach (var emptyLayoutFloatingWindow in this.Descendents().OfType<LayoutFloatingWindow>().Where(p => p.ChildrenCount == 0))
					{
						var parentGroup = emptyLayoutFloatingWindow.Parent;
						parentGroup.RemoveChild(emptyLayoutFloatingWindow);
						exitFlag = false;
						break;
					}
				}

				if (!exitFlag)
				{
					// removes any empty anchor group
					foreach (var emptyLayoutAnchorGroup in this.Descendents().OfType<LayoutAnchorGroup>().Where(p => p.ChildrenCount == 0))
					{
						if (!this.Descendents().OfType<ILayoutPreviousContainer>().Any(c => c.PreviousContainer == emptyLayoutAnchorGroup))
						{
							var parentGroup = emptyLayoutAnchorGroup.Parent;
							parentGroup.RemoveChild(emptyLayoutAnchorGroup);
							exitFlag = false;
							break;
						}
					}
				}
			}
			while (!exitFlag);

			do
			{
				exitFlag = true;
				// for each pane that is empty
				foreach (var paneGroupToCollapse in this.Descendents().OfType<LayoutAnchorablePaneGroup>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutAnchorablePaneGroup).ToArray())
				{
					var singleChild = paneGroupToCollapse.Children[0] as LayoutAnchorablePaneGroup;
					paneGroupToCollapse.Orientation = singleChild.Orientation;
					while (singleChild.ChildrenCount > 0)
						paneGroupToCollapse.InsertChildAt(paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
					paneGroupToCollapse.RemoveChild(singleChild);
					exitFlag = false;
					break;
				}
			}
			while (!exitFlag);

			do
			{
				exitFlag = true;
				// for each pane that is empty
				foreach (var paneGroupToCollapse in this.Descendents().OfType<LayoutDocumentPaneGroup>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutDocumentPaneGroup).ToArray())
				{
					var singleChild = paneGroupToCollapse.Children[0] as LayoutDocumentPaneGroup;
					paneGroupToCollapse.Orientation = singleChild.Orientation;
					while (singleChild.ChildrenCount > 0)
						paneGroupToCollapse.InsertChildAt(paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
					paneGroupToCollapse.RemoveChild(singleChild);
					exitFlag = false;
					break;
				}
			}
			while (!exitFlag);

			////do
			////{
			////  exitFlag = true;
			////  //for each panel that has only one child
			////  foreach( var panelToCollapse in this.Descendents().OfType<LayoutPanel>().Where( p => p.ChildrenCount == 1 && p.Children[ 0 ] is LayoutPanel ).ToArray() )
			////  {
			////    var singleChild = panelToCollapse.Children[ 0 ] as LayoutPanel;
			////    panelToCollapse.Orientation = singleChild.Orientation;
			////    panelToCollapse.RemoveChild( singleChild );
			////    ILayoutPanelElement[] singleChildChildren = new ILayoutPanelElement[ singleChild.ChildrenCount ];
			////    singleChild.Children.CopyTo( singleChildChildren, 0 );
			////    while( singleChild.ChildrenCount > 0 )
			////    {
			////      panelToCollapse.InsertChildAt(
			////          panelToCollapse.ChildrenCount, singleChildChildren[ panelToCollapse.ChildrenCount ] );
			////    }

			////    exitFlag = false;
			////    break;
			////  }
			////}
			////while( !exitFlag );

			// Update ActiveContent and LastFocusedDocument properties
			UpdateActiveContentProperty();

#if DEBUG
			Debug.Assert(!this.Descendents().OfType<LayoutAnchorablePane>().Any(a => a.ChildrenCount == 0 && a.IsVisible));
			// DumpTree(true);
#if TRACE
			RootPanel.ConsoleDump(4);
#endif
#endif
		}

		/// <inheritdoc/>
		XmlSchema IXmlSerializable.GetSchema() => null;

		/// <inheritdoc/>
		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			if (reader.IsEmptyElement)
			{
				reader.Read();
				return;
			}

			var layoutPanelElements = ReadRootPanel(reader, out var orientation, out var canDock);
			if (layoutPanelElements != null)
			{
				RootPanel = new LayoutPanel { Orientation = orientation, CanDock = canDock };
				// Add all children to RootPanel
				foreach (var panel in layoutPanelElements) RootPanel.Children.Add(panel);
			}

			TopSide = new LayoutAnchorSide();
			if (ReadElement(reader) != null) FillLayoutAnchorSide(reader, TopSide);
			RightSide = new LayoutAnchorSide();
			if (ReadElement(reader) != null) FillLayoutAnchorSide(reader, RightSide);
			LeftSide = new LayoutAnchorSide();
			if (ReadElement(reader) != null) FillLayoutAnchorSide(reader, LeftSide);
			BottomSide = new LayoutAnchorSide();
			if (ReadElement(reader) != null) FillLayoutAnchorSide(reader, BottomSide);

			FloatingWindows.Clear();
			var floatingWindows = ReadElementList(reader, true);
			foreach (var floatingWindow in floatingWindows) FloatingWindows.Add((LayoutFloatingWindow)floatingWindow);

			Hidden.Clear();
			var hidden = ReadElementList(reader, false);
			foreach (var hiddenObject in hidden) Hidden.Add((LayoutAnchorable)hiddenObject);

			// Read the closing end element of LayoutRoot
			reader.ReadEndElement();
		}

		/// <inheritdoc/>
		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(RootPanel));
			RootPanel?.WriteXml(writer);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(TopSide));
			TopSide?.WriteXml(writer);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(RightSide));
			RightSide?.WriteXml(writer);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(LeftSide));
			LeftSide?.WriteXml(writer);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(BottomSide));
			BottomSide?.WriteXml(writer);
			writer.WriteEndElement();

			// Write all floating windows (can be LayoutDocumentFloatingWindow or LayoutAnchorableFloatingWindow).
			// To prevent "can not create instance of abstract type", the type is retrieved with GetType().Name
			writer.WriteStartElement(nameof(FloatingWindows));
			foreach (var layoutFloatingWindow in FloatingWindows)
			{
				writer.WriteStartElement(layoutFloatingWindow.GetType().Name);
				layoutFloatingWindow.WriteXml(writer);
				writer.WriteEndElement();
			}

			writer.WriteEndElement();

			writer.WriteStartElement(nameof(Hidden));
			foreach (var layoutAnchorable in Hidden)
			{
				writer.WriteStartElement(layoutAnchorable.GetType().Name);
				layoutAnchorable.WriteXml(writer);
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		/// <summary>
		/// Finds the type.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The resulting value.</returns>
		internal static Type FindType(string name)
		{
			var avalonAssembly = Assembly.GetAssembly(typeof(LayoutRoot));

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a != avalonAssembly))
			{
				try
				{
					foreach (var type in assembly.GetTypes())
					{
						if (type.Name.Equals(name))
							return type;
					}
				}
				catch (ReflectionTypeLoadException)
				{
				}
			}

			return null;
		}

		/// <summary>
		/// Executes the fire layout updated operation.
		/// </summary>
		internal void FireLayoutUpdated() => Updated?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Executes the on layout element added operation.
		/// </summary>
		/// <param name="element">The layout element.</param>
		internal void OnLayoutElementAdded(LayoutElement element) => ElementAdded?.Invoke(this, new LayoutElementEventArgs(element));

		/// <summary>
		/// Executes the on layout element removed operation.
		/// </summary>
		/// <param name="element">The layout element.</param>
		internal void OnLayoutElementRemoved(LayoutElement element)
		{
			if (element.Descendents().OfType<LayoutContent>().Any(c => c == LastFocusedDocument))
				LastFocusedDocument = null;
			if (element.Descendents().OfType<LayoutContent>().Any(c => c == ActiveContent))
				ActiveContent = null;
			ElementRemoved?.Invoke(this, new LayoutElementEventArgs(element));
		}

		/// <summary>
		/// Executes the floating windows collection changed operation.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void _floatingWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var bNotifyChildren = false;

			if (e.OldItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
			{
				foreach (LayoutFloatingWindow element in e.OldItems)
				{
					if (element.Parent != this) continue;
					element.Parent = null;
					bNotifyChildren = true;
				}
			}

			if (e.NewItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
			{
				foreach (LayoutFloatingWindow element in e.NewItems)
				{
					element.Parent = this;
					bNotifyChildren = true;
				}
			}

			// descendants of LayoutElement notify when their Children and ChildrenCount properties change
			// https://github.com/xceedsoftware/wpftoolkit/issues/1313
			if (!bNotifyChildren) return;
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					RaisePropertyChanged(nameof(Children));
					RaisePropertyChanged(nameof(ChildrenCount));
					break;

				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					RaisePropertyChanged(nameof(Children));
					break;
			}
		}

		/// <summary>
		/// Executes the hidden anchorables collection changed operation.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void _hiddenAnchorables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var bNotifyChildren = false;

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				if (e.OldItems != null)
				{
					foreach (LayoutAnchorable element in e.OldItems)
					{
						if (element.Parent != this) continue;
						element.Parent = null;
						bNotifyChildren = true;
					}
				}
			}

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				if (e.NewItems != null)
				{
					foreach (LayoutAnchorable element in e.NewItems)
					{
						if (element.Parent == this) continue;
						element.Parent?.RemoveChild(element);
						element.Parent = this;
						bNotifyChildren = true;
					}
				}
			}

			// descendants of LayoutElement notify when their Children and ChildrenCount properties change
			// https://github.com/xceedsoftware/wpftoolkit/issues/1313
			if (!bNotifyChildren) return;
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					RaisePropertyChanged(nameof(Children));
					RaisePropertyChanged(nameof(ChildrenCount));
					break;

				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					RaisePropertyChanged(nameof(Children));
					break;
			}
		}

		/// <summary>
		/// Executes the internal set active content operation.
		/// </summary>
		/// <param name="currentValue">The current value.</param>
		/// <param name="newActiveContent">The new active content.</param>
		private void InternalSetActiveContent(LayoutContent currentValue, LayoutContent newActiveContent)
		{
			RaisePropertyChanging(nameof(ActiveContent));
			if (currentValue != null && currentValue.IsActive) currentValue.IsActive = false;
			_activeContent = new WeakReference(newActiveContent);
			currentValue = ActiveContent;
			if (currentValue != null && !currentValue.IsActive) currentValue.IsActive = true;
			RaisePropertyChanged(nameof(ActiveContent));
			_activeContentSet = currentValue != null;
			if (currentValue != null)
			{
				if (currentValue.Parent is LayoutDocumentPane || currentValue is LayoutDocument)
					LastFocusedDocument = currentValue;
			}
			else
			{
				LastFocusedDocument = null;
			}
		}

		/// <summary>
		/// Updates the active content property.
		/// </summary>
		private void UpdateActiveContentProperty()
		{
			var activeContent = ActiveContent;
			if (_activeContentSet && (activeContent == null || activeContent.Root != this))
			{
				_activeContentSet = false;
				InternalSetActiveContent(activeContent, null);
			}
		}

		/// <summary>
		/// Executes the fill layout anchor side operation.
		/// </summary>
		/// <param name="reader">The XML reader to read from.</param>
		/// <param name="layoutAnchorSide">The layout anchor side.</param>
		private void FillLayoutAnchorSide(XmlReader reader, LayoutAnchorSide layoutAnchorSide)
		{
			var result = new List<LayoutAnchorGroup>();

			while (true)
			{
				// Read all layoutAnchorSide children
				if (ReadElement(reader) is LayoutAnchorGroup element) result.Add(element);
				else if (reader.NodeType == XmlNodeType.EndElement) break;
			}

			reader.ReadEndElement();
			foreach (var las in result)
			{
				layoutAnchorSide.Children.Add(las);
			}
		}

		/// <summary>
		/// Reads the root panel.
		/// </summary>
		/// <param name="reader">The XML reader to read from.</param>
		/// <param name="orientation">The orientation.</param>
		/// <param name="canDock">The can dock.</param>
		/// <returns>The resulting value.</returns>
		private List<ILayoutPanelElement> ReadRootPanel(
			XmlReader reader,
			out Orientation orientation,
			out bool canDock)
		{
			orientation = Orientation.Horizontal;
			canDock = true;

			var result = new List<ILayoutPanelElement>();
			var startElementName = reader.LocalName;
			reader.Read();
			if (reader.LocalName.Equals(startElementName) && reader.NodeType == XmlNodeType.EndElement) return null;

			while (reader.NodeType == XmlNodeType.Whitespace) reader.Read();

			if (reader.LocalName.Equals(nameof(RootPanel)))
			{
				orientation = (Orientation)Enum.Parse(typeof(Orientation), reader.GetAttribute(nameof(Orientation)), true);

				var canDockStr = reader.GetAttribute("CanDock");
				if (canDockStr != null)
					canDock = bool.Parse(canDockStr);

				reader.Read();
				while (true)
				{
					// Read all RootPanel children
					if (ReadElement(reader) is ILayoutPanelElement element) result.Add(element);
					else if (reader.NodeType == XmlNodeType.EndElement) break;
				}
			}

			reader.ReadEndElement();
			return result;
		}

		/// <summary>
		/// Reads the element list.
		/// </summary>
		/// <param name="reader">The XML reader to read from.</param>
		/// <param name="isFloatingWindow">The is floating window.</param>
		/// <returns>The resulting value.</returns>
		private List<object> ReadElementList(XmlReader reader, bool isFloatingWindow)
		{
			var resultList = new List<object>();
			while (reader.NodeType == XmlNodeType.Whitespace) reader.Read();
			if (reader.NodeType == XmlNodeType.EndElement) return resultList;

			if (reader.IsEmptyElement)
			{
				reader.Read();
				return resultList;
			}

			var startElementName = reader.LocalName;
			reader.Read();
			if (reader.LocalName.Equals(startElementName) && reader.NodeType == XmlNodeType.EndElement) return null;

			while (reader.NodeType == XmlNodeType.Whitespace) reader.Read();

			while (true)
			{
				if (isFloatingWindow)
				{
					if (!(ReadElement(reader) is LayoutFloatingWindow result)) break;
					resultList.Add(result);
				}
				else
				{
					if (!(ReadElement(reader) is LayoutAnchorable result)) break;
					resultList.Add(result);
				}
			}

			reader.ReadEndElement();
			return resultList;
		}

		/// <summary>
		/// Reads the element.
		/// </summary>
		/// <param name="reader">The XML reader to read from.</param>
		/// <returns>The resulting value.</returns>
		private object ReadElement(XmlReader reader)
		{
			while (reader.NodeType == XmlNodeType.Whitespace) reader.Read();
			if (reader.NodeType == XmlNodeType.EndElement) return null;

			Type typeToSerialize;
			switch (reader.LocalName)
			{
				case nameof(LayoutAnchorablePaneGroup):
					typeToSerialize = typeof(LayoutAnchorablePaneGroup);
					break;

				case nameof(LayoutAnchorablePane):
					typeToSerialize = typeof(LayoutAnchorablePane);
					break;

				case nameof(LayoutAnchorable):
					typeToSerialize = typeof(LayoutAnchorable);
					break;

				case nameof(LayoutDocumentPaneGroup):
					typeToSerialize = typeof(LayoutDocumentPaneGroup);
					break;

				case nameof(LayoutDocumentPane):
					typeToSerialize = typeof(LayoutDocumentPane);
					break;

				case nameof(LayoutDocument):
					typeToSerialize = typeof(LayoutDocument);
					break;

				case nameof(LayoutAnchorGroup):
					typeToSerialize = typeof(LayoutAnchorGroup);
					break;

				case nameof(LayoutPanel):
					typeToSerialize = typeof(LayoutPanel);
					break;

				case nameof(LayoutDocumentFloatingWindow):
					typeToSerialize = typeof(LayoutDocumentFloatingWindow);
					break;

				case nameof(LayoutAnchorableFloatingWindow):
					typeToSerialize = typeof(LayoutAnchorableFloatingWindow);
					break;

				case nameof(LeftSide):
				case nameof(RightSide):
				case nameof(TopSide):
				case nameof(BottomSide):
					if (reader.IsEmptyElement)
					{
						reader.Read();
						return null;
					}

					return reader.Read();

				default:
					typeToSerialize = FindType(reader.LocalName);
					if (typeToSerialize == null)
						throw new ArgumentException("AvalonDock.LayoutRoot doesn't know how to deserialize " + reader.LocalName);
					break;
			}

			XmlSerializer serializer = XmlSerializersCache.GetSerializer(typeToSerialize);
			return serializer.Deserialize(reader);
		}
#if DEBUG

		/// <summary>
		/// Dumps the current layout tree to the debug output.
		/// </summary>
		/// <param name="shortPropertyNames">If set to <see langword="true"/>, uses abbreviated property labels.</param>
		public void DumpTree(bool shortPropertyNames = false)
		{
			void DumpElement(ILayoutElement element, StringBuilder indent, int childID, bool isLastChild)
			{
				Debug.Write($"{indent}{(indent.Length > 0 ? isLastChild ? " └─ " : " ├─ " : string.Empty)}{childID:D2} 0x{element.GetHashCode():X8} " +
								$"{element.GetType().Name} {(shortPropertyNames ? "P" : "Parent")}:0x{element.Parent?.GetHashCode() ?? 0:X8} " +
								$"{(shortPropertyNames ? "R" : "Root")}:0x{element.Root?.GetHashCode() ?? 0:X8}");
				if (!(element is ILayoutContainer containerElement))
				{
					Debug.WriteLine(string.Empty);
					return;
				}

				Debug.WriteLine($" {(shortPropertyNames ? "C" : "Children")}:{containerElement.ChildrenCount}");
				var nrChild = 0;
				indent.Append(isLastChild ? "   " : " │ ");
				foreach (var child in containerElement.Children)
				{
					var lastChild = nrChild == containerElement.ChildrenCount - 1;
					DumpElement(child, indent, nrChild++, lastChild);
				}

				indent.Remove(indent.Length - 3, 3);
			}

			DumpElement(this, new StringBuilder(), 0, true);
		}

#endif

		/// <inheritdoc/>
		IEnumerable<Core.Serialization.ISerializableLayoutElement> Core.Serialization.ISerializableLayoutRoot.Descendents()
			=> ((ILayoutElement)this).Descendents().OfType<Core.Serialization.ISerializableLayoutElement>();
	}
}