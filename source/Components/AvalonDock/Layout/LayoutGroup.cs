/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	[Serializable]
	public abstract class LayoutGroup<T> : LayoutGroupBase, ILayoutGroup, IXmlSerializable where T : class, ILayoutElement
	{
		#region fields
		private ObservableCollection<T> _children = new ObservableCollection<T>();
		private bool _isVisible = true;
		#endregion fields

		#region Constructors

		internal LayoutGroup()
		{
			_children.CollectionChanged += _children_CollectionChanged;
		}

		#endregion Constructors

		#region Properties

		public ObservableCollection<T> Children => _children;

		public bool IsVisible
		{
			get => _isVisible;
			protected set
			{
				if (value == _isVisible) return;
				RaisePropertyChanging(nameof(IsVisible));
				_isVisible = value;
				OnIsVisibleChanged();
				RaisePropertyChanged(nameof(IsVisible));
			}
		}

		public int ChildrenCount => _children.Count;

		#endregion Properties

		#region Public Methods
		IEnumerable<ILayoutElement> ILayoutContainer.Children => _children.Cast<ILayoutElement>();

		public void ComputeVisibility() => IsVisible = GetVisibility();

		public void MoveChild(int oldIndex, int newIndex)
		{
			if (oldIndex == newIndex) return;
			_children.Move(oldIndex, newIndex);
			ChildMoved(oldIndex, newIndex);
		}

		public void RemoveChildAt(int childIndex)
		{
			_children.RemoveAt(childIndex);
		}

		public int IndexOfChild(ILayoutElement element)
		{
			return _children.Cast<ILayoutElement>().ToList().IndexOf(element);
		}

		public void InsertChildAt(int index, ILayoutElement element)
		{
			_children.Insert(index, (T)element);
		}

		public void RemoveChild(ILayoutElement element)
		{
			_children.Remove((T)element);
		}

		public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			var index = _children.IndexOf((T)oldElement);
			_children.Insert(index, (T)newElement);
			_children.RemoveAt(index + 1);
		}

		public void ReplaceChildAt(int index, ILayoutElement element)
		{
			_children[index] = (T)element;
		}

		/// <inheritdoc />
		public System.Xml.Schema.XmlSchema GetSchema() => null;

		/// <inheritdoc />
		public virtual void ReadXml(System.Xml.XmlReader reader)
		{
			reader.MoveToContent();
			if (reader.IsEmptyElement)
			{
				reader.Read();
				ComputeVisibility();
				return;
			}
			var localName = reader.LocalName;
			reader.Read();
			while (true)
			{
				if ((reader.LocalName == localName) &&
					(reader.NodeType == System.Xml.XmlNodeType.EndElement))
				{
					break;
				}
				if (reader.NodeType == System.Xml.XmlNodeType.Whitespace)
				{
					reader.Read();
					continue;
				}

				XmlSerializer serializer = null;
				if (reader.LocalName == nameof(LayoutAnchorablePaneGroup))
					serializer = new XmlSerializer(typeof(LayoutAnchorablePaneGroup));
				else if (reader.LocalName == nameof(LayoutAnchorablePane))
					serializer = new XmlSerializer(typeof(LayoutAnchorablePane));
				else if (reader.LocalName == nameof(LayoutAnchorable))
					serializer = new XmlSerializer(typeof(LayoutAnchorable));
				else if (reader.LocalName == nameof(LayoutDocumentPaneGroup))
					serializer = new XmlSerializer(typeof(LayoutDocumentPaneGroup));
				else if (reader.LocalName == nameof(LayoutDocumentPane))
					serializer = new XmlSerializer(typeof(LayoutDocumentPane));
				else if (reader.LocalName == nameof(LayoutDocument))
					serializer = new XmlSerializer(typeof(LayoutDocument));
				else if (reader.LocalName == nameof(LayoutAnchorGroup))
					serializer = new XmlSerializer(typeof(LayoutAnchorGroup));
				else if (reader.LocalName == nameof(LayoutPanel))
					serializer = new XmlSerializer(typeof(LayoutPanel));
				else
				{
					var type = FindType(reader.LocalName);
					if (type == null)
						throw new ArgumentException("AvalonDock.LayoutGroup doesn't know how to deserialize " + reader.LocalName);
					serializer = new XmlSerializer(type);
				}

				Children.Add((T)serializer.Deserialize(reader));
			}

			reader.ReadEndElement();
		}

		/// <inheritdoc />
		public virtual void WriteXml(System.Xml.XmlWriter writer)
		{
			foreach (var child in Children)
			{
				var type = child.GetType();
				var serializer = new XmlSerializer(type);
				serializer.Serialize(writer, child);
			}
		}

		#endregion Public Methods

		#region Internal Methods

		protected virtual void OnIsVisibleChanged()
		{
			UpdateParentVisibility();
		}

		protected abstract bool GetVisibility();

		protected virtual void ChildMoved(int oldIndex, int newIndex)
		{
		}

		#endregion Internal Methods

		#region Overrides

		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			base.OnParentChanged(oldValue, newValue);
			ComputeVisibility();
		}

		#endregion Overrides

		#region Private Methods

		private void _children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				if (e.OldItems != null)
				{
					foreach (LayoutElement element in e.OldItems)
						if (element.Parent == this) element.Parent = null;
				}
			}
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				if (e.NewItems != null)
				{
					foreach (LayoutElement element in e.NewItems)
					{
						if (element.Parent == this) continue;
						element.Parent?.RemoveChild(element);
						element.Parent = this;
					}
				}
			}

			ComputeVisibility();
			OnChildrenCollectionChanged();

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
				// #81 - Make parents update their children up the tree. Otherwise, they will not be redrawn.
				RaiseChildrenTreeChanged();
			else
				NotifyChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
			RaisePropertyChanged(nameof(ChildrenCount));
		}

		private void UpdateParentVisibility()
		{
			if (Parent is ILayoutElementWithVisibility parentPane)
				parentPane.ComputeVisibility();
		}

		private Type FindType(string name)
		{
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
				foreach (var t in a.GetTypes())
					if (t.Name.Equals(name)) return t;
			return null;
		}

		#endregion Private Methods
	}
}
