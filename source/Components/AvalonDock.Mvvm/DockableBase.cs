using System;
using System.Runtime.Serialization;
using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Base class for all dockable view models using CommunityToolkit.Mvvm.
	/// Properties decorated with [DataMember] are serialized; [IgnoreDataMember] are runtime-only.
	/// </summary>
	[DataContract]
	public abstract class DockableBase : ObservableObject, IDockable
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

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public string Id
		{
			get => _id;
			set => SetProperty(ref _id, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		[IgnoreDataMember]
		public object? Context
		{
			get => _context;
			set => SetProperty(ref _context, value);
		}

		[IgnoreDataMember]
		public IDockable? Owner
		{
			get => _owner;
			set => SetProperty(ref _owner, value);
		}

		[IgnoreDataMember]
		public IFactory? Factory
		{
			get => _factory;
			set => SetProperty(ref _factory, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanClose
		{
			get => _canClose;
			set => SetProperty(ref _canClose, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanPin
		{
			get => _canPin;
			set => SetProperty(ref _canPin, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanFloat
		{
			get => _canFloat;
			set => SetProperty(ref _canFloat, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanDrag
		{
			get => _canDrag;
			set => SetProperty(ref _canDrag, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanDrop
		{
			get => _canDrop;
			set => SetProperty(ref _canDrop, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public bool IsModified
		{
			get => _isModified;
			set => SetProperty(ref _isModified, value);
		}

		[IgnoreDataMember]
		public bool IsActive
		{
			get => _isActive;
			set => SetProperty(ref _isActive, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public DockState DockState
		{
			get => _dockState;
			set => SetProperty(ref _dockState, value);
		}

		public virtual bool OnClose() => true;

		public virtual void OnSelected()
		{
		}
	}
}