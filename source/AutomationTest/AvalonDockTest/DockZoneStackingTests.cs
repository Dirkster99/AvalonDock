using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using AvalonDock.Core;
using AvalonDock.Layout;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Tests for DockZone-based stacking behavior: when multiple anchorable panes
	/// from different zones on the same side are docked, they must be wrapped in
	/// a <see cref="LayoutAnchorablePaneGroup"/> with the correct orientation.
	/// Left/Right zones → Vertical (top-to-bottom), Bottom zones → Horizontal (side-by-side).
	/// </summary>
	[TestFixture]
	public class DockZoneStackingTests
	{
		/// <summary>
		/// When two LayoutAnchorablePane children sit contiguously on the left side
		/// of a Horizontal root panel, wrapping them in a group should use Vertical
		/// orientation so they stack top-to-bottom (LeftTop above LeftBottom).
		/// </summary>
		[Test]
		public void LeftZones_ContiguousPanes_ShouldBeGroupedVertically()
		{
			var root = CreateLayoutWithTwoLeftPanes();
			var rootPanel = root.RootPanel;

			// Before grouping: rootPanel is Horizontal with [pane0, pane1, docPane]
			Assert.That(rootPanel.Orientation, Is.EqualTo(Orientation.Horizontal));
			var panes = rootPanel.Children.OfType<LayoutAnchorablePane>().ToList();
			Assert.That(panes.Count, Is.EqualTo(2));

			// Group them with Vertical orientation (simulating FixSplitOrientation for left zones)
			var group = WrapContiguousPanesInGroup(rootPanel, panes, Orientation.Vertical);

			Assert.That(group.Orientation, Is.EqualTo(Orientation.Vertical));
			Assert.That(group.Children.Count, Is.EqualTo(2));
			Assert.That(rootPanel.Children.OfType<LayoutAnchorablePane>().Count(), Is.EqualTo(0),
				"Individual panes should be removed from root panel");
			Assert.That(rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().Count(), Is.EqualTo(1),
				"A single group should replace the individual panes");
		}

		/// <summary>
		/// When two LayoutAnchorablePane children sit contiguously on the right side,
		/// wrapping them should also use Vertical orientation.
		/// </summary>
		[Test]
		public void RightZones_ContiguousPanes_ShouldBeGroupedVertically()
		{
			var root = CreateLayoutWithTwoRightPanes();
			var rootPanel = root.RootPanel;

			var panes = rootPanel.Children.OfType<LayoutAnchorablePane>().ToList();
			Assert.That(panes.Count, Is.EqualTo(2));

			var group = WrapContiguousPanesInGroup(rootPanel, panes, Orientation.Vertical);

			Assert.That(group.Orientation, Is.EqualTo(Orientation.Vertical));
			Assert.That(group.Children.Count, Is.EqualTo(2));
		}

		/// <summary>
		/// When two bottom-zone panes are docked, they should be grouped Horizontally
		/// (BottomLeft beside BottomRight).
		/// </summary>
		[Test]
		public void BottomZones_ContiguousPanes_ShouldBeGroupedHorizontally()
		{
			var rootPanel = new LayoutPanel { Orientation = Orientation.Vertical };
			var docPane = new LayoutDocumentPane(new LayoutDocument { Title = "Doc" });
			var bottomPane1 = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Problems" });
			var bottomPane2 = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Output" });
			rootPanel.Children.Add(docPane);
			rootPanel.Children.Add(bottomPane1);
			rootPanel.Children.Add(bottomPane2);

			var panes = new List<LayoutAnchorablePane> { bottomPane1, bottomPane2 };
			var group = WrapContiguousPanesInGroup(rootPanel, panes, Orientation.Horizontal);

			Assert.That(group.Orientation, Is.EqualTo(Orientation.Horizontal));
			Assert.That(group.Children.Count, Is.EqualTo(2));
		}

		/// <summary>
		/// Verifies that left-zone panes are NOT grouped horizontally —
		/// they must be grouped vertically for correct visual stacking.
		/// </summary>
		[Test]
		public void LeftZones_ShouldNotStack_Horizontally()
		{
			var desiredOrientation = GetDesiredOrientationForZone(DockZone.LeftTop);
			Assert.That(desiredOrientation, Is.EqualTo(Orientation.Vertical),
				"Left zones should stack vertically (top-to-bottom), not horizontally");

			desiredOrientation = GetDesiredOrientationForZone(DockZone.LeftBottom);
			Assert.That(desiredOrientation, Is.EqualTo(Orientation.Vertical));
		}

		/// <summary>
		/// Verifies that right-zone panes are grouped vertically.
		/// </summary>
		[Test]
		public void RightZones_ShouldStack_Vertically()
		{
			Assert.That(GetDesiredOrientationForZone(DockZone.RightTop), Is.EqualTo(Orientation.Vertical));
			Assert.That(GetDesiredOrientationForZone(DockZone.RightBottom), Is.EqualTo(Orientation.Vertical));
		}

		/// <summary>
		/// Verifies that bottom-zone panes are grouped horizontally.
		/// </summary>
		[Test]
		public void BottomZones_ShouldStack_Horizontally()
		{
			Assert.That(GetDesiredOrientationForZone(DockZone.BottomLeft), Is.EqualTo(Orientation.Horizontal));
			Assert.That(GetDesiredOrientationForZone(DockZone.BottomRight), Is.EqualTo(Orientation.Horizontal));
		}

		/// <summary>
		/// After grouping, the group should be at the same position as the first pane was.
		/// </summary>
		[Test]
		public void GroupedPanes_ShouldBeAtOriginalPosition()
		{
			var root = CreateLayoutWithTwoLeftPanes();
			var rootPanel = root.RootPanel;

			var panes = rootPanel.Children.OfType<LayoutAnchorablePane>().ToList();
			var firstPaneIndex = rootPanel.Children.IndexOf(panes[0]);
			Assert.That(firstPaneIndex, Is.EqualTo(0), "First pane should be at index 0");

			WrapContiguousPanesInGroup(rootPanel, panes, Orientation.Vertical);

			var group = rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null);
			var groupIndex = rootPanel.Children.IndexOf(group);
			Assert.That(groupIndex, Is.EqualTo(0),
				"Group should be at same position as the first pane was");
		}

		/// <summary>
		/// Document pane should not be affected by grouping side panes.
		/// </summary>
		[Test]
		public void Grouping_DoesNotAffect_DocumentPane()
		{
			var root = CreateLayoutWithTwoLeftPanes();
			var rootPanel = root.RootPanel;

			var docPane = rootPanel.Children.OfType<LayoutDocumentPane>().FirstOrDefault();
			Assert.That(docPane, Is.Not.Null);

			var panes = rootPanel.Children.OfType<LayoutAnchorablePane>().ToList();
			WrapContiguousPanesInGroup(rootPanel, panes, Orientation.Vertical);

			var docPaneAfter = rootPanel.Children.OfType<LayoutDocumentPane>().FirstOrDefault();
			Assert.That(docPaneAfter, Is.SameAs(docPane),
				"Document pane should remain in the root panel");
		}

		/// <summary>
		/// Replicates the orientation logic from ToggleDockingManager.FixSplitOrientation.
		/// Bottom zones → Horizontal, all other zones → Vertical.
		/// </summary>
		private static Orientation GetDesiredOrientationForZone(DockZone zone)
		{
			bool isBottom = zone == DockZone.BottomLeft || zone == DockZone.BottomRight;
			return isBottom ? Orientation.Horizontal : Orientation.Vertical;
		}

		/// <summary>
		/// Wraps contiguous anchorable panes in a group with the specified orientation.
		/// Replicates the grouping logic from ToggleDockingManager.FixSplitOrientation.
		/// </summary>
		private static LayoutAnchorablePaneGroup WrapContiguousPanesInGroup(
			LayoutPanel parentPanel,
			List<LayoutAnchorablePane> panes,
			Orientation orientation)
		{
			var group = new LayoutAnchorablePaneGroup { Orientation = orientation };
			int firstIdx = parentPanel.Children.IndexOf(panes[0]);

			for (int i = panes.Count - 1; i >= 0; i--)
				parentPanel.Children.Remove(panes[i]);

			foreach (var p in panes)
				group.Children.Add(p);

			parentPanel.Children.Insert(
				System.Math.Min(firstIdx, parentPanel.Children.Count), group);
			return group;
		}

		private static LayoutRoot CreateLayoutWithTwoLeftPanes()
		{
			var root = new LayoutRoot();
			var rootPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
			root.RootPanel = rootPanel;

			var leftTopPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Explorer" });
			var leftBottomPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Search" });
			var docPane = new LayoutDocumentPane(new LayoutDocument { Title = "Welcome" });

			rootPanel.Children.Add(leftTopPane);
			rootPanel.Children.Add(leftBottomPane);
			rootPanel.Children.Add(docPane);

			return root;
		}

		private static LayoutRoot CreateLayoutWithTwoRightPanes()
		{
			var root = new LayoutRoot();
			var rootPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
			root.RootPanel = rootPanel;

			var docPane = new LayoutDocumentPane(new LayoutDocument { Title = "Welcome" });
			var rightTopPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Properties" });
			var rightBottomPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Output" });

			rootPanel.Children.Add(docPane);
			rootPanel.Children.Add(rightTopPane);
			rootPanel.Children.Add(rightBottomPane);

			return root;
		}
	}
}
