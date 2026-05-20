using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using AvalonDock;
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
	/// Tests exercise both the <see cref="ILayoutEngine"/> interface and
	/// <see cref="ToggleLayoutEngine"/> zone-specific methods.
	/// </summary>
	[TestFixture]
	public class DockZoneStackingTests
	{
		private ToggleLayoutEngine _engine;

		/// <summary>
		/// Creates a new <see cref="ToggleLayoutEngine"/> instance for each test.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			_engine = new ToggleLayoutEngine();
		}

		// ────────────────────────────────────────────────────────────
		//  ILayoutEngine.GetDesiredOrientation tests
		// ────────────────────────────────────────────────────────────

		/// <summary>
		/// Verifies that left-zone panes are NOT grouped horizontally —
		/// they must be grouped vertically for correct visual stacking.
		/// </summary>
		[Test]
		public void LeftZones_ShouldNotStack_Horizontally()
		{
			Assert.That(ToggleLayoutEngine.GetDesiredOrientationForZone(DockZone.LeftTop), Is.EqualTo(Orientation.Vertical),
				"Left zones should stack vertically (top-to-bottom), not horizontally");
			Assert.That(ToggleLayoutEngine.GetDesiredOrientationForZone(DockZone.LeftBottom), Is.EqualTo(Orientation.Vertical));
		}

		/// <summary>
		/// Verifies that right-zone panes are grouped vertically.
		/// </summary>
		[Test]
		public void RightZones_ShouldStack_Vertically()
		{
			Assert.That(ToggleLayoutEngine.GetDesiredOrientationForZone(DockZone.RightTop), Is.EqualTo(Orientation.Vertical));
			Assert.That(ToggleLayoutEngine.GetDesiredOrientationForZone(DockZone.RightBottom), Is.EqualTo(Orientation.Vertical));
		}

		/// <summary>
		/// Verifies that bottom-zone panes are grouped horizontally.
		/// </summary>
		[Test]
		public void BottomZones_ShouldStack_Horizontally()
		{
			Assert.That(ToggleLayoutEngine.GetDesiredOrientationForZone(DockZone.BottomLeft), Is.EqualTo(Orientation.Horizontal));
			Assert.That(ToggleLayoutEngine.GetDesiredOrientationForZone(DockZone.BottomRight), Is.EqualTo(Orientation.Horizontal));
		}

		/// <summary>
		/// Verifies <see cref="ILayoutEngine.GetDesiredOrientation"/> returns matching values for AnchorSide.
		/// </summary>
		[Test]
		public void ILayoutEngine_GetDesiredOrientation_MatchesSides()
		{
			ILayoutEngine engine = _engine;
			Assert.That(engine.GetDesiredOrientation(AnchorSide.Left), Is.EqualTo(Orientation.Vertical));
			Assert.That(engine.GetDesiredOrientation(AnchorSide.Right), Is.EqualTo(Orientation.Vertical));
			Assert.That(engine.GetDesiredOrientation(AnchorSide.Top), Is.EqualTo(Orientation.Horizontal));
			Assert.That(engine.GetDesiredOrientation(AnchorSide.Bottom), Is.EqualTo(Orientation.Horizontal));
		}

		// ────────────────────────────────────────────────────────────
		//  InsertPaneForZone tests
		// ────────────────────────────────────────────────────────────

		/// <summary>
		/// InsertPaneForZone(LeftTop) inserts pane at index 0 of the horizontal content panel.
		/// </summary>
		[Test]
		public void InsertPaneForZone_LeftTop_InsertsAtStart()
		{
			var root = CreateDocumentOnlyLayout();
			var pane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Explorer" });

			_engine.InsertPaneForZone(root, pane, DockZone.LeftTop);

			var hPanel = root.RootPanel;
			Assert.That(hPanel.Children[0], Is.SameAs(pane));
		}

		/// <summary>
		/// InsertPaneForZone(Bottom) creates a Vertical wrapper around the root when needed.
		/// </summary>
		[Test]
		public void InsertPaneForZone_Bottom_CreatesVerticalWrapper()
		{
			var root = CreateDocumentOnlyLayout();
			var pane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Terminal" });

			_engine.InsertPaneForZone(root, pane, DockZone.BottomLeft);

			Assert.That(root.RootPanel.Orientation, Is.EqualTo(Orientation.Vertical));
			Assert.That(root.RootPanel.Children[root.RootPanel.Children.Count - 1], Is.SameAs(pane));
		}

		// ────────────────────────────────────────────────────────────
		//  ILayoutEngine.InsertPane (AnchorSide) tests
		// ────────────────────────────────────────────────────────────

		/// <summary>
		/// ILayoutEngine.InsertPane with Left side inserts at position 0.
		/// </summary>
		[Test]
		public void ILayoutEngine_InsertPane_Left_InsertsAtStart()
		{
			ILayoutEngine engine = _engine;
			var root = CreateDocumentOnlyLayout();
			var pane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Explorer" });

			engine.InsertPane(root, pane, AnchorSide.Left);

			var hPanel = root.RootPanel;
			Assert.That(hPanel.Children[0], Is.SameAs(pane));
		}

		/// <summary>
		/// ILayoutEngine.InsertPane with Right side appends to the end.
		/// </summary>
		[Test]
		public void ILayoutEngine_InsertPane_Right_AppendsToEnd()
		{
			ILayoutEngine engine = _engine;
			var root = CreateDocumentOnlyLayout();
			var pane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Properties" });

			engine.InsertPane(root, pane, AnchorSide.Right);

			var hPanel = root.RootPanel;
			Assert.That(hPanel.Children[hPanel.Children.Count - 1], Is.SameAs(pane));
		}

		// ────────────────────────────────────────────────────────────
		//  FixSplitOrientation tests
		// ────────────────────────────────────────────────────────────

		/// <summary>
		/// When two LayoutAnchorablePane children sit contiguously on the left side
		/// of a Horizontal root panel, FixSplitOrientationForZone should wrap them
		/// in a group with Vertical orientation (top-to-bottom stacking).
		/// </summary>
		[Test]
		public void LeftZones_ContiguousPanes_ShouldBeGroupedVertically()
		{
			var root = CreateLayoutWithTwoLeftPanes();
			var rootPanel = root.RootPanel;
			var secondPaneAnchorable = rootPanel.Children.OfType<LayoutAnchorablePane>().Last()
				.Children.OfType<LayoutAnchorable>().First();

			_engine.FixSplitOrientationForZone(secondPaneAnchorable, DockZone.LeftBottom);

			var group = rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null, "Contiguous panes should be wrapped in a group");
			Assert.That(group.Orientation, Is.EqualTo(Orientation.Vertical));
			Assert.That(group.Children.Count, Is.EqualTo(2));
		}

		/// <summary>
		/// When two LayoutAnchorablePane children sit contiguously on the right side,
		/// FixSplitOrientationForZone should also use Vertical orientation.
		/// </summary>
		[Test]
		public void RightZones_ContiguousPanes_ShouldBeGroupedVertically()
		{
			var root = CreateLayoutWithTwoRightPanes();
			var rootPanel = root.RootPanel;
			var secondPaneAnchorable = rootPanel.Children.OfType<LayoutAnchorablePane>().Last()
				.Children.OfType<LayoutAnchorable>().First();

			_engine.FixSplitOrientationForZone(secondPaneAnchorable, DockZone.RightBottom);

			var group = rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null);
			Assert.That(group.Orientation, Is.EqualTo(Orientation.Vertical));
			Assert.That(group.Children.Count, Is.EqualTo(2));
		}

		/// <summary>
		/// When two bottom-zone panes are docked, FixSplitOrientationForZone should
		/// group them Horizontally (BottomLeft beside BottomRight).
		/// </summary>
		[Test]
		public void BottomZones_ContiguousPanes_ShouldBeGroupedHorizontally()
		{
			var rootPanel = new LayoutPanel { Orientation = Orientation.Vertical };
			var root = new LayoutRoot { RootPanel = rootPanel };
			var docPane = new LayoutDocumentPane(new LayoutDocument { Title = "Doc" });
			var bottomPane1 = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Problems" });
			var bottomPane2 = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Output" });
			rootPanel.Children.Add(docPane);
			rootPanel.Children.Add(bottomPane1);
			rootPanel.Children.Add(bottomPane2);

			var anchorable = bottomPane2.Children.OfType<LayoutAnchorable>().First();
			_engine.FixSplitOrientationForZone(anchorable, DockZone.BottomRight);

			// Root is already Vertical with desired orientation Horizontal, so grouping should happen
			var group = rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null);
			Assert.That(group.Orientation, Is.EqualTo(Orientation.Horizontal));
			Assert.That(group.Children.Count, Is.EqualTo(2));
		}

		/// <summary>
		/// After grouping, the group should be at the same position as the first pane was.
		/// </summary>
		[Test]
		public void GroupedPanes_ShouldBeAtOriginalPosition()
		{
			var root = CreateLayoutWithTwoLeftPanes();
			var rootPanel = root.RootPanel;
			var secondPaneAnchorable = rootPanel.Children.OfType<LayoutAnchorablePane>().Last()
				.Children.OfType<LayoutAnchorable>().First();

			_engine.FixSplitOrientationForZone(secondPaneAnchorable, DockZone.LeftBottom);

			var group = rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null);
			Assert.That(rootPanel.Children.IndexOf(group), Is.EqualTo(0),
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

			var secondPaneAnchorable = rootPanel.Children.OfType<LayoutAnchorablePane>().Last()
				.Children.OfType<LayoutAnchorable>().First();

			_engine.FixSplitOrientationForZone(secondPaneAnchorable, DockZone.LeftBottom);

			var docPaneAfter = rootPanel.Children.OfType<LayoutDocumentPane>().FirstOrDefault();
			Assert.That(docPaneAfter, Is.SameAs(docPane),
				"Document pane should remain in the root panel");
		}

		// ────────────────────────────────────────────────────────────
		//  ILayoutEngine.FixSplitOrientation (AnchorSide) tests
		// ────────────────────────────────────────────────────────────

		/// <summary>
		/// The <see cref="ILayoutEngine.FixSplitOrientation"/> method groups left-side
		/// panes vertically, just like the zone variant.
		/// </summary>
		[Test]
		public void ILayoutEngine_FixSplitOrientation_Left_GroupsVertically()
		{
			ILayoutEngine engine = _engine;
			var root = CreateLayoutWithTwoLeftPanes();
			var rootPanel = root.RootPanel;
			var secondPaneAnchorable = rootPanel.Children.OfType<LayoutAnchorablePane>().Last()
				.Children.OfType<LayoutAnchorable>().First();

			engine.FixSplitOrientation(secondPaneAnchorable, AnchorSide.Left);

			var group = rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null);
			Assert.That(group.Orientation, Is.EqualTo(Orientation.Vertical));
		}

		// ────────────────────────────────────────────────────────────
		//  FindOrCreateContentPanel tests
		// ────────────────────────────────────────────────────────────

		/// <summary>
		/// When root panel is already Horizontal, FindOrCreateContentPanel returns it directly.
		/// </summary>
		[Test]
		public void FindOrCreateContentPanel_HorizontalRoot_ReturnsSame()
		{
			var root = CreateDocumentOnlyLayout();
			var result = _engine.FindOrCreateContentPanel(root, Orientation.Horizontal);
			Assert.That(result, Is.SameAs(root.RootPanel));
		}

		/// <summary>
		/// When root panel is Vertical, FindOrCreateContentPanel creates a Horizontal child.
		/// </summary>
		[Test]
		public void FindOrCreateContentPanel_VerticalRoot_CreatesHorizontalChild()
		{
			var root = new LayoutRoot();
			var vPanel = new LayoutPanel { Orientation = Orientation.Vertical };
			var docPane = new LayoutDocumentPane(new LayoutDocument { Title = "Doc" });
			var bottomPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Terminal" });
			vPanel.Children.Add(docPane);
			vPanel.Children.Add(bottomPane);
			root.RootPanel = vPanel;

			var result = _engine.FindOrCreateContentPanel(root, Orientation.Horizontal);

			Assert.That(result.Orientation, Is.EqualTo(Orientation.Horizontal));
			Assert.That(result.Parent, Is.SameAs(vPanel));
		}

		// ────────────────────────────────────────────────────────────
		//  DefaultLayoutEngine conformance tests
		// ────────────────────────────────────────────────────────────

		/// <summary>
		/// DefaultLayoutEngine implements ILayoutEngine and inserts panes correctly.
		/// </summary>
		[Test]
		public void DefaultLayoutEngine_InsertPane_Left_InsertsAtStart()
		{
			ILayoutEngine engine = new DefaultLayoutEngine();
			var root = CreateDocumentOnlyLayout();
			var pane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Explorer" });

			engine.InsertPane(root, pane, AnchorSide.Left);

			Assert.That(root.RootPanel.Children[0], Is.SameAs(pane));
		}

		/// <summary>
		/// DefaultLayoutEngine wraps root in vertical panel when inserting bottom pane on horizontal root.
		/// </summary>
		[Test]
		public void DefaultLayoutEngine_InsertPane_Bottom_CreatesVerticalWrapper()
		{
			ILayoutEngine engine = new DefaultLayoutEngine();
			var root = CreateDocumentOnlyLayout();
			var pane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Terminal" });

			engine.InsertPane(root, pane, AnchorSide.Bottom);

			Assert.That(root.RootPanel.Orientation, Is.EqualTo(Orientation.Vertical));
			Assert.That(root.RootPanel.Children[root.RootPanel.Children.Count - 1], Is.SameAs(pane));
		}

		// ────────────────────────────────────────────────────────────
		//  Helpers
		// ────────────────────────────────────────────────────────────

		private static LayoutRoot CreateDocumentOnlyLayout()
		{
			var root = new LayoutRoot();
			var rootPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
			rootPanel.Children.Add(new LayoutDocumentPane(new LayoutDocument { Title = "Welcome" }));
			root.RootPanel = rootPanel;
			return root;
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
