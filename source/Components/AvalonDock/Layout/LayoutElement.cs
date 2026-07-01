using System;
using System.ComponentModel;
using System.Windows;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout element.
	/// </summary>
	[Serializable]
	public abstract class LayoutElement : DependencyObject, ILayoutElement, Core.Serialization.ISerializableLayoutElement
	{
		[NonSerialized]
		private ILayoutContainer _parent = null;

		[NonSerialized]
		private ILayoutRoot _root = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutElement"/> class.
		/// </summary>
		internal LayoutElement()
		{
		}

		/// <summary>
		/// Occurs when the property changed event is raised.
		/// </summary>
		[field: NonSerialized]
		[field: XmlIgnore]
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Occurs when the property changing event is raised.
		/// </summary>
		[field: NonSerialized]
		[field: XmlIgnore]
		public event PropertyChangingEventHandler PropertyChanging;

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		[XmlIgnore]
		public ILayoutContainer Parent
		{
			get => _parent;
			set
			{
				if (_parent == value) return;
				var oldValue = _parent;
				var oldRoot = _root;
				RaisePropertyChanging(nameof(Parent));
				OnParentChanging(oldValue, value);
				_parent = value;
				OnParentChanged(oldValue, value);

				_root = Root;
				if (oldRoot != _root) OnRootChanged(oldRoot, _root);
				RaisePropertyChanged(nameof(Parent));
				if (Root is LayoutRoot root) root.FireLayoutUpdated();
			}
		}

		/// <summary>
		/// Gets the root.
		/// </summary>
		public ILayoutRoot Root
		{
			get
			{
				var parent = Parent;
				while (parent != null && (!(parent is ILayoutRoot))) parent = parent.Parent;
				return parent as ILayoutRoot;
			}
		}
#if TRACE
		/// <summary>
		/// Dumps this layout element to the trace output.
		/// </summary>
		/// <param name="tab">The indentation level.</param>
		public virtual void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine(this.ToString());
		}
#endif

		/// <summary>
		/// Executes the fix cached root on deserialize operation.
		/// </summary>
		public void FixCachedRootOnDeserialize()
		{
			if (_root == null)
				_root = Root;
		}

		/// <summary>
		/// Executes the on parent changing operation.
		/// </summary>
		/// <param name="oldValue">The previous value.</param>
		/// <param name="newValue">The new value.</param>
		protected virtual void OnParentChanging(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
		}

		/// <summary>
		/// Executes the on parent changed operation.
		/// </summary>
		/// <param name="oldValue">The previous value.</param>
		/// <param name="newValue">The new value.</param>
		protected virtual void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
		}

		/// <summary>
		/// Executes the on root changed operation.
		/// </summary>
		/// <param name="oldRoot">The old root.</param>
		/// <param name="newRoot">The new root.</param>
		protected virtual void OnRootChanged(ILayoutRoot oldRoot, ILayoutRoot newRoot)
		{
			((LayoutRoot)oldRoot)?.OnLayoutElementRemoved(this);
			((LayoutRoot)newRoot)?.OnLayoutElementAdded(this);
		}

		/// <summary>
		/// Raises the property changed.
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		protected virtual void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		/// <summary>
		/// Raises the property changing.
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		protected virtual void RaisePropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
	}
}