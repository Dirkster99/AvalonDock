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
	[ContentProperty("Children")]
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
			this.Children.Add(firstChild);
		}

		#endregion Constructors

		#region Properties

		public bool ShowHeader
		{
			get
			{
				return _showHeader;
			}
			set
			{
				if (value != _showHeader)
				{
					_showHeader = value;
					RaisePropertyChanged("ShowHeader");
				}
			}
		}

		public int SelectedContentIndex
		{
			get
			{
				return _selectedIndex;
			}
			set
			{
				if (value < 0 ||
					value >= Children.Count)
					value = -1;

				if (_selectedIndex != value)
				{
					RaisePropertyChanging("SelectedContentIndex");
					RaisePropertyChanging("SelectedContent");
					if (_selectedIndex >= 0 &&
						_selectedIndex < Children.Count)
						Children[_selectedIndex].IsSelected = false;

					_selectedIndex = value;

					if (_selectedIndex >= 0 &&
						_selectedIndex < Children.Count)
						Children[_selectedIndex].IsSelected = true;

					RaisePropertyChanged("SelectedContentIndex");
					RaisePropertyChanged("SelectedContent");
				}
			}
		}

		public LayoutContent SelectedContent
		{
			get
			{
				return _selectedIndex == -1 ? null : Children[_selectedIndex];
			}
		}

		string ILayoutPaneSerializable.Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}
		#endregion Properties

		#region Overrides

		protected override bool GetVisibility()
		{
			if (this.Parent is LayoutDocumentPaneGroup)
				return (this.ChildrenCount > 0) && this.Children.Any(c => (c is LayoutDocument && ((LayoutDocument)c).IsVisible) || (c is LayoutAnchorable));

			return true;
		}

		protected override void ChildMoved(int oldIndex, int newIndex)
		{
			if (_selectedIndex == oldIndex)
			{
				RaisePropertyChanging("SelectedContentIndex");
				_selectedIndex = newIndex;
				RaisePropertyChanged("SelectedContentIndex");
			}


			base.ChildMoved(oldIndex, newIndex);
		}

		protected override void OnChildrenCollectionChanged()
		{
			AutoFixSelectedContent();
			for (int i = 0; i < Children.Count; i++)
			{
				if (Children[i].IsSelected)
				{
					SelectedContentIndex = i;
					break;
				}
			}

			RaisePropertyChanged("CanClose");
			RaisePropertyChanged("CanHide");
			RaisePropertyChanged("IsDirectlyHostedInFloatingWindow");
			base.OnChildrenCollectionChanged();

			RaisePropertyChanged( "ChildrenSorted" );
		}

		[XmlIgnore]
		bool _autoFixSelectedContent = true;
		void AutoFixSelectedContent()
		{
			if (_autoFixSelectedContent)
			{
				if (SelectedContentIndex >= ChildrenCount)
					SelectedContentIndex = Children.Count - 1;

				if (SelectedContentIndex == -1 && ChildrenCount > 0)
					SetNextSelectedIndex();
			}
		}

		public bool IsDirectlyHostedInFloatingWindow
		{
			get
			{
				var parentFloatingWindow = this.FindParent<LayoutDocumentFloatingWindow>();
				if (parentFloatingWindow != null)
					return parentFloatingWindow.IsSinglePane;

				return false;
				//return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
			}
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			if (_id != null)
				writer.WriteAttributeString("Id", _id);
			if (!_showHeader)
				writer.WriteAttributeString("ShowHeader", _showHeader.ToString());

			base.WriteXml(writer);
		}

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute("Id"))
				_id = reader.Value;
			if (reader.MoveToAttribute("ShowHeader"))
				_showHeader = bool.Parse(reader.Value);


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
			if (documentChild == null)
				return -1;
			return Children.IndexOf(documentChild);
		}

		#endregion Public Methods

		#region Internal Methods

		internal void SetNextSelectedIndex()
		{
			this.SelectedContentIndex = -1;
			for (int i = 0; i < this.Children.Count; ++i)
			{
				if (this.Children[i].IsEnabled)
				{
					this.SelectedContentIndex = i;
					return;
				}
			}
		}

		internal void UpdateIsDirectlyHostedInFloatingWindow()
		{
			RaisePropertyChanged("IsDirectlyHostedInFloatingWindow");
		}

		#endregion Internal Methods

		#region Private Methods

		public bool IsHostedInFloatingWindow
		{
			get
			{
				return this.FindParent<LayoutFloatingWindow>() != null;
			}
		}
		
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
			var oldGroup = oldValue as ILayoutGroup;
			if (oldGroup != null)
				oldGroup.ChildrenCollectionChanged -= new EventHandler(OnParentChildrenCollectionChanged);

			RaisePropertyChanged("IsDirectlyHostedInFloatingWindow");

			var newGroup = newValue as ILayoutGroup;
			if (newGroup != null)
				newGroup.ChildrenCollectionChanged += new EventHandler(OnParentChildrenCollectionChanged);

			base.OnParentChanged(oldValue, newValue);
		}

		void OnParentChildrenCollectionChanged(object sender, EventArgs e)
		{
			RaisePropertyChanged("IsDirectlyHostedInFloatingWindow");
		}

		#endregion Private Methods
	}
}
