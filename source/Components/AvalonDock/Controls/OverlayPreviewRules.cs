using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	internal static class OverlayPreviewRules
	{
		internal const double DefaultSplitFraction = 1.0 / 3.0;

		internal static bool TryComputePreviewRect(
			CompassDropZone zone,
			double managerWidth,
			double managerHeight,
			out double left,
			out double top,
			out double width,
			out double height,
			double splitFraction = DefaultSplitFraction)
		{
			left = 0;
			top = 0;
			width = managerWidth;
			height = managerHeight;

			if (zone == CompassDropZone.None)
				return false;

			switch (zone)
			{
				case CompassDropZone.Left:
				case CompassDropZone.OuterLeft:
					width = managerWidth * splitFraction;
					break;
				case CompassDropZone.Right:
				case CompassDropZone.OuterRight:
					left = managerWidth - managerWidth * splitFraction;
					width = managerWidth * splitFraction;
					break;
				case CompassDropZone.Top:
				case CompassDropZone.OuterTop:
					height = managerHeight * splitFraction;
					break;
				case CompassDropZone.Bottom:
				case CompassDropZone.OuterBottom:
					top = managerHeight - managerHeight * splitFraction;
					height = managerHeight * splitFraction;
					break;
				case CompassDropZone.Center:
				default:
					break;
			}

			return true;
		}
	}
}
