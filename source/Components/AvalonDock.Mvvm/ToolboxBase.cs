using System.Runtime.Serialization;
using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Base class for toolbox (anchorable tool) view models in the toggle docking system.
	/// Inherits from <see cref="DockableBase"/> and implements <see cref="IToolbox"/>.
	/// </summary>
	/// <remarks>
	/// <para>Inherit from this class for your toolbox ViewModels. Set <see cref="Zone"/>
	/// in the constructor to control placement. Override <see cref="DockableBase.Title"/>
	/// and <see cref="DockableBase.Id"/> for display and serialization.</para>
	/// <para>Register with DI using <c>services.AddToolbox&lt;T&gt;()</c> and they will
	/// be automatically placed into the ToggleDockingManager layout.</para>
	/// </remarks>
	[DataContract]
	public abstract class ToolboxBase : DockableBase, IToolbox
	{
		private string? _toolTipText;
		private DockZone _zone = DockZone.LeftTop;
		private bool _isOpenByDefault;
		private object? _icon;

		/// <inheritdoc/>
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public string? ToolTipText
		{
			get => _toolTipText;
			set => SetProperty(ref _toolTipText, value);
		}

		/// <inheritdoc/>
		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public DockZone Zone
		{
			get => _zone;
			set => SetProperty(ref _zone, value);
		}

		/// <inheritdoc/>
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public bool IsOpenByDefault
		{
			get => _isOpenByDefault;
			set => SetProperty(ref _isOpenByDefault, value);
		}

		/// <inheritdoc/>
		[IgnoreDataMember]
		public object? Icon
		{
			get => _icon;
			set => SetProperty(ref _icon, value);
		}
	}
}