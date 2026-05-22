using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
		/// When BottomLeft pane is inserted into an existing BottomRight group,
		/// the BottomLeft pane must be first (at the start) of the horizontal group.
		/// This is the Case 1 (existing group neighbor) scenario.
		/// </summary>
		[Test]
		public void BottomLeft_IntoExistingBottomRightGroup_ShouldInsertAtStart()
		{
			var rootPanel = new LayoutPanel { Orientation = Orientation.Vertical };
			var root = new LayoutRoot { RootPanel = rootPanel };
			var docPane = new LayoutDocumentPane(new LayoutDocument { Title = "Doc" });
			rootPanel.Children.Add(docPane);

			// Create existing horizontal group with two BottomRight panes
			var rightPane1 = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Output" });
			var rightPane2 = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Debug" });
			var existingGroup = new LayoutAnchorablePaneGroup { Orientation = Orientation.Horizontal };
			existingGroup.Children.Add(rightPane1);
			existingGroup.Children.Add(rightPane2);
			rootPanel.Children.Add(existingGroup);

			// Insert BottomLeft pane next to the group
			var leftPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Terminal" });
			rootPanel.Children.Insert(rootPanel.Children.IndexOf(existingGroup), leftPane);

			// FixSplitOrientation should insert Terminal at the START of the group
			var terminalAnchorable = leftPane.Children.OfType<LayoutAnchorable>().First();
			_engine.FixSplitOrientationForZone(terminalAnchorable, DockZone.BottomLeft);

			// Terminal (BottomLeft) should be first in the group
			Assert.That(existingGroup.Children.Count, Is.EqualTo(3));
			Assert.That(existingGroup.Children[0], Is.SameAs(leftPane),
				"BottomLeft pane must be first in the horizontal group");
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

		/// <summary>
		/// Reproduces the user scenario: open BottomRight first (full width),
		/// then open BottomLeft. Verify BottomLeft appears BEFORE BottomRight
		/// in the resulting horizontal group.
		/// </summary>
		[Test]
		public void BottomRight_ThenBottomLeft_ShouldPreserveOrder()
		{
			// Step 1: Start with document-only layout
			var root = CreateDocumentOnlyLayout();

			// Step 2: Insert Problems at BottomRight (like ToggleAnchorable)
			var problemsPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Problems" });
			_engine.InsertPaneForZone(root, problemsPane, DockZone.BottomRight);

			// After step 2: V:[H:[Doc], ProblemsPane]
			Assert.That(root.RootPanel.Orientation, Is.EqualTo(Orientation.Vertical),
				"After inserting bottom pane, root should be Vertical");
			Assert.That(root.RootPanel.Children[root.RootPanel.Children.Count - 1], Is.SameAs(problemsPane),
				"Problems should be at the end");

			// Step 3: Insert Terminal at BottomLeft
			var terminalPane = new LayoutAnchorablePane(new LayoutAnchorable { Title = "Terminal" });
			_engine.InsertPaneForZone(root, terminalPane, DockZone.BottomLeft);

			// After step 3: V:[H:[Doc], TerminalPane, ProblemsPane]
			var rootPanel = root.RootPanel;
			int termIdx = rootPanel.Children.IndexOf(terminalPane);
			int probIdx = rootPanel.Children.IndexOf(problemsPane);
			Assert.That(termIdx, Is.LessThan(probIdx),
				$"Terminal (BottomLeft) at index {termIdx} should be before Problems (BottomRight) at index {probIdx}");

			// Step 4: FixSplitOrientation for Terminal (the last inserted)
			var terminalAnchorable = terminalPane.Children.OfType<LayoutAnchorable>().First();
			_engine.FixSplitOrientationForZone(terminalAnchorable, DockZone.BottomLeft);

			// After grouping: V:[H:[Doc], H:[TerminalPane, ProblemsPane]]
			var group = rootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null, "Panes should be grouped");
			Assert.That(group.Orientation, Is.EqualTo(Orientation.Horizontal));
			Assert.That(group.Children.Count, Is.EqualTo(2));

			// The KEY assertion: BottomLeft (Terminal) must be first, BottomRight (Problems) second
			Assert.That(group.Children[0], Is.SameAs(terminalPane),
				"Terminal (BottomLeft) must be first in the horizontal group");
			Assert.That(group.Children[1], Is.SameAs(problemsPane),
				"Problems (BottomRight) must be second in the horizontal group");
		}

		/// <summary>
		/// Full integration test simulating the DockFromAutoHide flow:
		/// Problems dragged to BottomRight then Terminal dragged to BottomLeft.
		/// Verifies that BottomLeft appears before BottomRight in the final group.
		/// </summary>
		[Test]
		public void FullDockFlow_BottomRight_ThenBottomLeft_ShouldPreserveOrder()
		{
			// Setup: doc-only layout with auto-hidden anchorables
			var root = new LayoutRoot();
			var rootPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
			rootPanel.Children.Add(new LayoutDocumentPane(new LayoutDocument { Title = "Welcome" }));
			root.RootPanel = rootPanel;

			var problems = new LayoutAnchorable { Title = "Problems", AutoHideMinHeight = 100 };
			var terminal = new LayoutAnchorable { Title = "Terminal", AutoHideMinHeight = 100 };

			// Put both in BottomSide auto-hide (simulating app startup)
			var problemsGroup = new LayoutAnchorGroup();
			problemsGroup.Children.Add(problems);
			root.BottomSide.Children.Add(problemsGroup);

			var terminalGroup = new LayoutAnchorGroup();
			terminalGroup.Children.Add(terminal);
			root.BottomSide.Children.Add(terminalGroup);

			Assert.That(problems.IsAutoHidden, Is.True, "Problems should start auto-hidden");
			Assert.That(terminal.IsAutoHidden, Is.True, "Terminal should start auto-hidden");

			// ── Step 1: Simulate DockFromAutoHide(Problems, BottomRight) ──
			var problemsPane = new LayoutAnchorablePane
			{
				DockMinHeight = problems.AutoHideMinHeight,
				DockHeight = new GridLength(200)
			};
			_engine.InsertPaneForZone(root, problemsPane, DockZone.BottomRight);
			problemsGroup.Children.Remove(problems);
			problemsPane.Children.Add(problems);
			root.BottomSide.Children.Remove(problemsGroup);

			// After step 1: V:[H:[DocPane], ProblemsPane]
			Assert.That(root.RootPanel.Orientation, Is.EqualTo(Orientation.Vertical));
			Assert.That(problems.IsAutoHidden, Is.False, "Problems should be docked now");

			// FixSplitOrientation (should be no-op with single pane)
			_engine.FixSplitOrientationForZone(problems, DockZone.BottomRight);
			_engine.EnsureBottomFullWidth(root);

			// ── Step 2: Simulate MoveAnchorableToZone(Terminal, BottomLeft) ──
			// MoveAnchorableToZone creates a fresh group
			terminalGroup.Children.Remove(terminal);
			root.BottomSide.Children.Remove(terminalGroup);
			var freshGroup = new LayoutAnchorGroup();
			root.BottomSide.Children.Add(freshGroup);
			freshGroup.Children.Add(terminal);

			Assert.That(terminal.IsAutoHidden, Is.True, "Terminal should still be auto-hidden");

			// Simulate DockFromAutoHide(Terminal, BottomLeft)
			var terminalParentGroup = terminal.Parent as LayoutAnchorGroup;
			Assert.That(terminalParentGroup, Is.Not.Null);
			var prevContainer = ((ILayoutPreviousContainer)terminalParentGroup).PreviousContainer as LayoutAnchorablePane;
			Assert.That(prevContainer, Is.Null, "Fresh group should have no PreviousContainer");

			var terminalPane = new LayoutAnchorablePane
			{
				DockMinHeight = terminal.AutoHideMinHeight,
				DockHeight = new GridLength(200)
			};
			_engine.InsertPaneForZone(root, terminalPane, DockZone.BottomLeft);
			freshGroup.Children.Remove(terminal);
			terminalPane.Children.Add(terminal);
			root.BottomSide.Children.Remove(freshGroup);

			// Verify intermediate tree: V:[H:[DocPane], TerminalPane, ProblemsPane]
			var rp = root.RootPanel;
			int termIdx = rp.Children.IndexOf(terminalPane);
			int probIdx = rp.Children.IndexOf(problemsPane);
			Assert.That(termIdx, Is.GreaterThan(-1), "TerminalPane should be in root panel");
			Assert.That(probIdx, Is.GreaterThan(-1), "ProblemsPane should be in root panel");
			Assert.That(termIdx, Is.LessThan(probIdx),
				$"Before grouping: Terminal at {termIdx} should be before Problems at {probIdx}");

			// FixSplitOrientation for Terminal (BottomLeft)
			_engine.FixSplitOrientationForZone(terminal, DockZone.BottomLeft);

			// EnsureBottomFullWidth
			_engine.EnsureBottomFullWidth(root);

			// Final verification: should be H:[TerminalPane, ProblemsPane] in a group
			var group = root.RootPanel.Children.OfType<LayoutAnchorablePaneGroup>().FirstOrDefault();
			Assert.That(group, Is.Not.Null, "Bottom panes should be grouped");
			Assert.That(group.Orientation, Is.EqualTo(Orientation.Horizontal));
			Assert.That(group.Children.Count, Is.EqualTo(2));
			Assert.That(group.Children[0], Is.SameAs(terminalPane),
				"Terminal (BottomLeft) must be the FIRST child in the horizontal group");
			Assert.That(group.Children[1], Is.SameAs(problemsPane),
				"Problems (BottomRight) must be the SECOND child in the horizontal group");
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
