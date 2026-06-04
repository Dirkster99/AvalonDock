namespace AvalonDock.Controls
{
	/// <summary>
	/// Shared, UI-agnostic tab target geometry rules used by overlay implementations to compute
	/// where a dropped item would be appended relative to the visible tabs of a pane.
	/// </summary>
	/// <remarks>
	/// All methods operate purely on logical pixel coordinates so the same rules can be reused
	/// across WPF and other UI front ends without depending on any presentation framework types.
	/// </remarks>
	public static class OverlayTabTargetRules
	{
		/// <summary>
		/// Determines whether a candidate tab should become the trailing (right-most) tab used to
		/// anchor an append drop area.
		/// </summary>
		/// <param name="currentTrailingRight">The right edge of the current trailing candidate, or <c>null</c> when none has been chosen yet.</param>
		/// <param name="candidateRight">The right edge of the candidate tab being evaluated.</param>
		/// <returns><c>true</c> when the candidate extends further right than the current trailing candidate (or none exists yet); otherwise <c>false</c>.</returns>
		public static bool ShouldUseAsTrailingTabCandidate(double? currentTrailingRight, double candidateRight)
		{
			return !currentTrailingRight.HasValue || candidateRight > currentTrailingRight.Value;
		}

		/// <summary>
		/// Computes the trailing (append) tab drop area immediately after the last visible tab.
		/// </summary>
		/// <param name="lastTabLeft">The left edge of the last visible tab.</param>
		/// <param name="lastTabTop">The top edge of the last visible tab.</param>
		/// <param name="lastTabRight">The right edge of the last visible tab.</param>
		/// <param name="lastTabBottom">The bottom edge of the last visible tab.</param>
		/// <param name="paneRight">The right boundary of the pane that hosts the tabs.</param>
		/// <param name="areaLeft">When this method returns <c>true</c>, the left edge of the computed drop area.</param>
		/// <param name="areaTop">When this method returns <c>true</c>, the top edge of the computed drop area.</param>
		/// <param name="areaRight">When this method returns <c>true</c>, the right edge of the computed drop area.</param>
		/// <param name="areaBottom">When this method returns <c>true</c>, the bottom edge of the computed drop area.</param>
		/// <returns><c>false</c> when the last tab has no width or the computed area would exceed the pane's right boundary; otherwise <c>true</c>.</returns>
		public static bool TryComputeTrailingTabDropArea(
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