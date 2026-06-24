using System.Collections.Generic;
using System.Linq;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Manages toggling entire dock sides (left, right, bottom) with
	/// last-open memory so the previously visible toolboxes are restored
	/// when the side is toggled back on.
	/// </summary>
	public class SideToggleManager
	{
		private readonly IDockLayoutService _layoutService;
		private readonly Dictionary<ToolboxSide, List<IToolbox>> _lastOpenBySide = new Dictionary<ToolboxSide, List<IToolbox>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="SideToggleManager"/> class.
		/// </summary>
		/// <param name="layoutService">The dock layout service providing anchorable access.</param>
		public SideToggleManager(IDockLayoutService layoutService)
		{
			_layoutService = layoutService;
		}

		/// <summary>
		/// Toggles all anchorables on the specified side.
		/// If any are open, all are hidden (and their identities are remembered).
		/// If none are open, the previously open toolboxes are restored; if no
		/// history exists, the first available toolbox is shown.
		/// </summary>
		/// <param name="side">The side to toggle.</param>
		public void Toggle(ToolboxSide side)
		{
			var toolboxes = _layoutService.Anchorables
				.OfType<IToolbox>()
				.Where(t => t.Zone.ToSide() == side)
				.ToList();

			if (toolboxes.Count == 0)
			{
				return;
			}

			bool anyOpen = toolboxes.Any(t => t.IsOpen);

			if (anyOpen)
			{
				_lastOpenBySide[side] = toolboxes.Where(t => t.IsOpen).ToList();

				foreach (var t in toolboxes)
				{
					t.IsOpen = false;
				}
			}
			else
			{
				if (_lastOpenBySide.TryGetValue(side, out var lastOpen) && lastOpen.Count > 0)
				{
					foreach (var t in lastOpen)
					{
						t.IsOpen = true;
					}
				}
				else
				{
					toolboxes[0].IsOpen = true;
				}
			}
		}
	}
}