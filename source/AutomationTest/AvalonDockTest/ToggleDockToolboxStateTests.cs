namespace AvalonDockTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Threading;

	using NUnit.Framework;

	using AvalonDock;
	using AvalonDock.Core;
	using AvalonDock.Layout;
	using AvalonDock.Mvvm;
	using AvalonDockTest.TestHelpers;

	/// <summary>
	/// Tests for the state a <see cref="ToggleDockingManager"/> and its <see cref="IToolbox"/> view
	/// models agree on.
	/// </summary>
	/// <remarks>
	/// The manager only learns about a toolbox once it is loaded and registers it. A view model built
	/// by a dependency injection container exists long before that, so whatever it decided about its
	/// own visibility was decided while nobody was listening. These tests pin down that such a decision
	/// is still honoured, and that the two sides never end up disagreeing: a toolbox left holding a
	/// value the layout does not show cannot ask for that value again, because a property that does not
	/// change raises no notification.
	/// </remarks>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class ToggleDockToolboxStateTests : AutomationTestBase
	{
		/// <summary>A toolbox view model that can be placed in any zone.</summary>
		private sealed class TestToolbox : ToolboxBase
		{
			/// <summary>Initializes a new instance of the <see cref="TestToolbox"/> class.</summary>
			/// <param name="id">The content id, also used as the title.</param>
			/// <param name="zone">The zone to place the toolbox in.</param>
			public TestToolbox(string id, DockZone zone)
			{
				Id = id;
				Title = id;
				Zone = zone;
			}
		}

		/// <summary>
		/// A toolbox that asked to be open before the manager ever saw it has to be showing once the
		/// manager is loaded. This is the shape a dependency injection container produces: the view
		/// model is constructed at container build time, the manager loads later.
		/// </summary>
		[Test]
		public void IsOpenSetBeforeTheManagerLoads_ShowsTheToolbox()
		{
			var toolbox = new TestToolbox("explorer", DockZone.LeftTop) { IsOpen = true };
			var window = CreateHostingWindow(out var manager, toolbox);

			try
			{
				window.Show();
				DoEvents();

				Assert.That(IsDocked(manager, toolbox), Is.True,
					"An IsOpen set before the manager was loaded has to be applied once it loads.");
				Assert.That(toolbox.IsOpen, Is.True);
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// IsOpenByDefault seeds IsOpen, so an application can read back from a single property whether
		/// a toolbox is showing, whichever of the two put it there.
		/// </summary>
		[Test]
		public void IsOpenByDefault_SeedsIsOpen()
		{
			var toolbox = new TestToolbox("explorer", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, toolbox);

			try
			{
				window.Show();
				DoEvents();

				Assert.That(IsDocked(manager, toolbox), Is.True);
				Assert.That(toolbox.IsOpen, Is.True,
					"IsOpenByDefault has to leave IsOpen reporting what the layout shows.");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// One zone shows one toolbox at a time, so of two that both want to open on startup only one
		/// ends up docked - and the other has to say so, rather than keep claiming it is open.
		/// </summary>
		[Test]
		public void TwoToolboxesInOneZone_LeaveTheCollapsedOneReportingClosed()
		{
			var first = new TestToolbox("first", DockZone.LeftTop) { IsOpenByDefault = true };
			var second = new TestToolbox("second", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, first, second);

			try
			{
				window.Show();
				DoEvents();

				var toolboxes = new[] { first, second };
				Assert.That(toolboxes.Count(t => IsDocked(manager, t)), Is.EqualTo(1),
					"A zone shows one toolbox at a time.");
				Assert.That(toolboxes.Single(t => !IsDocked(manager, t)).IsOpen, Is.False,
					"A toolbox collapsed to make room for a sibling may not keep reporting itself as open.");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// The regression this fixture exists for: a toolbox collapsed by a sibling used to keep
		/// IsOpen == true, so assigning true again changed nothing, raised no notification, and left the
		/// toolbox impossible to reopen from the view model.
		/// </summary>
		[Test]
		public void AToolboxCollapsedByASibling_CanBeReopenedFromTheViewModel()
		{
			var first = new TestToolbox("first", DockZone.LeftTop) { IsOpenByDefault = true };
			var second = new TestToolbox("second", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, first, second);

			try
			{
				window.Show();
				DoEvents();

				var toolboxes = new[] { first, second };
				var collapsed = toolboxes.Single(t => !IsDocked(manager, t));
				var showing = toolboxes.Single(t => IsDocked(manager, t));

				collapsed.IsOpen = true;
				DoEvents();

				Assert.That(IsDocked(manager, collapsed), Is.True,
					"Setting IsOpen = true has to bring the collapsed toolbox back.");
				Assert.That(IsDocked(manager, showing), Is.False);
				Assert.That(showing.IsOpen, Is.False,
					"Reopening one toolbox has to collapse the sibling and tell that sibling about it.");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// Closing from the view model still works now that the open and close paths run through the
		/// same toggle implementation.
		/// </summary>
		[Test]
		public void SettingIsOpenToFalse_CollapsesTheToolbox()
		{
			var toolbox = new TestToolbox("explorer", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, toolbox);

			try
			{
				window.Show();
				DoEvents();
				Assert.That(IsDocked(manager, toolbox), Is.True);

				toolbox.IsOpen = false;
				DoEvents();

				Assert.That(IsDocked(manager, toolbox), Is.False);
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// The reconciliation runs again whenever the bars are rebuilt - the manager is re-attached to
		/// the visual tree, the theme changes - so it must never toggle a showing toolbox back off.
		/// </summary>
		[Test]
		public void ApplyingTheInitialState_IsIdempotent()
		{
			var toolbox = new TestToolbox("explorer", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, toolbox);

			try
			{
				window.Show();
				DoEvents();
				Assert.That(IsDocked(manager, toolbox), Is.True);

				ApplyInitialToolboxStateAgain(manager);
				DoEvents();

				Assert.That(IsDocked(manager, toolbox), Is.True,
					"Re-applying the startup state may not close what is already open.");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// Builds an off screen window hosting a <see cref="ToggleDockingManager"/> whose layout already
		/// holds the given toolboxes, which is the order an application declaring its layout in XAML
		/// produces: the layout is in place before the manager is loaded.
		/// </summary>
		/// <param name="manager">Receives the hosted manager.</param>
		/// <param name="toolboxes">The toolboxes to place in the layout.</param>
		/// <returns>The window, not yet shown.</returns>
		private static Window CreateHostingWindow(out ToggleDockingManager manager, params TestToolbox[] toolboxes)
		{
			manager = new ToggleDockingManager
			{
				Layout = BuildLayout(toolboxes),
			};

			return new Window
			{
				Width = 800,
				Height = 600,
				ShowInTaskbar = false,
				ShowActivated = false,
				WindowStartupLocation = WindowStartupLocation.Manual,
				Left = -10000,
				Top = -10000,
				Content = manager,
			};
		}

		/// <summary>
		/// Builds a layout holding one auto hidden anchorable per toolbox, which is the state the toggle
		/// docking manager keeps them in while their sidebar button is not toggled on.
		/// </summary>
		/// <param name="toolboxes">The toolboxes to place.</param>
		/// <returns>The layout root.</returns>
		private static LayoutRoot BuildLayout(IEnumerable<TestToolbox> toolboxes)
		{
			var layout = new LayoutRoot
			{
				RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())),
			};

			foreach (var toolbox in toolboxes)
			{
				var group = new LayoutAnchorGroup();
				group.Children.Add(new LayoutAnchorable
				{
					Title = toolbox.Title,
					ContentId = toolbox.Id,
					Content = toolbox,
				});

				SideFor(layout, toolbox.Zone).Children.Add(group);
			}

			return layout;
		}

		/// <summary>Gets the layout side a zone belongs to.</summary>
		/// <param name="layout">The layout.</param>
		/// <param name="zone">The zone.</param>
		/// <returns>The side of the layout holding that zone.</returns>
		private static LayoutAnchorSide SideFor(LayoutRoot layout, DockZone zone)
		{
			switch (zone)
			{
				case DockZone.RightTop:
				case DockZone.RightBottom:
					return layout.RightSide;
				case DockZone.BottomLeft:
				case DockZone.BottomRight:
					return layout.BottomSide;
				default:
					return layout.LeftSide;
			}
		}

		/// <summary>Gets a value indicating whether the given toolbox is showing in the dock area.</summary>
		/// <param name="manager">The manager holding the layout.</param>
		/// <param name="toolbox">The toolbox to check.</param>
		/// <returns>True when the toolbox is docked rather than collapsed onto its stripe.</returns>
		private static bool IsDocked(ToggleDockingManager manager, TestToolbox toolbox)
		{
			var anchorable = manager.Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.First(a => ReferenceEquals(a.Content, toolbox));

			return !anchorable.IsAutoHidden;
		}

		/// <summary>
		/// Runs the startup reconciliation a second time. It is private because applications never call
		/// it themselves; the manager runs it whenever it rebuilds the sidebar bars.
		/// </summary>
		/// <param name="manager">The manager to run it on.</param>
		private static void ApplyInitialToolboxStateAgain(ToggleDockingManager manager)
		{
			var method = typeof(ToggleDockingManager).GetMethod(
				"ApplyInitialToolboxState", BindingFlags.Instance | BindingFlags.NonPublic);

			Assert.That(method, Is.Not.Null, "ApplyInitialToolboxState is the seam these tests rely on.");
			method.Invoke(manager, null);
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
