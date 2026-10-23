namespace AvalonDockTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Threading;

	using NUnit.Framework;

	using AvalonDock;
	using AvalonDock.Controls;
	using AvalonDock.Layout;
	using AvalonDockTest.TestHelpers;

	/// <summary>
	/// Regression coverage for issue #677: two tool windows docked side by side in a horizontal
	/// <see cref="LayoutPanel"/>. Hiding one of them and then floating the other one leaves the shared
	/// row without visible content. Showing the hidden one again puts it back into the model - visible,
	/// selected and attached to the main layout - but the <see cref="LayoutPanelControl"/> that hosts it
	/// keeps an <see cref="FrameworkElement.ActualHeight"/> of zero, so the tool window never appears.
	/// Docking the floating sibling again does not bring it back either.
	/// </summary>
	/// <remarks>
	/// The model is fine throughout - a layout saved and loaded again shows the tool window - so these
	/// tests look at the live controls: the size the <see cref="LayoutAnchorablePaneControl"/> of the
	/// shown tool window and every <see cref="LayoutPanelControl"/> above it actually get.
	/// </remarks>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class HiddenAnchorableShowAfterFloatingSiblingTests : AutomationTestBase
	{
		/// <summary>
		/// Hide B, float A, show B: B has to get a size again.
		/// </summary>
		/// <param name="documentBelowTheRow">
		/// <see langword="true"/> to put a document pane below the row of tool windows, <see langword="false"/>
		/// to put another tool window there. <see cref="LayoutPanelControl"/> fixes the dock lengths of its
		/// children differently depending on whether it holds documents, so both are covered.
		/// </param>
		[TestCase(true, TestName = "ShowAfterFloatingSibling_GetsASize_DocumentBelow_Issue677")]
		[TestCase(false, TestName = "ShowAfterFloatingSibling_GetsASize_ToolWindowBelow_Issue677")]
		public void ShowAfterFloatingSibling_GetsASize_Issue677(bool documentBelowTheRow)
		{
			var window = CreateHostingWindow(documentBelowTheRow, out var dockingManager, out var anchorableA, out var anchorableB);
			try
			{
				window.Show();
				Settle(window);

				AssertHasASize(dockingManager, anchorableB, "before anything was done (sanity check of the test)");

				anchorableB.Hide();
				Settle(window);
				Assert.That(anchorableB.IsHidden, Is.True, "Hide did not hide panel B.");

				anchorableA.Float();
				Settle(window);
				Assert.That(anchorableA.IsFloating, Is.True, "Float did not float panel A.");

				anchorableB.Show();
				Settle(window);

				Assert.Multiple(() =>
				{
					Assert.That(anchorableB.IsHidden, Is.False, "Show left panel B hidden.");
					Assert.That(anchorableB.IsVisible, Is.True, "Show left panel B invisible.");
					Assert.That(anchorableB.Root, Is.SameAs(dockingManager.Layout), "Panel B is not attached to the layout.");
					Assert.That(anchorableB.FindParent<LayoutFloatingWindow>(), Is.Null, "Panel B was expected in the docked area.");
				});

				AssertHasASize(dockingManager, anchorableB, "after showing it while its sibling floats");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// The issue reports that docking the floating sibling again does not bring the tool window back.
		/// </summary>
		/// <param name="documentBelowTheRow">See <see cref="ShowAfterFloatingSibling_GetsASize_Issue677"/>.</param>
		[TestCase(true, TestName = "ShowAfterFloatingSibling_GetsASizeAfterSiblingIsDocked_DocumentBelow_Issue677")]
		[TestCase(false, TestName = "ShowAfterFloatingSibling_GetsASizeAfterSiblingIsDocked_ToolWindowBelow_Issue677")]
		public void ShowAfterFloatingSibling_GetsASizeAfterSiblingIsDocked_Issue677(bool documentBelowTheRow)
		{
			var window = CreateHostingWindow(documentBelowTheRow, out var dockingManager, out var anchorableA, out var anchorableB);
			try
			{
				window.Show();
				Settle(window);

				anchorableB.Hide();
				Settle(window);

				anchorableA.Float();
				Settle(window);

				anchorableB.Show();
				Settle(window);

				anchorableA.Dock();
				Settle(window);

				Assert.That(anchorableA.IsFloating, Is.False, "Dock did not dock panel A again.");
				AssertHasASize(dockingManager, anchorableA, "after docking it again");
				AssertHasASize(dockingManager, anchorableB, "after its sibling was docked again");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// Asserts that the pane control hosting <paramref name="anchorable"/>, and every
		/// <see cref="LayoutPanelControl"/> above it, was given a size by the layout.
		/// </summary>
		/// <param name="dockingManager">The docking manager hosting the anchorable.</param>
		/// <param name="anchorable">The anchorable that has to be on screen.</param>
		/// <param name="when">Describes the moment of the check for the failure message.</param>
		private static void AssertHasASize(DockingManager dockingManager, LayoutAnchorable anchorable, string when)
		{
			var paneModel = anchorable.Parent as LayoutAnchorablePane;
			Assert.That(paneModel, Is.Not.Null, $"{anchorable.Title} is not hosted by an anchorable pane {when}.");

			var paneControl = dockingManager.FindVisualChildren<LayoutAnchorablePaneControl>()
				.FirstOrDefault(control => control.Model == paneModel);
			Assert.That(paneControl, Is.Not.Null, $"No pane control was created for {anchorable.Title} {when}.");

			Assert.Multiple(() =>
			{
				Assert.That(paneControl.IsVisible, Is.True, $"The pane control of {anchorable.Title} is not visible {when}.");
				Assert.That(paneControl.ActualWidth, Is.GreaterThan(0d), $"The pane control of {anchorable.Title} has no width {when} (Issue #677).");
				Assert.That(paneControl.ActualHeight, Is.GreaterThan(0d), $"The pane control of {anchorable.Title} has no height {when} (Issue #677).");

				foreach (var panelControl in AncestorsOf<LayoutPanelControl>(paneControl, dockingManager))
				{
					var panel = (LayoutPanel)panelControl.Model;
					Assert.That(panelControl.ActualHeight, Is.GreaterThan(0d),
						$"The {panel.Orientation} LayoutPanelControl above {anchorable.Title} has no height {when} (Issue #677).");
					Assert.That(panelControl.ActualWidth, Is.GreaterThan(0d),
						$"The {panel.Orientation} LayoutPanelControl above {anchorable.Title} has no width {when} (Issue #677).");
				}
			});
		}

		/// <summary>Walks the visual tree up from <paramref name="start"/> to <paramref name="stop"/>.</summary>
		/// <typeparam name="T">The type of the ancestors to return.</typeparam>
		/// <param name="start">The element to start from; it is not returned itself.</param>
		/// <param name="stop">The element to stop at; it is not returned itself.</param>
		/// <returns>The ancestors of the given type, closest first.</returns>
		private static IEnumerable<T> AncestorsOf<T>(DependencyObject start, DependencyObject stop)
			where T : DependencyObject
		{
			for (var current = VisualTreeHelper.GetParent(start);
				current != null && current != stop;
				current = VisualTreeHelper.GetParent(current))
			{
				if (current is T match) yield return match;
			}
		}

		/// <summary>
		/// Creates a window hosting the layout of the issue without showing the window:
		/// <code>
		/// LayoutPanel (Horizontal)
		///   LayoutPanel (Vertical)
		///     LayoutPanel (Horizontal)          - the row of the issue
		///       LayoutAnchorablePaneGroup
		///         LayoutAnchorablePane          - A
		///       LayoutAnchorablePaneGroup
		///         LayoutAnchorablePane          - B
		///     LayoutDocumentPaneGroup | LayoutAnchorablePane
		///   LayoutAnchorablePane                - a tool window on the right
		/// </code>
		/// </summary>
		/// <param name="documentBelowTheRow">Whether the vertical panel holds a document pane below the row.</param>
		/// <param name="dockingManager">The docking manager hosted by the window.</param>
		/// <param name="anchorableA">The tool window that is floated.</param>
		/// <param name="anchorableB">The tool window that is hidden and shown again.</param>
		/// <returns>The window that has not been shown yet.</returns>
		private static Window CreateHostingWindow(
			bool documentBelowTheRow,
			out DockingManager dockingManager,
			out LayoutAnchorable anchorableA,
			out LayoutAnchorable anchorableB)
		{
			anchorableA = new LayoutAnchorable { Title = "A", ContentId = "A", Content = new Grid() };
			anchorableB = new LayoutAnchorable { Title = "B", ContentId = "B", Content = new Grid() };

			var row = new LayoutPanel
			{
				Orientation = Orientation.Horizontal,
				DockHeight = new GridLength(2, GridUnitType.Star),
			};
			row.Children.Add(new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(anchorableA)));
			row.Children.Add(new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(anchorableB)));

			ILayoutPanelElement belowTheRow = documentBelowTheRow
				? new LayoutDocumentPaneGroup(new LayoutDocumentPane(
					new LayoutDocument { Title = "Document", ContentId = "Document", Content = new Grid() }))
				: new LayoutAnchorablePane(
					new LayoutAnchorable { Title = "Below", ContentId = "Below", Content = new Grid() });

			var column = new LayoutPanel
			{
				Orientation = Orientation.Vertical,
				DockWidth = new GridLength(4, GridUnitType.Star),
			};
			column.Children.Add(row);
			column.Children.Add(belowTheRow);

			var rootPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
			rootPanel.Children.Add(column);
			rootPanel.Children.Add(new LayoutAnchorablePane(
				new LayoutAnchorable { Title = "Right", ContentId = "Right", Content = new Grid() }));

			dockingManager = new DockingManager
			{
				Layout = new LayoutRoot { RootPanel = rootPanel },
			};

			return new Window
			{
				Width = 1200,
				Height = 800,
				ShowInTaskbar = false,
				ShowActivated = false,
				WindowStartupLocation = WindowStartupLocation.Manual,
				Left = -10000,
				Top = -10000,
				Content = dockingManager,
			};
		}

		/// <summary>
		/// Lets the layout settle: the grid controls rebuild their children through dispatcher
		/// operations, and a rebuilt child only gets its size in the layout pass after that.
		/// </summary>
		/// <param name="window">The window hosting the docking manager.</param>
		private static void Settle(Window window)
		{
			for (var pass = 0; pass < 3; pass++)
			{
				DoEvents();
				window.UpdateLayout();
			}
		}

		/// <summary>
		/// Drains the dispatcher queue down to <see cref="DispatcherPriority.Background"/>, which also
		/// executes every pending <see cref="DispatcherPriority.Loaded"/> operation.
		/// </summary>
		private static void DoEvents()
		{
			var frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(
				DispatcherPriority.Background,
				new Action(() => frame.Continue = false));
			Dispatcher.PushFrame(frame);
		}
	}
}
