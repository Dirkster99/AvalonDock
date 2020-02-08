/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows;
using System.ComponentModel;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Implements an abstract base class for almost all layout models in the AvalonDock.Layout namespace.
	/// 
	/// This base inherites from <see cref="DependencyObject"/> and implements <see cref="PropertyChanged"/>
	/// and <see cref="PropertyChanging"/> events. Deriving classes can, therefore, implement
	/// depedency object and/or viewmodel specific functionalities.
	/// class supports both 
	/// </summary>
	[Serializable]
	public abstract class LayoutElement : DependencyObject, ILayoutElement
	{
		#region fields
		[NonSerialized]
		private ILayoutContainer _parent = null;

		[NonSerialized]
		private ILayoutRoot _root = null;
		#endregion fields

		#region Constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		internal LayoutElement()
		{
		}

		#endregion Constructors

		#region Events
		/// <summary>Raised when a property has changed (after the change has taken place).</summary>
		[field: NonSerialized]
		[field: XmlIgnore]
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>Raised when a property is about to change (raised before the actual change).</summary>
		[field: NonSerialized]
		[field: XmlIgnore]
		public event PropertyChangingEventHandler PropertyChanging;

		#endregion Events

		#region Properties
		/// <summary>Gets or sets the parent container of the element</summary>
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

		/// <summary>Gets or sets the layout root of the element.</summary>
		public ILayoutRoot Root
		{
			get
			{
				var parent = Parent;
				while (parent != null && (!(parent is ILayoutRoot))) parent = parent.Parent;
				return parent as ILayoutRoot;
			}
		}
		#endregion Properties

		#region Public Methods

#if TRACE
		public virtual void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new String(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine(this.ToString());
		}
#endif

		#endregion Public Methods

		#region protected methods

		/// <summary>Provides derived classes an opportunity to handle execute code before to the <see cref="Parent"/> property changes.</summary>
		protected virtual void OnParentChanging(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
		}

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Parent"/> property.</summary>
		protected virtual void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
		}

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Root"/> property.</summary>
		protected virtual void OnRootChanged(ILayoutRoot oldRoot, ILayoutRoot newRoot)
		{
			((LayoutRoot) oldRoot)?.OnLayoutElementRemoved(this);
			((LayoutRoot) newRoot)?.OnLayoutElementAdded(this);
		}

		/// <summary>Should be invoked to raise the <see cref="PropertyChanged"/> event for the property named in <paramref name="propertyName"/>.
		/// This event should be fired AFTER changing properties with viewmodel binding support.
		/// </summary>
		protected virtual void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		/// <summary>Should be invoked to raise the <see cref="RaisePropertyChanging"/> event for the property named in <paramref name="propertyName"/>.
		/// This event should be fired BEFORE changing properties with viewmodel binding support.
		/// </summary>
		protected virtual void RaisePropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

		#endregion protected methods
	}
}
