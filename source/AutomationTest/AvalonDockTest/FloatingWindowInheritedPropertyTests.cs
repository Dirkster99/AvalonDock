namespace AvalonDockTest
{
	using System;
	using System.Linq;
	using System.Windows.Threading;

	using NUnit.Framework;

	using AvalonDock.Controls;
	using AvalonDockTest.TestHelpers;
	using AvalonDockTest.Views;

	/// <summary>
	/// Regression coverage for issue #615: an inheritable dependency property assigned to an ancestor of
	/// the <see cref="AvalonDock.DockingManager"/> reached the content of a floating window but not the
	/// floating window itself, so its title kept the default value.
	/// </summary>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class FloatingWindowInheritedPropertyTests : AutomationTestBase
	{
		/// <summary>A font size that differs from the WPF default of 12.</summary>
		private const double ExpectedFontSize = 27d;

		[Test]
		public void FontSizeChangedAfterFloatingReachesTheFloatingWindow_Issue615()
		{
			var window = CreateTestWindow();
			try
			{
				var floatingWindow = FloatFirstAnchorable(window);

				window.FontSize = ExpectedFontSize;
				window.UpdateLayout();

				Assert.That(floatingWindow.FontSize, Is.EqualTo(ExpectedFontSize),
					"A floating window has to pick up the FontSize of the window hosting the DockingManager (Issue #615).");
			}
			finally
			{
				window.Close();
			}
		}

		[Test]
		public void FontSizeSetBeforeFloatingReachesTheFloatingWindow_Issue615()
		{
			var window = CreateTestWindow();
			try
			{
				window.FontSize = ExpectedFontSize;
				window.UpdateLayout();

				var floatingWindow = FloatFirstAnchorable(window);

				Assert.That(floatingWindow.FontSize, Is.EqualTo(ExpectedFontSize),
					"A floating window created after the FontSize was assigned has to pick it up as well (Issue #615).");
			}
			finally
			{
				window.Close();
			}
		}

		[Test]
		public void FontSizeAssignedToTheFloatingWindowItselfIsNotOverwritten_Issue615()
		{
			var window = CreateTestWindow();
			try
			{
				var floatingWindow = FloatFirstAnchorable(window);
				floatingWindow.FontSize = ExpectedFontSize + 5d;

				window.FontSize = ExpectedFontSize;
				window.UpdateLayout();

				Assert.That(floatingWindow.FontSize, Is.EqualTo(ExpectedFontSize + 5d),
					"A value assigned to the floating window itself has to win over the mirrored value.");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>Creates the test window and waits until it is loaded and its layout is up to date.</summary>
		/// <returns>The fully loaded test window.</returns>
		private static LayoutAnchorableFloatingWindowControlTestWindow CreateTestWindow()
		{
			var taskResult = WindowHelpers.CreateInvisibleWindowAsync<LayoutAnchorableFloatingWindowControlTestWindow>();
			taskResult.Wait();

			var window = taskResult.Result;

			// The DockingManager builds its Layout*Controls when it is loaded, so the dispatcher queue has to
			// be drained before the docking manager can be operated on.
			DoEvents();
			window.UpdateLayout();
			DoEvents();
			window.UpdateLayout();

			return window;
		}

		/// <summary>Floats an anchorable of the test window and waits until its floating window is shown.</summary>
		/// <param name="window">The test window to operate on.</param>
		/// <returns>The floating window hosting the anchorable.</returns>
		private static LayoutFloatingWindowControl FloatFirstAnchorable(LayoutAnchorableFloatingWindowControlTestWindow window)
		{
			window.Window1.Float();

			// The floating window is shown through a dispatcher operation, so the queue has to be drained
			// before the floating window is loaded.
			DoEvents();
			window.UpdateLayout();

			var floatingWindow = window.dockingManager.FloatingWindows.FirstOrDefault();
			Assert.That(floatingWindow, Is.Not.Null, "The anchorable should be hosted in a floating window.");

			return floatingWindow;
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
