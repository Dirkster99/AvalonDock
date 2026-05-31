using System.Linq;

namespace AvalonDock.Core
{
	/// <summary>
	/// Extension methods for <see cref="IDockLayoutService"/> that provide
	/// higher-level operations built on the primitive show/hide API.
	/// </summary>
	public static class DockLayoutServiceExtensions
	{
		/// <summary>
		/// Toggles all anchorables on the specified side.
		/// If any are open, all are hidden. If none are open, the first available is shown.
		/// </summary>
		/// <param name="service">The layout service.</param>
		/// <param name="side">The side to toggle.</param>
		public static void ToggleSide(this IDockLayoutService service, ToolboxSide side)
		{
			var toolboxes = service.Anchorables
				.OfType<IToolbox>()
				.Where(t => t.Zone.ToSide() == side)
				.ToList();

			if (toolboxes.Count == 0)
			{
				return;
			}

			bool anyOpen = toolboxes.Any(service.IsAnchorableOpen);

			if (anyOpen)
			{
				foreach (var t in toolboxes)
				{
					if (service.IsAnchorableOpen(t))
					{
						service.HideAnchorable(t);
					}
				}
			}
			else
			{
				service.ShowAnchorable(toolboxes[0]);
			}
		}
	}
}