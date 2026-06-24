using System;
using System.Linq;

namespace AvalonDock.Core
{
	/// <summary>
	/// Extension methods for <see cref="IDockLayoutService"/> that provide
	/// higher-level operations built on the service's core primitives.
	/// </summary>
	public static class DockLayoutServiceExtensions
	{
		/// <summary>Finds the first open document matching the predicate.</summary>
		/// <typeparam name="T">The concrete document type.</typeparam>
		/// <param name="service">The layout service.</param>
		/// <param name="predicate">A predicate to match against.</param>
		/// <returns>The matching document, or null if not found.</returns>
		public static T? FindDocument<T>(this IDockLayoutService service, Func<T, bool> predicate)
			where T : class, IDockable
		{
			return service.Documents.OfType<T>().FirstOrDefault(predicate);
		}

		/// <summary>Opens a document if not already open, otherwise activates it.</summary>
		/// <typeparam name="T">The concrete document type.</typeparam>
		/// <param name="service">The layout service.</param>
		/// <param name="predicate">Predicate to find an existing instance.</param>
		/// <param name="factory">Factory to create a new instance if not found.</param>
		/// <returns>The existing or newly created document.</returns>
		public static T OpenOrActivateDocument<T>(this IDockLayoutService service, Func<T, bool> predicate, Func<T> factory)
			where T : class, IDockable
		{
			var existing = service.FindDocument(predicate);
			if (existing != null)
			{
				service.ActiveDockable = existing;
				return existing;
			}

			var doc = factory();
			service.OpenDocument(doc);
			return doc;
		}

		/// <summary>Gets a registered anchorable/tool by type.</summary>
		/// <typeparam name="T">The concrete anchorable type.</typeparam>
		/// <param name="service">The layout service.</param>
		/// <returns>The anchorable instance, or null if not registered.</returns>
		public static T? GetAnchorable<T>(this IDockLayoutService service)
			where T : class, IToolbox
		{
			return service.Anchorables.OfType<T>().FirstOrDefault();
		}

		/// <summary>Docks (shows) the specified anchorable by setting <see cref="IToolbox.IsOpen"/> to <c>true</c>.</summary>
		/// <param name="service">The layout service.</param>
		/// <param name="anchorable">The anchorable to show.</param>
		public static void ShowAnchorable(this IDockLayoutService service, IDockable anchorable)
		{
			if (anchorable is IToolbox toolbox)
			{
				toolbox.IsOpen = true;
			}
		}

		/// <summary>Auto-hides the specified anchorable by setting <see cref="IToolbox.IsOpen"/> to <c>false</c>.</summary>
		/// <param name="service">The layout service.</param>
		/// <param name="anchorable">The anchorable to hide.</param>
		public static void HideAnchorable(this IDockLayoutService service, IDockable anchorable)
		{
			if (anchorable is IToolbox toolbox)
			{
				toolbox.IsOpen = false;
			}
		}

		/// <summary>Gets a value indicating whether the specified anchorable is currently docked (not auto-hidden).</summary>
		/// <param name="service">The layout service.</param>
		/// <param name="anchorable">The anchorable to query.</param>
		/// <returns><c>true</c> if the anchorable is docked; <c>false</c> if auto-hidden or not found.</returns>
		public static bool IsAnchorableOpen(this IDockLayoutService service, IDockable anchorable)
		{
			return anchorable is IToolbox toolbox && toolbox.IsOpen;
		}

		/// <summary>Gets a value indicating whether any anchorable on the specified side is docked.</summary>
		/// <param name="service">The layout service.</param>
		/// <param name="side">The side to query.</param>
		/// <returns><c>true</c> if any anchorable on the side is docked; otherwise <c>false</c>.</returns>
		public static bool IsSideOpen(this IDockLayoutService service, ToolboxSide side)
		{
			return service.Anchorables.OfType<IToolbox>()
				.Any(t => t.Zone.ToSide() == side && t.IsOpen);
		}
	}
}