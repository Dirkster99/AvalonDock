using System;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	// Shared preview-rect rules used by both WPF and Uno overlay implementations.
	// All methods operate on logical pixel dimensions and return offsets relative to
	// the top-left of the area being described (pane or DockingManager).
	//
	// WPF canonical rules (from *DropTarget.GetPreviewPath):
	//   Pane Left/Right   → half the pane width
	//   Pane Top/Bottom   → half the pane height
	//   Pane Inside       → full pane
	//   Manager Left/Right → min(preferredDockWidth,  areaWidth  / 2)
	//   Manager Top/Bottom → min(preferredDockHeight, areaHeight / 2)
	internal static class OverlayPreviewRules
	{
		// ── Pane drop targets (WPF: AnchorablePaneDropTarget / DocumentPaneDropTarget) ──

		/// <summary>
		/// Computes the preview rect for a directional pane split or an inside (tab-join) drop.
		/// All output coordinates are relative to the pane's top-left corner.
		/// Returns <c>false</c> when <paramref name="type"/> is not a recognised pane drop target.
		/// </summary>
		internal static bool TryComputePanePreviewRect(
			DropTargetType type,
			double paneWidth,
			double paneHeight,
			out double left,
			out double top,
			out double width,
			out double height)
		{
			left = 0; top = 0; width = paneWidth; height = paneHeight;

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
					left  = paneWidth / 2.0;
					width = paneWidth / 2.0;
					return true;

				// Bottom half
				case DropTargetType.AnchorablePaneDockBottom:
				case DropTargetType.DocumentPaneDockBottom:
					top    = paneHeight / 2.0;
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

		// ── DockingManager outer-edge targets (WPF: DockingManagerDropTarget) ──

		/// <summary>
		/// Computes the preview rect for a DockingManager outer-edge drop.
		/// <paramref name="preferredSize"/> is the floating window's preferred dock width or height
		/// (from DockWidth/DockHeight or ActualWidth/ActualHeight).
		/// All output coordinates are relative to the DockingManager's top-left corner.
		/// Returns <c>false</c> when <paramref name="type"/> is not a recognised manager drop target.
		/// </summary>
		internal static bool TryComputeManagerPreviewRect(
			DropTargetType type,
			double areaWidth,
			double areaHeight,
			double preferredSize,
			out double left,
			out double top,
			out double width,
			out double height)
		{
			left = 0; top = 0; width = areaWidth; height = areaHeight;

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
					left  = areaWidth - width;
					return true;

				case DropTargetType.DockingManagerDockBottom:
					height = Math.Min(preferredSize, areaHeight / 2.0);
					top    = areaHeight - height;
					return true;

				default:
					return false;
			}
		}
	}
}
