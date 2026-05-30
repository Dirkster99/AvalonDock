namespace AvalonDock.Controls
{
	// Shared tab target geometry rules used by overlay implementations.
	internal static class OverlayTabTargetRules
	{
		internal static bool ShouldUseAsTrailingTabCandidate(double? currentTrailingRight, double candidateRight)
		{
			return !currentTrailingRight.HasValue || candidateRight > currentTrailingRight.Value;
		}

		// Computes the trailing (append) tab drop area immediately after the last visible tab.
		// Returns false when the computed area would exceed the pane's right boundary.
		internal static bool TryComputeTrailingTabDropArea(
			double lastTabLeft,
			double lastTabTop,
			double lastTabRight,
			double lastTabBottom,
			double paneRight,
			out double areaLeft,
			out double areaTop,
			out double areaRight,
			out double areaBottom)
		{
			areaLeft = 0;
			areaTop = 0;
			areaRight = 0;
			areaBottom = 0;

			var width = lastTabRight - lastTabLeft;
			if (width <= 0)
				return false;

			var proposedRight = lastTabRight + width;
			if (proposedRight >= paneRight)
				return false;

			areaLeft = lastTabRight;
			areaTop = lastTabTop;
			areaRight = proposedRight;
			areaBottom = lastTabBottom;
			return true;
		}
	}
}
