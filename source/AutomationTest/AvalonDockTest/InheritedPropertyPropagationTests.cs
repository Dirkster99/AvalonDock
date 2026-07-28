namespace AvalonDockTest
{
	using System;
	using System.Windows;
	using System.Windows.Documents;
	using System.Windows.Media;
	using System.Windows.Threading;

	using NUnit.Framework;

	using AvalonDockTest.TestHelpers;
	using AvalonDockTest.Views;

	/// <summary>
	/// Regression coverage for the propagation of inheritable dependency properties into the controls
	/// that are managed by a <see cref="AvalonDock.DockingManager"/>.
	/// Covers issues:
	///   #614 - an inheritable property assigned to an ancestor of the DockingManager does not reach the
	///          controls inside the panels, depending on the order of the logical children
	/// </summary>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class InheritedPropertyPropagationTests : AutomationTestBase
	{
		/// <summary>A font size that differs from the WPF default of 12.</summary>
		private const double ExpectedFontSize = 27d;

		[Test]
		public void InheritablePropertyReachesTheViewOfEveryLayoutItem_Issue614()
		{
			var window = CreateTestWindow();
			try
			{
				var documentView = window.dockingManager.GetLayoutItemFromModel(window.Document1).View;
				var anchorableView = window.dockingManager.GetLayoutItemFromModel(window.Anchorable1).View;

				// The defect only shows up once the views are part of the visual tree of a Layout*Control,
				// because only then are they reachable through two different paths from the DockingManager.
				Assert.That(VisualTreeHelper.GetParent(documentView), Is.Not.Null,
					"The document view has to be part of the visual tree for this test to be meaningful.");
				Assert.That(VisualTreeHelper.GetParent(anchorableView), Is.Not.Null,
					"The anchorable view has to be part of the visual tree for this test to be meaningful.");

				window.FontSize = ExpectedFontSize;
				window.UpdateLayout();

				Assert.That(GetInheritedFontSize(documentView), Is.EqualTo(ExpectedFontSize),
					"The view of a LayoutDocument has to inherit the FontSize of the window hosting the DockingManager (Issue #614).");
				Assert.That(GetInheritedFontSize(anchorableView), Is.EqualTo(ExpectedFontSize),
					"The view of a LayoutAnchorable has to inherit the FontSize of the window hosting the DockingManager (Issue #614).");
				Assert.That(window.DocumentContent.FontSize, Is.EqualTo(ExpectedFontSize),
					"The content of a LayoutDocument has to inherit the FontSize of the window hosting the DockingManager.");
				Assert.That(window.AnchorableContent.FontSize, Is.EqualTo(ExpectedFontSize),
					"The content of a LayoutAnchorable has to inherit the FontSize of the window hosting the DockingManager.");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// Reads the inherited font size of an element. A <see cref="System.Windows.Controls.ContentPresenter"/>
		/// derives from <see cref="FrameworkElement"/> and therefore has no FontSize property of its own, but it
		/// does take part in the inheritance of <see cref="TextElement.FontSizeProperty"/>.
		/// </summary>
		/// <param name="element">The element to read the font size from.</param>
		/// <returns>The inherited font size of <paramref name="element"/>.</returns>
		private static double GetInheritedFontSize(DependencyObject element) =>
			(double)element.GetValue(TextElement.FontSizeProperty);

		/// <summary>Creates the test window and waits until it is loaded and its layout is up to date.</summary>
		/// <returns>The fully loaded test window.</returns>
		private static InheritedPropertyTestWindow CreateTestWindow()
		{
			var taskResult = WindowHelpers.CreateInvisibleWindowAsync<InheritedPropertyTestWindow>();
			taskResult.Wait();

			var window = taskResult.Result;

			// The DockingManager builds its Layout*Controls when it is loaded, so the dispatcher queue has to
			// be drained before the visual tree below the DockingManager can be inspected.
			DoEvents();
			window.UpdateLayout();
			DoEvents();
			window.UpdateLayout();

			return window;
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
