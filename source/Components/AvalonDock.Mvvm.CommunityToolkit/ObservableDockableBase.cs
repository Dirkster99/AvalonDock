using System.Runtime.Serialization;
using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvalonDock.Mvvm.CommunityToolkit
{
	/// <summary>
	/// Base class for dockable view models backed by <see cref="ObservableObject"/>
	/// from CommunityToolkit.Mvvm. Supports <c>[ObservableProperty]</c> and
	/// <c>[RelayCommand]</c> source generators in derived classes.
	/// </summary>
	[DataContract]
	public abstract class ObservableDockableBase : ObservableObject, IDockable
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
	}
}