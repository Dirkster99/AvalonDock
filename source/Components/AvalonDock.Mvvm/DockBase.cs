using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using AvalonDock.Core;
using CommunityToolkit.Mvvm.Input;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Base class for dock containers (holds multiple dockables) with navigation commands.
	/// </summary>
	[DataContract]
	public abstract class DockBase : DockableBase, IDock
	{
		private IList<IDockable>? _visibleDockables;
		private IDockable? _activeDockable;
		private IDockable? _defaultDockable;
		private IDockable? _focusedDockable;

		protected DockBase()
		{
			CloseCommand = new RelayCommand(() => Factory?.CloseDockable(this));
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IList<IDockable>? VisibleDockables
		{
			get => _visibleDockables;
			set => SetProperty(ref _visibleDockables, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IDockable? ActiveDockable
		{
			get => _activeDockable;
			set => SetProperty(ref _activeDockable, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IDockable? DefaultDockable
		{
			get => _defaultDockable;
			set => SetProperty(ref _defaultDockable, value);
		}

		[IgnoreDataMember]
		public IDockable? FocusedDockable
		{
			get => _focusedDockable;
			set => SetProperty(ref _focusedDockable, value);
		}

		[IgnoreDataMember]
		public virtual bool CanGoBack => false;

		[IgnoreDataMember]
		public virtual bool CanGoForward => false;

		[IgnoreDataMember]
		public ICommand CloseCommand { get; }
	}
}