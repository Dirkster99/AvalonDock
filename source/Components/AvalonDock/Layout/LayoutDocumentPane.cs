/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Implements a layout element that contains a collection of <see cref="LayoutDocument"/> objects.
	/// This is the viewmodel for a <see cref="Controls.LayoutDocumentPaneControl"/>.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutDocumentPane : LayoutPositionableGroup<LayoutContent>, ILayoutDocumentPane, ILayoutPositionableElement, ILayoutContentSelector, ILayoutPaneSerializable
	{
		#region fields
		private bool _showHeader = true;
		private int _selectedIndex = -1;
		string _id;

		[XmlIgnore]
		bool _autoFixSelectedContent = true;
		#endregion fields

		#region Constructors
		/// <summary>Standard class constructor</summary>
		public LayoutDocumentPane()
		{
		}

		/// <summary>
		/// Class constructor from <see cref="LayoutContent"/> to be inserted in <see cref="Children"/>
		/// collection of this object.
		/// </summary>
		/// <param name="firstChild"></param>
		public LayoutDocumentPane(LayoutContent firstChild)
		{
			Children.Add(firstChild);
		}

		#endregion Constructors

		#region Properties
		/// <summary>Gets/sets whether to show the header or not.</summary>
		public bool ShowHeader
		{
			get => _showHeader;
			set
			{
				if (value == _showHeader) return;
				_showHeader = value;
				RaisePropertyChanged(nameof(ShowHeader));
			}
		}

		/// <summary>Gets/sets the index of the selected content in the pane.</summary>
		public int SelectedContentIndex
		{
			get => _selectedIndex;
			set
			{
				if (value < 0 || value >= Children.Count) value = -1;
				if (value == _selectedIndex) return;
				RaisePropertyChanging(nameof(SelectedContentIndex));
				RaisePropertyChanging(nameof(SelectedContent));
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count) Children[_selectedIndex].IsSelected = false;
				_selectedIndex = value;
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count) Children[_selectedIndex].IsSelected = true;
				RaisePropertyChanged(nameof(SelectedContentIndex));
				RaisePropertyChanged(nameof(SelectedContent));
			}
		}

		/// <inheritdoc cref="ILayoutContentSelector"/>
		public LayoutContent SelectedContent => _selectedIndex == -1 ? null : Children[_selectedIndex];

		/// <inheritdoc />
		string ILayoutPaneSerializable.Id
		{
			get => _id;
			set => _id = value;
		}

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

		/// <summary>Gets a sorted collection (using the default comparer) of childrens from the Children property.</summary>
		public IEnumerable<LayoutContent> ChildrenSorted
		{
			get
			{
				var listSorted = Children.ToList();
				listSorted.Sort();
				return listSorted;
			}
		}
		#endregion Properties

		#region Overrides
		/// <inheritdoc />
		protected override bool GetVisibility()
		{
			if (Parent is LayoutDocumentPaneGroup)
				return ChildrenCount > 0 && Children.Any(c => c is LayoutDocument && ((LayoutDocument)c).IsVisible || c is LayoutAnchorable);
			return true;
		}

		/// <inheritdoc />
		protected override void ChildMoved(int oldIndex, int newIndex)
		{
			if (_selectedIndex == oldIndex)
			{
				RaisePropertyChanging(nameof(SelectedContentIndex));
				_selectedIndex = newIndex;
				RaisePropertyChanged(nameof(SelectedContentIndex));
			}
			base.ChildMoved(oldIndex, newIndex);
		}

		/// <inheritdoc />
		protected override void OnChildrenCollectionChanged()
		{
			AutoFixSelectedContent();
			for (var i = 0; i < Children.Count; i++)
			{
				if (!Children[i].IsSelected) continue;
				SelectedContentIndex = i;
				break;
			}
			// TODO: MK who's properties are these??
			//RaisePropertyChanged(nameof(CanClose));
			//RaisePropertyChanged(nameof(CanHide));
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			base.OnChildrenCollectionChanged();
			RaisePropertyChanged( nameof(ChildrenSorted));
		}

		private void AutoFixSelectedContent()
		{
			if (!_autoFixSelectedContent) return;
			if (SelectedContentIndex >= ChildrenCount) SelectedContentIndex = Children.Count - 1;
			if (SelectedContentIndex == -1 && ChildrenCount > 0) SetNextSelectedIndex();
		}

		/// <summary>
		/// Gets whether the pane is hosted directly in a floating window (<see cref="LayoutDocumentFloatingWindow"/>).
		/// </summary>
		public bool IsDirectlyHostedInFloatingWindow
		{
			get
			{
				var parentFloatingWindow = this.FindParent<LayoutDocumentFloatingWindow>();
				return parentFloatingWindow != null && parentFloatingWindow.IsSinglePane;
				//return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
			}
		}

		/// <inheritdoc/>
		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			if (_id != null) writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
			if (!_showHeader) writer.WriteAttributeString(nameof(ShowHeader), _showHeader.ToString());
			base.WriteXml(writer);
		}

		/// <inheritdoc/>
		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id))) _id = reader.Value;
			if (reader.MoveToAttribute(nameof(ShowHeader))) _showHeader = bool.Parse(reader.Value);
			base.ReadXml(reader);
		}

#if TRACE
		/// <inheritdoc/>
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("DocumentPane()");

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif
		#endregion Overrides

		#region methods
		/// <summary>Gets the index of the <paramref name="content"/> in the Children collection or -1</summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public int IndexOf(LayoutContent content)
		{
			var documentChild = content as LayoutDocument;
			return documentChild == null ? -1 : Children.IndexOf(documentChild);
		}

		/// <summary>Invalidates the current <see cref="SelectedContentIndex"/> and sets the index for the next avialable child with IsEnabled == true.</summary>
		internal void SetNextSelectedIndex()
		{
			SelectedContentIndex = -1;
			for (var i = 0; i < Children.Count; ++i)
			{
				if (!Children[i].IsEnabled) continue;
				SelectedContentIndex = i;
				return;
			}
		}

		/// <summary>Updates the <see cref="IsDirectlyHostedInFloatingWindow"/> property of this object.</summary>
		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
		
		/// <inheritdoc/>
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			if (oldValue is ILayoutGroup oldGroup) oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if (newValue is ILayoutGroup newGroup) newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;
			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc/>
		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
		#endregion methods
	}
}
