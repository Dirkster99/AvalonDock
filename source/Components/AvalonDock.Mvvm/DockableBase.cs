using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Base class for all dockable view models.
	/// Properties decorated with [DataMember] are serialized; [IgnoreDataMember] are runtime-only.
	/// </summary>
	[DataContract]
	public abstract class DockableBase : INotifyPropertyChanged, IDockable
	{
		private string _id = string.Empty;
		private string _title = string.Empty;
		private object? _context;
		private IDockable? _owner;
		private IFactory? _factory;
		private bool _canClose = true;
		private bool _canPin = true;
		private bool _canFloat = true;
		private bool _canDrag = true;
		private bool _canDrop = true;
		private bool _isModified;
		private bool _isActive;
		private DockState _dockState = DockState.Docked;

		/// <inheritdoc/>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Gets or sets the unique identifier of the dockable.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public string Id
		{
			get => _id;
			set => SetProperty(ref _id, value);
		}

		/// <summary>
		/// Gets or sets the display title of the dockable.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		/// <summary>
		/// Gets or sets the runtime context associated with the dockable.
		/// </summary>
		[IgnoreDataMember]
		public object? Context
		{
			get => _context;
			set => SetProperty(ref _context, value);
		}

		/// <summary>
		/// Gets or sets the parent dockable that owns this instance.
		/// </summary>
		[IgnoreDataMember]
		public IDockable? Owner
		{
			get => _owner;
			set => SetProperty(ref _owner, value);
		}

		/// <summary>
		/// Gets or sets the factory that manages this dockable.
		/// </summary>
		[IgnoreDataMember]
		public IFactory? Factory
		{
			get => _factory;
			set => SetProperty(ref _factory, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dockable can be closed.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanClose
		{
			get => _canClose;
			set => SetProperty(ref _canClose, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dockable can be pinned.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanPin
		{
			get => _canPin;
			set => SetProperty(ref _canPin, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dockable can float.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanFloat
		{
			get => _canFloat;
			set => SetProperty(ref _canFloat, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dockable can be dragged.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanDrag
		{
			get => _canDrag;
			set => SetProperty(ref _canDrag, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dockable can accept dropped items.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanDrop
		{
			get => _canDrop;
			set => SetProperty(ref _canDrop, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dockable has unsaved changes.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public bool IsModified
		{
			get => _isModified;
			set => SetProperty(ref _isModified, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dockable is currently active.
		/// </summary>
		[IgnoreDataMember]
		public bool IsActive
		{
			get => _isActive;
			set => SetProperty(ref _isActive, value);
		}

		/// <summary>
		/// Gets or sets the current docking state of the dockable.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public DockState DockState
		{
			get => _dockState;
			set => SetProperty(ref _dockState, value);
		}

		/// <summary>
		/// Determines whether the dockable can be closed.
		/// </summary>
		/// <returns><see langword="true"/> when closing is allowed; otherwise, <see langword="false"/>.</returns>
		public virtual bool OnClose() => true;

		/// <summary>
		/// Handles selection of the dockable.
		/// </summary>
		public virtual void OnSelected()
		{
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="e">The event args containing the property name.</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event for the specified property.
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Sets a property value and raises <see cref="PropertyChanged"/> if the value changed.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="field">A reference to the backing field.</param>
		/// <param name="value">The new value.</param>
		/// <param name="propertyName">The property name (auto-filled by the compiler).</param>
		/// <returns><see langword="true"/> if the value changed; otherwise, <see langword="false"/>.</returns>
		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
				return false;

			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}