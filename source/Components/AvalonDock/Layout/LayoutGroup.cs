using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout group.
	/// </summary>
	/// <typeparam name="T">The type of the related layout element.</typeparam>
	[Serializable]
	public abstract class LayoutGroup<T> : LayoutGroupBase, ILayoutGroup, IXmlSerializable
		where T : class, ILayoutElement
	{
		private readonly ObservableCollection<T> _children = new ObservableCollection<T>();
		private bool _isVisible = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutGroup{T}"/> class.
		/// </summary>
		internal LayoutGroup()
		{
			_children.CollectionChanged += Children_CollectionChanged;
		}

		/// <summary>
		/// Gets the children.
		/// </summary>
		public ObservableCollection<T> Children => _children;

		/// <summary>
		/// Gets the children count.
		/// </summary>
		public int ChildrenCount => _children.Count;

		/// <inheritdoc/>
		IEnumerable<ILayoutElement> ILayoutContainer.Children => _children.Cast<ILayoutElement>();

		/// <summary>
		/// Gets or sets a value indicating whether this instance is visible.
		/// </summary>
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

		/// <summary>
		/// Executes the compute visibility operation.
		/// </summary>
		public void ComputeVisibility() => IsVisible = GetVisibility();

		/// <summary>
		/// Executes the move child operation.
		/// </summary>
		/// <param name="oldIndex">The old index.</param>
		/// <param name="newIndex">The new index.</param>
		public void MoveChild(int oldIndex, int newIndex)
		{
			if (oldIndex == newIndex) return;
			_children.Move(oldIndex, newIndex);
			ChildMoved(oldIndex, newIndex);
		}

		/// <summary>
		/// Removes the child at.
		/// </summary>
		/// <param name="childIndex">The child index.</param>
		public void RemoveChildAt(int childIndex)
		{
			_children.RemoveAt(childIndex);
		}

		/// <summary>
		/// Executes the index of child operation.
		/// </summary>
		/// <param name="element">The layout element.</param>
		/// <returns>The resulting value.</returns>
		public int IndexOfChild(ILayoutElement element)
		{
			return _children.Cast<ILayoutElement>().ToList().IndexOf(element);
		}

		/// <summary>
		/// Inserts the child at.
		/// </summary>
		/// <param name="index">The zero-based index.</param>
		/// <param name="element">The layout element.</param>
		public void InsertChildAt(int index, ILayoutElement element)
		{
			if (element is T t)
				_children.Insert(index, t);
		}

		/// <summary>
		/// Removes the child.
		/// </summary>
		/// <param name="element">The layout element.</param>
		public void RemoveChild(ILayoutElement element)
		{
			if (element is T t)
				_children.Remove(t);
		}

		/// <summary>
		/// Replaces the child.
		/// </summary>
		/// <param name="oldElement">The existing layout element.</param>
		/// <param name="newElement">The replacement layout element.</param>
		public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			if (oldElement is T oldT && newElement is T newT)
			{
				var index = _children.IndexOf(oldT);
				_children.Insert(index, newT);
				_children.RemoveAt(index + 1);
			}
		}

		/// <summary>
		/// Replaces the child at.
		/// </summary>
		/// <param name="index">The zero-based index.</param>
		/// <param name="element">The layout element.</param>
		public void ReplaceChildAt(int index, ILayoutElement element)
		{
			_children[index] = (T)element;
		}

		/// <inheritdoc/>
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() => null;

		/// <summary>
		/// Reads the xml.
		/// </summary>
		/// <param name="reader">The XML reader to read from.</param>
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

				string fullName = string.Format("{0}.{1}", GetType().Namespace, reader.LocalName);
				Type typeForSerializer = Type.GetType(fullName);

				if (typeForSerializer == null)
					typeForSerializer = FindType(reader.LocalName);

				if (typeForSerializer == null)
					throw new ArgumentException("AvalonDock.LayoutGroup doesn't know how to deserialize " + reader.LocalName);

				XmlSerializer serializer = XmlSerializersCache.GetSerializer(typeForSerializer);
				Children.Add((T)serializer.Deserialize(reader));
			}

			reader.ReadEndElement();
		}

		/// <summary>
		/// Writes the xml.
		/// </summary>
		/// <param name="writer">The XML writer to write to.</param>
		public virtual void WriteXml(System.Xml.XmlWriter writer)
		{
			foreach (var child in Children)
			{
				var type = child.GetType();
				var serializer = XmlSerializersCache.GetSerializer(type);
				serializer.Serialize(writer, child);
			}
		}

		/// <summary>
		/// Executes the on is visible changed operation.
		/// </summary>
		protected virtual void OnIsVisibleChanged()
		{
			UpdateParentVisibility();
		}

		/// <summary>
		/// Gets the visibility.
		/// </summary>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		protected abstract bool GetVisibility();

		/// <summary>
		/// Executes the child moved operation.
		/// </summary>
		/// <param name="oldIndex">The old index.</param>
		/// <param name="newIndex">The new index.</param>
		protected virtual void ChildMoved(int oldIndex, int newIndex)
		{
		}

		/// <inheritdoc/>
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			base.OnParentChanged(oldValue, newValue);
			ComputeVisibility();
		}

		/// <summary>
		/// Executes the children collection changed operation.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
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

		/// <summary>
		/// Updates the parent visibility.
		/// </summary>
		private void UpdateParentVisibility()
		{
			if (Parent is ILayoutElementWithVisibility parentPane)
				parentPane.ComputeVisibility();
		}

		/// <summary>
		/// Finds the type.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The resulting value.</returns>
		private Type FindType(string name)
		{
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var t in a.GetTypes())
					if (t.Name.Equals(name)) return t;
			}

			return null;
		}
	}
}