using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Concrete factory using AvalonDock.Mvvm view models.
	/// Users inherit from this class and override <see cref="CreateLayout"/> to define their layout.
	/// </summary>
	public class Factory : FactoryBase
	{
		/// <summary>
		/// Creates a new root dock view model.
		/// </summary>
		/// <returns>A new <see cref="IRootDock"/> instance.</returns>
		public override IRootDock CreateRootDock() => new RootDock();

		/// <summary>
		/// Creates a new document dock view model.
		/// </summary>
		/// <returns>A new <see cref="IDocumentDock"/> instance.</returns>
		public override IDocumentDock CreateDocumentDock() => new DocumentDock();

		/// <summary>
		/// Creates a new tool dock view model.
		/// </summary>
		/// <returns>A new <see cref="IToolDock"/> instance.</returns>
		public override IToolDock CreateToolDock() => new ToolDock();

		/// <summary>
		/// Creates a list containing the supplied dockables.
		/// </summary>
		/// <typeparam name="T">The item type stored in the list.</typeparam>
		/// <param name="items">The items to include in the created list.</param>
		/// <returns>A new list initialized with <paramref name="items"/>.</returns>
		public override IList<T> CreateList<T>(params T[] items)
			=> new ObservableCollection<T>(items);

		/// <summary>
		/// Creates the default root layout.
		/// </summary>
		/// <returns>A new <see cref="IRootDock"/> layout.</returns>
		public override IRootDock CreateLayout() => CreateRootDock();
	}
}