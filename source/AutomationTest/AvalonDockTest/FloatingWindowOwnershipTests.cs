namespace AvalonDockTest
{
	using System;
	using System.Linq;
	using System.Windows;
	using System.Windows.Threading;

	using NUnit.Framework;

	using AvalonDock;
	using AvalonDock.Layout;
	using AvalonDockTest.TestHelpers;

	/// <summary>
	/// Regression coverage for issue #618: floating a <see cref="LayoutAnchorable"/> before the window
	/// hosting the <see cref="DockingManager"/> has been shown crashed with
	/// InvalidOperationException("Cannot set Owner property to a Window that has not been shown previously."),
	/// because WPF refuses to own a window by a window that has never been shown.
	/// </summary>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class FloatingWindowOwnershipTests : AutomationTestBase
	{
		[Test]
		public void FloatBeforeTheHostingWindowIsShownDoesNotThrow_Issue618()
		{
			var window = CreateHostingWindow(out var dockingManager, out var anchorable);
			Exception dispatcherException = null;
			void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
			{
				dispatcherException = e.Exception;
				e.Handled = true;
			}

			Dispatcher.CurrentDispatcher.UnhandledException += OnUnhandledException;
			try
			{
				// The floating window is shown through a dispatcher operation, so the crash of issue #618
				// surfaces while the dispatcher queue is drained and not in the call to Float() itself.
				Assert.DoesNotThrow(
					() =>
					{
						anchorable.Float();
						DoEvents();
					},
					"Floating an anchorable before the window hosting the DockingManager is shown must not throw (Issue #618).");

				Assert.That(dispatcherException, Is.Null,
					"Floating an anchorable before the window hosting the DockingManager is shown must not throw (Issue #618).");

				var floatingWindow = dockingManager.FloatingWindows.FirstOrDefault();
				Assert.That(floatingWindow, Is.Not.Null, "The anchorable should be hosted in a floating window.");
				Assert.That(floatingWindow.Owner, Is.Null,
					"As long as the hosting window has not been shown it cannot own the floating window.");
			}
			finally
			{
				Dispatcher.CurrentDispatcher.UnhandledException -= OnUnhandledException;
				window.Close();
			}
		}

		[Test]
		public void OwnershipIsEstablishedOnceTheHostingWindowIsShown_Issue618()
		{
			var window = CreateHostingWindow(out var dockingManager, out var anchorable);
			try
			{
				anchorable.Float();
				DoEvents();

				var floatingWindow = dockingManager.FloatingWindows.FirstOrDefault();
				Assert.That(floatingWindow, Is.Not.Null, "The anchorable should be hosted in a floating window.");
				Assert.That(floatingWindow.Owner, Is.Null,
					"As long as the hosting window has not been shown it cannot own the floating window.");

				window.Show();
				DoEvents();

				Assert.That(floatingWindow.Owner, Is.SameAs(window),
					"The deferred ownership has to be established as soon as the hosting window is shown (Issue #618).");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// Creates a window hosting a <see cref="DockingManager"/> with a single floatable anchorable
		/// without showing the window.
		/// </summary>
		/// <param name="dockingManager">The docking manager hosted by the window.</param>
		/// <param name="anchorable">The anchorable that can be floated.</param>
		/// <returns>The window that has not been shown yet.</returns>
		private static Window CreateHostingWindow(out DockingManager dockingManager, out LayoutAnchorable anchorable)
		{
			dockingManager = new DockingManager();
			anchorable = new LayoutAnchorable { Title = "Anchorable" };
			dockingManager.Layout.RootPanel.Children.Add(new LayoutAnchorablePane(anchorable));

			return new Window
			{
				Width = 400,
				Height = 300,
				ShowInTaskbar = false,
				ShowActivated = false,
				WindowStartupLocation = WindowStartupLocation.Manual,
				Left = -10000,
				Top = -10000,
				Content = dockingManager,
			};
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
