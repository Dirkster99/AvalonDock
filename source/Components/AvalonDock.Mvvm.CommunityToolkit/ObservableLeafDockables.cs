using System.Runtime.Serialization;

namespace AvalonDock.Mvvm.CommunityToolkit
{
	/// <summary>
	/// Leaf dockable view model for tools/anchorables, backed by
	/// <see cref="global::CommunityToolkit.Mvvm.ComponentModel.ObservableObject"/>.
	/// Supports <c>[ObservableProperty]</c> and <c>[RelayCommand]</c> source generators.
	/// </summary>
	[DataContract]
	public class ObservableTool : ObservableDockableBase
	{
	}

	/// <summary>
	/// Leaf dockable view model for documents, backed by
	/// <see cref="global::CommunityToolkit.Mvvm.ComponentModel.ObservableObject"/>.
	/// Supports <c>[ObservableProperty]</c> and <c>[RelayCommand]</c> source generators.
	/// </summary>
	[DataContract]
	public class ObservableDocument : ObservableDockableBase
	{
	}
}