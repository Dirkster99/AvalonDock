/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorablePane : LayoutPositionableGroup<LayoutAnchorable>, ILayoutAnchorablePane, ILayoutPositionableElement, ILayoutContentSelector, ILayoutPaneSerializable
	{
		#region fields
		private int _selectedIndex = -1;

		[XmlIgnore]
		private bool _autoFixSelectedContent = true;
		private string _name = null;
		#endregion fields

		#region Constructors

		public LayoutAnchorablePane()
		{
		}

		public LayoutAnchorablePane(LayoutAnchorable anchorable)
		{
			Children.Add(anchorable);
		}

		#endregion Constructors

		#region Properties

		public bool CanHide => Children.All(a => a.CanHide);

		public bool CanClose => Children.All(a => a.CanClose);

		public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

		public string Name
		{
			get => _name;
			set
			{
				if (value == _name) return;
				_name = value;
				RaisePropertyChanged(nameof(Name));
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
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = false;
				_selectedIndex = value;
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = true;
				RaisePropertyChanged(nameof(SelectedContentIndex));
				RaisePropertyChanged(nameof(SelectedContent));
			}
		}

		public LayoutContent SelectedContent => _selectedIndex == -1 ? null : Children[_selectedIndex];

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

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
			RaisePropertyChanged(nameof(CanClose));
			RaisePropertyChanged(nameof(CanHide));
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			base.OnChildrenCollectionChanged();
		}

		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			if (oldValue is ILayoutGroup oldGroup) oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if (newValue is ILayoutGroup newGroup) newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;
			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			if (_id != null) writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
			if (_name != null) writer.WriteAttributeString(nameof(Name), _name);
			base.WriteXml(writer);
		}

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id))) _id = reader.Value;
			if (reader.MoveToAttribute(nameof(Name))) _name = reader.Value;
			_autoFixSelectedContent = false;
			base.ReadXml(reader);
			_autoFixSelectedContent = true;
			AutoFixSelectedContent();
		}

#if TRACE
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("AnchorablePane()");

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides

		#region Public Methods

		public int IndexOf(LayoutContent content)
		{
			if (!(content is LayoutAnchorable anchorableChild)) return -1;
			return Children.IndexOf(anchorableChild);
		}

		/// <summary>
		/// Gets whether the model hosts only 1 <see cref="LayoutAnchorable"/> (True)
		/// or whether there are more than one <see cref="LayoutAnchorable"/>s below
		/// this model pane.
		/// </summary>
		public bool IsDirectlyHostedInFloatingWindow
		{
			get
			{
				var parentFloatingWindow = this.FindParent<LayoutAnchorableFloatingWindow>();
				return parentFloatingWindow != null && parentFloatingWindow.IsSinglePane;
				//return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
			}
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

		private void AutoFixSelectedContent()
		{
			if (!_autoFixSelectedContent) return;
			if (SelectedContentIndex >= ChildrenCount) SelectedContentIndex = Children.Count - 1;
			if (SelectedContentIndex == -1 && ChildrenCount > 0) SetNextSelectedIndex();
		}

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods

		#region ILayoutPaneSerializable Interface

		string _id;

		/// <inheritdoc />
		string ILayoutPaneSerializable.Id
		{
			get => _id;
			set => _id = value;
		}

		#endregion ILayoutPaneSerializable Interface
	}
}
