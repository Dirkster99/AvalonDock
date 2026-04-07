using NUnit.Framework;
using AvalonDock.Layout;
using System.Linq;
using System.Collections.Generic;

namespace AvalonDockTest
{
	/// <summary>
	/// Unit tests for the layout model tree structure — adding, removing, and
	/// rearranging layout elements without a DockingManager.
	/// Covers issues:
	///   #396 - Max Anchorable Width / Height
	///   #49  - DockWidth and DockHeight not working
	///   #157 - DockWidth and DockHeight reset after building
	///   #37  - Resize behavior of invisible anchorable panels
	///   #471 - Weird IsFloating error at AnchorablePane creation
	///   #112 - Closing Floating Document Container results in Empty Container
	/// </summary>
	[TestFixture]
	public class LayoutModelTests
	{
		/// <summary>
		/// Verifies that DockWidth and DockHeight can be set on panes.
		/// Regression for #49 - DockWidth and DockHeight not working.
		/// </summary>
		[Test]
		public void DockWidthHeight_CanBeSet_Issue49()
		{
			var pane = new LayoutAnchorablePane();
			pane.DockWidth = new System.Windows.GridLength(200);
			pane.DockHeight = new System.Windows.GridLength(150);

			Assert.That(pane.DockWidth.Value, Is.EqualTo(200));
			Assert.That(pane.DockHeight.Value, Is.EqualTo(150));
		}

		/// <summary>
		/// Verifies that DockMinWidth and DockMinHeight work on panes.
		/// Regression for #396 - Max Anchorable Width / Height.
		/// </summary>
		[Test]
		public void DockMinWidthHeight_CanBeSet_Issue396()
		{
			var pane = new LayoutAnchorablePane
			{
				DockMinWidth = 100,
				DockMinHeight = 80
			};

			Assert.That(pane.DockMinWidth, Is.EqualTo(100));
			Assert.That(pane.DockMinHeight, Is.EqualTo(80));
			Assert.That(pane.CalculatedDockMinWidth(), Is.EqualTo(100));
			Assert.That(pane.CalculatedDockMinHeight(), Is.EqualTo(80));
		}

		/// <summary>
		/// Verifies that DockWidth is preserved after adding children.
		/// Regression for #157 - DockWidth/DockHeight reset after building.
		/// </summary>
		[Test]
		public void DockWidth_PreservedAfterAddingChildren_Issue157()
		{
			var pane = new LayoutAnchorablePane();
			pane.DockWidth = new System.Windows.GridLength(300);

			var anchorable = new LayoutAnchorable { Title = "Tool", ContentId = "tool1" };
			pane.Children.Add(anchorable);

			Assert.That(pane.DockWidth.Value, Is.EqualTo(300),
				"DockWidth should be preserved after adding children (Issue #157).");
		}

		/// <summary>
		/// Verifies that a LayoutPanel with Orientation correctly computes min sizes.
		/// </summary>
		[Test]
		public void LayoutPanel_Orientation_MinSizeCalculation()
		{
			var pane1 = new LayoutAnchorablePane { DockMinWidth = 100, DockMinHeight = 50 };
			pane1.InsertChildAt(0, new LayoutAnchorable { ContentId = "a1" });

			var pane2 = new LayoutAnchorablePane { DockMinWidth = 150, DockMinHeight = 75 };
			pane2.InsertChildAt(0, new LayoutAnchorable { ContentId = "a2" });

			// Horizontal: widths add, heights take max
			var hPanel = new LayoutPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
			hPanel.InsertChildAt(0, pane1);
			hPanel.InsertChildAt(1, pane2);

			Assert.That(hPanel.CalculatedDockMinWidth(), Is.EqualTo(250));
			Assert.That(hPanel.CalculatedDockMinHeight(), Is.EqualTo(75));
		}

		/// <summary>
		/// Verifies that adding a document to an empty LayoutDocumentPane works.
		/// </summary>
		[Test]
		public void AddDocumentToEmptyPane_Works()
		{
			var pane = new LayoutDocumentPane();
			var doc = new LayoutDocument { Title = "New Doc", ContentId = "newDoc" };

			pane.Children.Add(doc);

			Assert.That(pane.Children.Count, Is.EqualTo(1));
			Assert.That(pane.Children[0], Is.EqualTo(doc));
			Assert.That(doc.Parent, Is.EqualTo(pane));
		}

		/// <summary>
		/// Verifies that removing the last document from a pane doesn't crash.
		/// Regression for #112 - Closing Floating Document Container results in Empty Container.
		/// </summary>
		[Test]
		public void RemoveLastDocument_PaneStaysValid_Issue112()
		{
			var pane = new LayoutDocumentPane();
			var doc = new LayoutDocument { Title = "Doc", ContentId = "doc1" };
			pane.Children.Add(doc);

			pane.RemoveChild(doc);

			Assert.That(pane.Children.Count, Is.EqualTo(0));
			Assert.That(doc.Parent, Is.Null);
		}

		/// <summary>
		/// Verifies that SelectedContentIndex tracks correctly.
		/// </summary>
		[Test]
		public void SelectedContentIndex_TracksCorrectly()
		{
			var pane = new LayoutDocumentPane();
			var doc1 = new LayoutDocument { Title = "Doc1", ContentId = "doc1" };
			var doc2 = new LayoutDocument { Title = "Doc2", ContentId = "doc2" };
			pane.Children.Add(doc1);
			pane.Children.Add(doc2);

			pane.SelectedContentIndex = 1;
			Assert.That(pane.SelectedContentIndex, Is.EqualTo(1));
			Assert.That(pane.SelectedContent, Is.EqualTo(doc2));
		}

		/// <summary>
		/// Verifies that a new LayoutAnchorable is not floating by default.
		/// Regression for #471 - Weird IsFloating error at AnchorablePane creation.
		/// </summary>
		[Test]
		public void NewAnchorable_IsNotFloating_Issue471()
		{
			var anchorable = new LayoutAnchorable { Title = "Tool", ContentId = "tool1" };
			var pane = new LayoutAnchorablePane(anchorable);

			Assert.That(anchorable.IsFloating, Is.False,
				"New anchorable in a pane should not be floating (Issue #471).");
			Assert.That(anchorable.IsHidden, Is.False);
			Assert.That(anchorable.IsAutoHidden, Is.False);
		}

		/// <summary>
		/// Verifies that LayoutAnchorGroup creates auto-hidden state.
		/// Regression for #9 - AutoHide goes to wrong side.
		/// </summary>
		[Test]
		public void AnchorGroup_MakesAutoHidden_Issue9()
		{
			var anchorable = new LayoutAnchorable { Title = "AutoHide", ContentId = "ah1" };
			var group = new LayoutAnchorGroup();
			group.Children.Add(anchorable);

			Assert.That(anchorable.IsAutoHidden, Is.True,
				"Anchorable in LayoutAnchorGroup should be auto-hidden (Issue #9).");
			Assert.That(anchorable.Parent, Is.EqualTo(group));
		}

		/// <summary>
		/// Verifies that LayoutRoot properly tracks hidden anchorables.
		/// Regression for #19 - Hidden anchorable cannot be shown again.
		/// </summary>
		[Test]
		public void HiddenAnchorables_TrackedByRoot_Issue19()
		{
			var pane = new LayoutAnchorablePane();
			var anchorable = new LayoutAnchorable { Title = "Tool", ContentId = "tool1" };
			pane.Children.Add(anchorable);

			var root = new LayoutRoot();
			root.RootPanel = new LayoutPanel(pane);

			Assert.That(anchorable.IsHidden, Is.False);

			anchorable.Hide();

			Assert.That(anchorable.IsHidden, Is.True,
				"Anchorable should be hidden after Hide() (Issue #19).");
			Assert.That(root.Hidden.Contains(anchorable), Is.True,
				"Root.Hidden should contain the hidden anchorable.");
		}

		/// <summary>
		/// Verifies that hidden anchorable can be shown again.
		/// Regression for #19 - Hidden anchorable cannot be shown again.
		/// </summary>
		[Test]
		public void HiddenAnchorable_CanBeShownAgain_Issue19()
		{
			var pane = new LayoutAnchorablePane();
			var anchorable = new LayoutAnchorable { Title = "Tool", ContentId = "tool1" };
			pane.Children.Add(anchorable);

			var root = new LayoutRoot();
			root.RootPanel = new LayoutPanel(pane);

			anchorable.Hide();
			Assert.That(anchorable.IsHidden, Is.True);

			anchorable.Show();
			Assert.That(anchorable.IsHidden, Is.False,
				"Anchorable should be visible after Show() (Issue #19).");
		}
	}
}
