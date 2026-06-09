using System.Runtime.Serialization;
using AvalonDock.Core;

namespace AvalonDock.Mvvm.CommunityToolkit
{
	/// <summary>
	/// Base class for toolbox (anchorable tool) view models backed by
	/// <see cref="global::CommunityToolkit.Mvvm.ComponentModel.ObservableObject"/>.
	/// Supports <c>[ObservableProperty]</c> and <c>[RelayCommand]</c> source generators.
	/// </summary>
	/// <remarks>
	/// <para>Inherit from this class for your toolbox ViewModels when using
	/// CommunityToolkit.Mvvm source generators. Set <see cref="Zone"/> in the
	/// constructor to control placement.</para>
	/// <para>Register with DI using <c>dock.AddToolbox&lt;T&gt;()</c> inside the
	/// <c>AddDockLayoutService</c> builder.</para>
	/// </remarks>
	[DataContract]
	public abstract class ObservableToolboxBase : ObservableDockableBase, IToolbox
	{
		private string? _toolTipText;
		private DockZone _zone = DockZone.LeftTop;
		private bool _isOpenByDefault;
		private bool _isOpen;
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
		public bool IsOpen
		{
			get => _isOpen;
			set => SetProperty(ref _isOpen, value);
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