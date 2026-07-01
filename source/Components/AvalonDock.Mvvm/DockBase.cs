using System.Collections.Generic;
using System.Runtime.Serialization;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Base class for dock containers (holds multiple dockables).
	/// </summary>
	[DataContract]
	public abstract class DockBase : DockableBase, IDock
	{
		private IList<IDockable>? _visibleDockables;
		private IDockable? _activeDockable;
		private IDockable? _defaultDockable;
		private IDockable? _focusedDockable;

		/// <summary>
		/// Gets or sets the visible dockables contained by this dock.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IList<IDockable>? VisibleDockables
		{
			get => _visibleDockables;
			set => SetProperty(ref _visibleDockables, value);
		}

		/// <summary>
		/// Gets or sets the currently active dockable.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IDockable? ActiveDockable
		{
			get => _activeDockable;
			set => SetProperty(ref _activeDockable, value);
		}

		/// <summary>
		/// Gets or sets the default dockable shown by this dock.
		/// </summary>
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IDockable? DefaultDockable
		{
			get => _defaultDockable;
			set => SetProperty(ref _defaultDockable, value);
		}

		/// <summary>
		/// Gets or sets the dockable that currently has focus.
		/// </summary>
		[IgnoreDataMember]
		public IDockable? FocusedDockable
		{
			get => _focusedDockable;
			set => SetProperty(ref _focusedDockable, value);
		}

		/// <summary>
		/// Gets a value indicating whether backward navigation is available.
		/// </summary>
		[IgnoreDataMember]
		public virtual bool CanGoBack => false;

		/// <summary>
		/// Gets a value indicating whether forward navigation is available.
		/// </summary>
		[IgnoreDataMember]
		public virtual bool CanGoForward => false;
	}
}