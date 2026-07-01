using System;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Shared, UI-agnostic preview-rect rules used by overlay implementations to describe the
	/// rectangle highlighted while a floating window is dragged over a drop target.
	/// </summary>
	/// <remarks>
	/// All methods operate on logical pixel dimensions and return offsets relative to the
	/// top-left of the area being described (a pane or the docking manager). The WPF canonical
	/// rules these mirror are:
	/// <list type="bullet">
	/// <item><description>Pane Left/Right → half the pane width.</description></item>
	/// <item><description>Pane Top/Bottom → half the pane height.</description></item>
	/// <item><description>Pane Inside → the full pane.</description></item>
	/// <item><description>Manager Left/Right → <c>min(preferredDockWidth, areaWidth / 2)</c>.</description></item>
	/// <item><description>Manager Top/Bottom → <c>min(preferredDockHeight, areaHeight / 2)</c>.</description></item>
	/// </list>
	/// </remarks>
	public static class OverlayPreviewRules
	{
		/// <summary>
		/// Computes the preview rect for a directional pane split or an inside (tab-join) drop.
		/// All output coordinates are relative to the pane's top-left corner.
		/// </summary>
		/// <param name="type">The drop target being previewed.</param>
		/// <param name="paneWidth">The width of the pane.</param>
		/// <param name="paneHeight">The height of the pane.</param>
		/// <param name="left">When this method returns <c>true</c>, the left offset of the preview rect.</param>
		/// <param name="top">When this method returns <c>true</c>, the top offset of the preview rect.</param>
		/// <param name="width">When this method returns <c>true</c>, the width of the preview rect.</param>
		/// <param name="height">When this method returns <c>true</c>, the height of the preview rect.</param>
		/// <returns><c>false</c> when <paramref name="type"/> is not a recognised pane drop target; otherwise <c>true</c>.</returns>
		public static bool TryComputePanePreviewRect(
			DropTargetType type,
			double paneWidth,
			double paneHeight,
			out double left,
			out double top,
			out double width,
			out double height)
		{
			left = 0;
			top = 0;
			width = paneWidth;
			height = paneHeight;

			switch (type)
			{
				// Left half
				case DropTargetType.AnchorablePaneDockLeft:
				case DropTargetType.DocumentPaneDockLeft:
					width = paneWidth / 2.0;
					return true;

				// Top half
				case DropTargetType.AnchorablePaneDockTop:
				case DropTargetType.DocumentPaneDockTop:
					height = paneHeight / 2.0;
					return true;

				// Right half
				case DropTargetType.AnchorablePaneDockRight:
				case DropTargetType.DocumentPaneDockRight:
					left = paneWidth / 2.0;
					width = paneWidth / 2.0;
					return true;

				// Bottom half
				case DropTargetType.AnchorablePaneDockBottom:
				case DropTargetType.DocumentPaneDockBottom:
					top = paneHeight / 2.0;
					height = paneHeight / 2.0;
					return true;

				// Full pane (tab-join)
				case DropTargetType.AnchorablePaneDockInside:
				case DropTargetType.DocumentPaneDockInside:
				case DropTargetType.DocumentPaneGroupDockInside:
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Computes the preview rect for a docking manager outer-edge drop.
		/// All output coordinates are relative to the docking manager's top-left corner.
		/// </summary>
		/// <param name="type">The drop target being previewed.</param>
		/// <param name="areaWidth">The width of the docking manager area.</param>
		/// <param name="areaHeight">The height of the docking manager area.</param>
		/// <param name="preferredSize">The floating window's preferred dock width or height (from its dock size or actual size).</param>
		/// <param name="left">When this method returns <c>true</c>, the left offset of the preview rect.</param>
		/// <param name="top">When this method returns <c>true</c>, the top offset of the preview rect.</param>
		/// <param name="width">When this method returns <c>true</c>, the width of the preview rect.</param>
		/// <param name="height">When this method returns <c>true</c>, the height of the preview rect.</param>
		/// <returns><c>false</c> when <paramref name="type"/> is not a recognised manager drop target; otherwise <c>true</c>.</returns>
		public static bool TryComputeManagerPreviewRect(
			DropTargetType type,
			double areaWidth,
			double areaHeight,
			double preferredSize,
			out double left,
			out double top,
			out double width,
			out double height)
		{
			left = 0;
			top = 0;
			width = areaWidth;
			height = areaHeight;

			switch (type)
			{
				case DropTargetType.DockingManagerDockLeft:
					width = Math.Min(preferredSize, areaWidth / 2.0);
					return true;

				case DropTargetType.DockingManagerDockTop:
					height = Math.Min(preferredSize, areaHeight / 2.0);
					return true;

				case DropTargetType.DockingManagerDockRight:
					width = Math.Min(preferredSize, areaWidth / 2.0);
					left = areaWidth - width;
					return true;

				case DropTargetType.DockingManagerDockBottom:
					height = Math.Min(preferredSize, areaHeight / 2.0);
					top = areaHeight - height;
					return true;

				default:
					return false;
			}
		}
	}
}