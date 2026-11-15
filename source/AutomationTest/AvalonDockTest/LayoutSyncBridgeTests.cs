#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.Layout;
using AvalonDock.Mvvm;
using AvalonDockTest.TestHelpers;
using NUnit.Framework;
using UnitTests;

namespace AvalonDockTest;

/// <summary>
/// Covers how <see cref="LayoutSyncBridge"/> hands anchorables of the MVVM layout to the
/// <see cref="DockingManager"/>: an anchorable added while the manager is bound goes to the side of
/// its tool dock, like the ones that were there when the layout was bound.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class LayoutSyncBridgeTests : AutomationTestBase
{
	private sealed class Toolbox : ToolboxBase
	{
		public Toolbox(string id, DockZone zone)
		{
			Id = id;
			Title = id;
			Zone = zone;
		}
	}

	/// <summary>
	/// An anchorable added to a tool dock at runtime goes to that tool dock's side - not into the pane
	/// of the active content, where the manager puts an anchorable no strategy placed.
	/// </summary>
	[Test]
	public void AnchorableAddedAtRuntime_GoesToTheSideOfItsToolDock()
	{
		AnchorSide? sideAtBinding = null;
		AnchorSide? activeSide = null;
		AnchorSide? sideAtRuntime = null;

		var failure = RunOnStaThread(() =>
		{
			var manager = new DockingManager();
			var host = HostInWindow(manager);
			try
			{
				var right = new Toolbox("right", DockZone.RightTop);
				var bottom = new Toolbox("bottom", DockZone.BottomLeft);
				var service = new DockLayoutService(new IToolbox[] { right, bottom });
				manager.DockLayout = service.Layout;
				Pump();

				sideAtBinding = SideOf(manager, right);

				// The bottom one pinned and active, as when the user works in a docked bottom pane.
				var bottomAnchorable = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.Content == bottom);
				var mainPanel = manager.Layout.RootPanel;
				var vertical = new LayoutPanel { Orientation = System.Windows.Controls.Orientation.Vertical };
				manager.Layout.RootPanel = vertical;
				vertical.Children.Add(mainPanel);
				var bottomPane = new LayoutAnchorablePane();
				vertical.Children.Add(bottomPane);
				bottomAnchorable.Parent.RemoveChild(bottomAnchorable);
				bottomPane.Children.Add(bottomAnchorable);
				Pump();
				bottomAnchorable.IsActive = true;
				Pump();
				activeSide = SideOf(manager, bottom);

				var late = new Toolbox("late", DockZone.RightTop);
				var rightDock = service.Layout.VisibleDockables!.OfType<IToolDock>().Single(dock => dock.Alignment == DockAlignment.Right);
				rightDock.VisibleDockables!.Add(late);
				Pump();

				sideAtRuntime = SideOf(manager, late);
			}
			finally
			{
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(sideAtBinding, Is.EqualTo(AnchorSide.Right), "an anchorable present at binding goes to its side");
			Assert.That(activeSide, Is.EqualTo(AnchorSide.Bottom), "precondition: the active content is docked at the bottom");
			Assert.That(sideAtRuntime, Is.EqualTo(AnchorSide.Right), "an anchorable added later has to go to its side as well");
		});
	}

	/// <summary>Where the anchorable showing the content ended up: its auto hide side or the side of its docked pane.</summary>
	/// <param name="manager">The manager.</param>
	/// <param name="content">The content.</param>
	/// <returns>The side, or <see langword="null"/> when the content is not shown.</returns>
	private static AnchorSide? SideOf(DockingManager manager, object content)
	{
		var anchorable = manager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(a => a.Content == content);
		if (anchorable?.Parent is LayoutAnchorGroup group && group.Parent is LayoutAnchorSide anchorSide)
			return anchorSide.Side;
		if (anchorable?.Parent is LayoutAnchorablePane pane && pane.FindParent<LayoutFloatingWindow>() == null)
			return pane.GetSide();
		return null;
	}

	/// <summary>Puts the manager into a window and shows it, so the loaded paths under test run.</summary>
	/// <param name="manager">The manager to host.</param>
	/// <returns>The host window.</returns>
	private static Window HostInWindow(DockingManager manager)
	{
		var host = new Window
		{
			Content = manager,
			Width = 800,
			Height = 600,
			ShowInTaskbar = false,
			WindowStartupLocation = WindowStartupLocation.Manual,

			// Kept off screen: these tests are about behaviour, not about what is drawn.
			Left = SystemParameters.VirtualScreenLeft - 10000,
			Top = SystemParameters.VirtualScreenTop - 10000,
		};

		host.Show();
		Pump();
		return host;
	}

	/// <summary>Lets the dispatcher run pending work before anything is asserted.</summary>
	private static void Pump()
	{
		System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
			System.Windows.Threading.DispatcherPriority.ContextIdle,
			new Action(() => { }));
	}

	/// <summary>Runs the given code on an STA thread and returns any exception it raised.</summary>
	/// <param name="action">The code to run.</param>
	/// <returns>The exception, or <see langword="null"/> when the code completed.</returns>
	private Exception? RunOnStaThread(Action action)
	{
		Exception? failure = null;
		ThreadExecutor.RunCodeAsSTA(
			_are,
			() =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					failure = ex;
				}
			});

		_are.WaitOne();
		return failure;
	}
}