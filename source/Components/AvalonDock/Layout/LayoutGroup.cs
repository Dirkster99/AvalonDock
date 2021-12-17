/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout anchorable (group and non-group) related classes
	/// that implement the viewmodel aspect for layout anchorable controls.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class LayoutGroup<T> : LayoutGroupBase, ILayoutGroup, IXmlSerializable where T : class, ILayoutElement
	{
		#region fields

		private readonly ObservableCollection<T> _children = new ObservableCollection<T>();
		private bool _isVisible = true;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor.</summary>
		internal LayoutGroup()
		{
			_children.CollectionChanged += Children_CollectionChanged;
		}

		#endregion Constructors

		#region Properties

		/// <summary>Gets a collection of children objects below this object.</summary>
		public ObservableCollection<T> Children => _children;

		/// <summary>Gets the number of of children objects below this object.</summary>
		public int ChildrenCount => _children.Count;

		/// <summary>Gets a collection of <see cref="ILayoutElement"/> based children objects below this object.</summary>
		IEnumerable<ILayoutElement> ILayoutContainer.Children => _children.Cast<ILayoutElement>();

		/// <summary>Gets whether this object is visible or not.</summary>
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

		#endregion Properties

		#region Public Methods

		/// <inheritdoc cref="ILayoutElementWithVisibility" />
		public void ComputeVisibility() => IsVisible = GetVisibility();

		/// <inheritdoc cref="ILayoutPane" />
		public void MoveChild(int oldIndex, int newIndex)
		{
			if (oldIndex == newIndex) return;
			_children.Move(oldIndex, newIndex);
			ChildMoved(oldIndex, newIndex);
		}

		/// <inheritdoc cref="ILayoutGroup" />
		public void RemoveChildAt(int childIndex)
		{
			_children.RemoveAt(childIndex);
		}

		/// <inheritdoc cref="ILayoutGroup" />
		public int IndexOfChild(ILayoutElement element)
		{
			return _children.Cast<ILayoutElement>().ToList().IndexOf(element);
		}

		/// <inheritdoc cref="ILayoutGroup" />
		public void InsertChildAt(int index, ILayoutElement element)
		{
			if (element is T t)
				_children.Insert(index, t);
		}

		/// <inheritdoc cref="ILayoutContainer" />
		public void RemoveChild(ILayoutElement element)
		{
			if (element is T t)
				_children.Remove(t);
		}

		/// <inheritdoc cref="ILayoutContainer" />
		public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			if (oldElement is T oldT && newElement is T newT)
			{
				var index = _children.IndexOf(oldT);
				_children.Insert(index, newT);
				_children.RemoveAt(index + 1);
			}
		}

		/// <inheritdoc cref="ILayoutGroup" />
		public void ReplaceChildAt(int index, ILayoutElement element)
		{
			_children[index] = (T)element;
		}

		/// <inheritdoc cref=" IXmlSerializable." />
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() => null;

		/// <inheritdoc cref=" IXmlSerializable." />
		/// <summary>provides a standard overridable implementation for deriving classes.</summary>
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
				if (reader.LocalName == localName && reader.NodeType == System.Xml.XmlNodeType.EndElement)
					break;
				if (reader.NodeType == System.Xml.XmlNodeType.Whitespace)
				{
					reader.Read();
					continue;
				}

				string fullName = String.Format("{0}.{1}", GetType().Namespace, reader.LocalName);
				Type typeForSerializer = Type.GetType(fullName);

				if (typeForSerializer == null)
					typeForSerializer = FindType(reader.LocalName);

				if (typeForSerializer == null)
					throw new ArgumentException("AvalonDock.LayoutGroup doesn't know how to deserialize " + reader.LocalName);

				XmlSerializer serializer = XmlSerializer.FromTypes(new[] { typeForSerializer })[0];
				Children.Add((T)serializer.Deserialize(reader));
			}

			reader.ReadEndElement();
		}

		/// <inheritdoc cref=" IXmlSerializable." />
		/// <summary>provides a standard overridable implementation for deriving classes.</summary>
		public virtual void WriteXml(System.Xml.XmlWriter writer)
		{
			foreach (var child in Children)
			{
				var type = child.GetType();
				var serializer = XmlSerializer.FromTypes(new[] { type })[0];
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

		private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
			{
				if (e.OldItems != null)
				{
					foreach (LayoutElement element in e.OldItems)
						if (element.Parent == this || e.Action == NotifyCollectionChangedAction.Remove) element.Parent = null;
				}
			}
			if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
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

			if (e.Action == NotifyCollectionChangedAction.Add)
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