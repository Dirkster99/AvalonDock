using AvalonDock.Controls;
using NUnit.Framework;

namespace AvalonDockTest
{
	[TestFixture]
	public class OverlayPreviewRulesTests
	{
		private const double PaneWidth = 400.0;
		private const double PaneHeight = 300.0;

		[Test]
		public void Pane_DockLeft_TakesLeftHalf()
		{
			var ok = OverlayPreviewRules.TryComputePanePreviewRect(
				DropTargetType.DocumentPaneDockLeft, PaneWidth, PaneHeight,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(PaneWidth / 2.0));
			Assert.That(height, Is.EqualTo(PaneHeight));
		}

		[Test]
		public void Pane_DockRight_TakesRightHalf()
		{
			var ok = OverlayPreviewRules.TryComputePanePreviewRect(
				DropTargetType.AnchorablePaneDockRight, PaneWidth, PaneHeight,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(left, Is.EqualTo(PaneWidth / 2.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(PaneWidth / 2.0));
			Assert.That(height, Is.EqualTo(PaneHeight));
		}

		[Test]
		public void Pane_DockTop_TakesTopHalf()
		{
			var ok = OverlayPreviewRules.TryComputePanePreviewRect(
				DropTargetType.DocumentPaneDockTop, PaneWidth, PaneHeight,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(PaneWidth));
			Assert.That(height, Is.EqualTo(PaneHeight / 2.0));
		}

		[Test]
		public void Pane_DockBottom_TakesBottomHalf()
		{
			var ok = OverlayPreviewRules.TryComputePanePreviewRect(
				DropTargetType.AnchorablePaneDockBottom, PaneWidth, PaneHeight,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(PaneHeight / 2.0));
			Assert.That(width, Is.EqualTo(PaneWidth));
			Assert.That(height, Is.EqualTo(PaneHeight / 2.0));
		}

		[TestCase(DropTargetType.DocumentPaneDockInside)]
		[TestCase(DropTargetType.AnchorablePaneDockInside)]
		[TestCase(DropTargetType.DocumentPaneGroupDockInside)]
		public void Pane_DockInside_TakesFullPane(DropTargetType type)
		{
			var ok = OverlayPreviewRules.TryComputePanePreviewRect(
				type, PaneWidth, PaneHeight,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(PaneWidth));
			Assert.That(height, Is.EqualTo(PaneHeight));
		}

		[TestCase(DropTargetType.DockingManagerDockLeft)]
		[TestCase(DropTargetType.DocumentPaneDockAsAnchorableLeft)]
		public void Pane_UnrecognisedType_ReturnsFalseWithFullPaneDefaults(DropTargetType type)
		{
			var ok = OverlayPreviewRules.TryComputePanePreviewRect(
				type, PaneWidth, PaneHeight,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.False);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(PaneWidth));
			Assert.That(height, Is.EqualTo(PaneHeight));
		}

		private const double AreaWidth = 1000.0;
		private const double AreaHeight = 800.0;

		[Test]
		public void Manager_DockLeft_PreferredSmallerThanHalf_UsesPreferred()
		{
			var ok = OverlayPreviewRules.TryComputeManagerPreviewRect(
				DropTargetType.DockingManagerDockLeft, AreaWidth, AreaHeight, 200.0,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(200.0));
			Assert.That(height, Is.EqualTo(AreaHeight));
		}

		[Test]
		public void Manager_DockLeft_PreferredLargerThanHalf_ClampsToHalf()
		{
			var ok = OverlayPreviewRules.TryComputeManagerPreviewRect(
				DropTargetType.DockingManagerDockLeft, AreaWidth, AreaHeight, 900.0,
				out _, out _, out var width, out _);

			Assert.That(ok, Is.True);
			Assert.That(width, Is.EqualTo(AreaWidth / 2.0));
		}

		[Test]
		public void Manager_DockRight_AnchorsToRightEdge()
		{
			var ok = OverlayPreviewRules.TryComputeManagerPreviewRect(
				DropTargetType.DockingManagerDockRight, AreaWidth, AreaHeight, 200.0,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(width, Is.EqualTo(200.0));
			Assert.That(left, Is.EqualTo(AreaWidth - 200.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(height, Is.EqualTo(AreaHeight));
		}

		[Test]
		public void Manager_DockTop_UsesPreferredHeight()
		{
			var ok = OverlayPreviewRules.TryComputeManagerPreviewRect(
				DropTargetType.DockingManagerDockTop, AreaWidth, AreaHeight, 150.0,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(AreaWidth));
			Assert.That(height, Is.EqualTo(150.0));
		}

		[Test]
		public void Manager_DockBottom_AnchorsToBottomEdge()
		{
			var ok = OverlayPreviewRules.TryComputeManagerPreviewRect(
				DropTargetType.DockingManagerDockBottom, AreaWidth, AreaHeight, 150.0,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.True);
			Assert.That(height, Is.EqualTo(150.0));
			Assert.That(top, Is.EqualTo(AreaHeight - 150.0));
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(AreaWidth));
		}

		[Test]
		public void Manager_UnrecognisedType_ReturnsFalseWithFullAreaDefaults()
		{
			var ok = OverlayPreviewRules.TryComputeManagerPreviewRect(
				DropTargetType.DocumentPaneDockInside, AreaWidth, AreaHeight, 150.0,
				out var left, out var top, out var width, out var height);

			Assert.That(ok, Is.False);
			Assert.That(left, Is.EqualTo(0.0));
			Assert.That(top, Is.EqualTo(0.0));
			Assert.That(width, Is.EqualTo(AreaWidth));
			Assert.That(height, Is.EqualTo(AreaHeight));
		}
	}

	[TestFixture]
	public class OverlayTabTargetRulesTests
	{
		[Test]
		public void ShouldUseAsTrailingTabCandidate_NoCurrentCandidate_ReturnsTrue()
		{
			Assert.That(OverlayTabTargetRules.ShouldUseAsTrailingTabCandidate(null, 50.0), Is.True);
		}

		[Test]
		public void ShouldUseAsTrailingTabCandidate_CandidateFurtherRight_ReturnsTrue()
		{
			Assert.That(OverlayTabTargetRules.ShouldUseAsTrailingTabCandidate(40.0, 50.0), Is.True);
		}

		[TestCase(50.0)]
		[TestCase(60.0)]
		public void ShouldUseAsTrailingTabCandidate_CandidateNotFurtherRight_ReturnsFalse(double current)
		{
			Assert.That(OverlayTabTargetRules.ShouldUseAsTrailingTabCandidate(current, 50.0), Is.False);
		}

		[Test]
		public void TryComputeTrailingTabDropArea_FitsWithinPane_ReturnsAppendArea()
		{
			// Last tab spans [100,160] (width 60); pane right edge at 400 leaves room to append.
			var ok = OverlayTabTargetRules.TryComputeTrailingTabDropArea(
				100.0, 5.0, 160.0, 30.0, 400.0,
				out var areaLeft, out var areaTop, out var areaRight, out var areaBottom);

			Assert.That(ok, Is.True);
			Assert.That(areaLeft, Is.EqualTo(160.0));
			Assert.That(areaTop, Is.EqualTo(5.0));
			Assert.That(areaRight, Is.EqualTo(220.0));
			Assert.That(areaBottom, Is.EqualTo(30.0));
		}

		[TestCase(160.0, 160.0)] // zero width
		[TestCase(170.0, 160.0)] // negative width
		public void TryComputeTrailingTabDropArea_NonPositiveWidth_ReturnsFalse(double tabLeft, double tabRight)
		{
			var ok = OverlayTabTargetRules.TryComputeTrailingTabDropArea(
				tabLeft, 5.0, tabRight, 30.0, 400.0,
				out var areaLeft, out var areaTop, out var areaRight, out var areaBottom);

			Assert.That(ok, Is.False);
			Assert.That(areaLeft, Is.EqualTo(0.0));
			Assert.That(areaTop, Is.EqualTo(0.0));
			Assert.That(areaRight, Is.EqualTo(0.0));
			Assert.That(areaBottom, Is.EqualTo(0.0));
		}

		[Test]
		public void TryComputeTrailingTabDropArea_AppendWouldExceedPane_ReturnsFalse()
		{
			// Last tab spans [100,160] (width 60); append would reach 220, but pane right is 200.
			var ok = OverlayTabTargetRules.TryComputeTrailingTabDropArea(
				100.0, 5.0, 160.0, 30.0, 200.0,
				out _, out _, out _, out _);

			Assert.That(ok, Is.False);
		}
	}
}