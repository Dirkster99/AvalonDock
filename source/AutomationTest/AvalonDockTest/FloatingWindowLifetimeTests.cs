namespace AvalonDockTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Automation;
	using System.Windows.Threading;

	using NUnit.Framework;

	using AvalonDock;
	using AvalonDock.Layout;
	using AvalonDockTest.TestHelpers;

	/// <summary>
	/// Regression coverage for issue #587: floating an anchorable over and over left empty windows
	/// behind that could not be closed individually and only disappeared with the application.
	/// </summary>
	/// <remarks>
	/// The overlay windows of the reported scenario are only put on screen by a real mouse drag, which
	/// a unit test cannot perform. What is covered here is that repeating the float and dock cycle
	/// reaches a steady state - neither the floating windows themselves nor the content hosts they
	/// register with the <see cref="DockingManager"/> may grow with every float.
	/// </remarks>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class FloatingWindowLifetimeTests : AutomationTestBase
	{
		/// <summary>
		/// The automation name that <c>LayoutFloatingWindowControl.FloatingWindowContentHost</c> gives to
		/// the presenter it registers as a logical child of the <see cref="DockingManager"/>.
		/// </summary>
		private const string FloatingWindowHostName = "FloatingWindowHost";

		/// <summary>
		/// The number of float and dock cycles that are run before the counts are taken as the baseline.
		/// A docked anchorable keeps its floating window as the container to float back into, so the
		/// first cycle is the one that establishes it and the ones after it have to change nothing.
		/// </summary>
		private const int WarmUpCycles = 2;

		[Test]
		public void RepeatedFloatAndDockDoesNotAccumulateWindows_Issue587()
		{
			var window = CreateHostingWindow(out var dockingManager, out var anchorable);
			try
			{
				window.Show();
				DoEvents();

				for (var cycle = 1; cycle <= WarmUpCycles; cycle++)
					RunFloatAndDockCycle(anchorable);

				var floatingWindows = dockingManager.FloatingWindows.Count();
				var contentHosts = FloatingWindowContentHostsOf(dockingManager).Count;

				Assert.That(floatingWindows, Is.EqualTo(1),
					"Floating a single anchorable has to end up with exactly one floating window.");

				for (var cycle = WarmUpCycles + 1; cycle <= WarmUpCycles + 3; cycle++)
				{
					RunFloatAndDockCycle(anchorable);

					Assert.That(dockingManager.FloatingWindows.Count(), Is.EqualTo(floatingWindows),
						$"Floating an anchorable again must not add another floating window (cycle {cycle}, Issue #587).");

					Assert.That(FloatingWindowContentHostsOf(dockingManager).Count, Is.LessThanOrEqualTo(contentHosts),
						"The content hosts registered with the DockingManager must not grow with every float, " +
						$"they have to be handed back when a floating window goes away (cycle {cycle}, Issue #587).");
				}
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>Floats the given anchorable and docks it back again.</summary>
		/// <param name="anchorable">The anchorable to float and dock.</param>
		private static void RunFloatAndDockCycle(LayoutAnchorable anchorable)
		{
			anchorable.Float();
			DoEvents();

			anchorable.Dock();
			DoEvents();
		}

		/// <summary>
		/// Collects the content hosts of floating windows that are registered as logical children of the
		/// given <see cref="DockingManager"/>.
		/// </summary>
		/// <param name="manager">The docking manager to inspect.</param>
		/// <returns>The content hosts the manager holds.</returns>
		private static IReadOnlyList<object> FloatingWindowContentHostsOf(DockingManager manager)
		{
			var hosts = new List<object>();
			foreach (var child in LogicalTreeHelper.GetChildren(manager))
			{
				if (child is DependencyObject dependencyObject &&
					AutomationProperties.GetName(dependencyObject) == FloatingWindowHostName)
				{
					hosts.Add(child);
				}
			}

			return hosts;
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
