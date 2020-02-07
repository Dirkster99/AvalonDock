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
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutDocumentPane : LayoutPositionableGroup<LayoutContent>, ILayoutDocumentPane, ILayoutPositionableElement, ILayoutContentSelector, ILayoutPaneSerializable
	{
		#region fields
		private bool _showHeader = true;
		private int _selectedIndex = -1;
		string _id;
		#endregion fields

		#region Constructors

		public LayoutDocumentPane()
		{
		}

		public LayoutDocumentPane(LayoutContent firstChild)
		{
			Children.Add(firstChild);
		}

		#endregion Constructors

		#region Properties

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

		public LayoutContent SelectedContent => _selectedIndex == -1 ? null : Children[_selectedIndex];

		/// <inheritdoc />
		string ILayoutPaneSerializable.Id
		{
			get => _id;
			set => _id = value;
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

		[XmlIgnore]
		bool _autoFixSelectedContent = true;

		private void AutoFixSelectedContent()
		{
			if (!_autoFixSelectedContent) return;
			if (SelectedContentIndex >= ChildrenCount) SelectedContentIndex = Children.Count - 1;
			if (SelectedContentIndex == -1 && ChildrenCount > 0) SetNextSelectedIndex();
		}

		public bool IsDirectlyHostedInFloatingWindow
		{
			get
			{
				var parentFloatingWindow = this.FindParent<LayoutDocumentFloatingWindow>();
				return parentFloatingWindow != null && parentFloatingWindow.IsSinglePane;
				//return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
			}
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			if (_id != null) writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
			if (!_showHeader) writer.WriteAttributeString(nameof(ShowHeader), _showHeader.ToString());
			base.WriteXml(writer);
		}

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id))) _id = reader.Value;
			if (reader.MoveToAttribute(nameof(ShowHeader))) _showHeader = bool.Parse(reader.Value);
			base.ReadXml(reader);
		}

#if TRACE
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("DocumentPane()");

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides

		#region Public Methods

		public int IndexOf(LayoutContent content)
		{
			var documentChild = content as LayoutDocument;
			return documentChild == null ? -1 : Children.IndexOf(documentChild);
		}

		#endregion Public Methods

		#region Internal Methods

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

		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Internal Methods

		#region Private Methods

		public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

		public IEnumerable<LayoutContent> ChildrenSorted
		{
			get
			{
				var listSorted = Children.ToList();
				listSorted.Sort();
				return listSorted;
			}
		}
		
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			if (oldValue is ILayoutGroup oldGroup) oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if (newValue is ILayoutGroup newGroup) newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;
			base.OnParentChanged(oldValue, newValue);
		}

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods
	}
}
