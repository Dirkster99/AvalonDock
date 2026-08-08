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
	/// these tests cannot perform. What they do cover is the other half of the report - the resources a
	/// floating window leaves behind once it has been closed - and the plain bookkeeping of the windows
	/// themselves over repeated float and dock cycles.
	/// </remarks>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class FloatingWindowLifetimeTests : AutomationTestBase
	{
		/// <summary>
		/// The automation name that <see cref="AvalonDock.Controls.LayoutFloatingWindowControl"/> gives to
		/// the presenter it registers as a logical child of the <see cref="DockingManager"/>.
		/// </summary>
		private const string FloatingWindowHostName = "FloatingWindowHost";

		[Test]
		public void RepeatedFloatAndDockKeepsExactlyOneFloatingWindow_Issue587()
		{
			var window = CreateHostingWindow(out var dockingManager, out var anchorable);
			try
			{
				window.Show();
				DoEvents();

				for (var cycle = 1; cycle <= 3; cycle++)
				{
					anchorable.Float();
					DoEvents();

					Assert.That(dockingManager.FloatingWindows.Count(), Is.EqualTo(1),
						$"Floating an anchorable has to create exactly one floating window (cycle {cycle}, Issue #587).");

					anchorable.Dock();
					DoEvents();

					Assert.That(dockingManager.FloatingWindows, Is.Empty,
						$"Docking an anchorable back has to take its floating window down again (cycle {cycle}, Issue #587).");
				}
			}
			finally
			{
				window.Close();
			}
		}

		[Test]
		public void ClosedFloatingWindowReleasesItsContentHost_Issue587()
		{
			var window = CreateHostingWindow(out var dockingManager, out var anchorable);
			try
			{
				window.Show();
				DoEvents();

				for (var cycle = 1; cycle <= 3; cycle++)
				{
					anchorable.Float();
					DoEvents();

					anchorable.Dock();
					DoEvents();

					Assert.That(FloatingWindowContentHostsOf(dockingManager), Is.Empty,
						"A closed floating window has to hand its content host back to the DockingManager, " +
						$"otherwise every float leaves one behind for the rest of the session (cycle {cycle}, Issue #587).");
				}
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// Collects the content hosts of floating windows that are still registered as logical children of
		/// the given <see cref="DockingManager"/>.
		/// </summary>
		/// <param name="manager">The docking manager to inspect.</param>
		/// <returns>The content hosts the manager still holds.</returns>
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
