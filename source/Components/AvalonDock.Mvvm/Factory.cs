using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Concrete factory using CommunityToolkit.Mvvm view models.
	/// Users inherit from this class and override <see cref="CreateLayout"/> to define their layout.
	/// </summary>
	public class Factory : FactoryBase
	{
		public override IRootDock CreateRootDock() => new RootDock();

		public override IDocumentDock CreateDocumentDock() => new DocumentDock();

		public override IToolDock CreateToolDock() => new ToolDock();

		public override IList<T> CreateList<T>(params T[] items)
			=> new ObservableCollection<T>(items);

		public override IRootDock CreateLayout() => CreateRootDock();
	}
}