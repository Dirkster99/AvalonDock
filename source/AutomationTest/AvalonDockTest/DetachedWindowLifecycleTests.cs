#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDockTest.TestHelpers;
using NUnit.Framework;

namespace AvalonDockTest;

/// <summary>
/// Regression tests for what happens to a standalone window when the docking manager it belongs to
/// goes away.
/// </summary>
/// <remarks>
/// A detached window outlives the manager in three ways, and each one strands it: it holds the
/// presenter that the rebuilt layout needs, it hosts a model that may no longer be part of any
/// layout, and because it has no owner it keeps the process alive. These tests pin the agreed
/// behaviour for each case.
/// </remarks>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class DetachedWindowLifecycleTests : AutomationTestBase
{
	/// <summary>Unloading the manager closes the window but remembers that it was detached.</summary>
	/// <remarks>
	/// Unloading also happens when the manager is merely switched away from, e.g. between tabs, so the
	/// flag has to survive for the restore path to bring the window back.
	/// </remarks>
	[Test]
	public void Unload_ClosesDetachedWindow_ButKeepsAnchorableMarkedDetached()
	{
		var detachedWhileOpen = false;
		var stillDetachedAfterUnload = false;
		var openWindowsAfterUnload = -1;

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			var host = HostInWindow(manager);
			try
			{
				manager.DetachAnchorableToWindow(anchorable);
				detachedWhileOpen = manager.IsDetached(anchorable);

				// Detaching the manager from its parent raises Unloaded, which is what a tab switch does.
				host.Content = null;
				Pump();

				openWindowsAfterUnload = manager.DetachedAnchorables.Count();
				stillDetachedAfterUnload = anchorable.IsDetached;
			}
			finally
			{
				manager.ReattachAllDetachedAnchorables();
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(detachedWhileOpen, Is.True, "The anchorable should be detached to begin with.");
			Assert.That(openWindowsAfterUnload, Is.Zero, "Unloading should leave no standalone window open.");
			Assert.That(stillDetachedAfterUnload, Is.True,
				"The anchorable should stay marked as detached so the window is recreated on reload.");
		});
	}

	/// <summary>Replacing the layout closes windows belonging to the layout that is going away.</summary>
	[Test]
	public void ReplacingLayout_ClosesDetachedWindows()
	{
		var detachedBefore = false;
		var openWindowsAfter = -1;

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			var host = HostInWindow(manager);
			try
			{
				manager.DetachAnchorableToWindow(anchorable);
				detachedBefore = manager.IsDetached(anchorable);

				manager.Layout = BuildLayout(out _);
				Pump();

				openWindowsAfter = manager.DetachedAnchorables.Count();
			}
			finally
			{
				manager.ReattachAllDetachedAnchorables();
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(detachedBefore, Is.True, "The anchorable should be detached to begin with.");
			Assert.That(openWindowsAfter, Is.Zero,
				"A window whose anchorable left the manager with the old layout must not survive it.");
		});
	}

	/// <summary>Docking back on request clears the flag, unlike the lifecycle paths.</summary>
	[Test]
	public void ReattachAnchorable_ClearsDetachedFlag()
	{
		var stillDetached = true;
		var openWindows = -1;

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			var host = HostInWindow(manager);
			try
			{
				manager.DetachAnchorableToWindow(anchorable);
				manager.ReattachAnchorable(anchorable);
				Pump();

				stillDetached = anchorable.IsDetached;
				openWindows = manager.DetachedAnchorables.Count();
			}
			finally
			{
				manager.ReattachAllDetachedAnchorables();
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(stillDetached, Is.False, "Docking back on request should clear the detached flag.");
			Assert.That(openWindows, Is.Zero, "Docking back should close the standalone window.");
		});
	}

	/// <summary>A remembered position that no longer lands on a screen is discarded.</summary>
	/// <remarks>
	/// A layout saved on a multi monitor machine and restored on a single monitor would otherwise place
	/// the window where it cannot be reached or moved.
	/// </remarks>
	[Test]
	public void RestoringOffScreenPosition_FallsBackToCentre()
	{
		WindowStartupLocation? startupLocation = null;

		var failure = RunOnStaThread(() =>
		{
			var model = new LayoutAnchorable
			{
				Title = "Off screen",
				ContentId = "offscreen",
				FloatingLeft = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth + 5000,
				FloatingTop = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight + 5000,
				FloatingWidth = 300,
				FloatingHeight = 300,
			};

			// Constructing is enough - the geometry is applied in the constructor, nothing is shown.
			var window = new DetachedAnchorableWindow(model, new ContentPresenter());
			startupLocation = window.WindowStartupLocation;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.That(startupLocation, Is.EqualTo(WindowStartupLocation.CenterScreen),
			"An unreachable stored position should be replaced by centring the window.");
	}

	/// <summary>A remembered position that is still on screen is honoured.</summary>
	[Test]
	public void RestoringOnScreenPosition_IsHonoured()
	{
		WindowStartupLocation? startupLocation = null;
		var left = double.NaN;
		var top = double.NaN;

		var failure = RunOnStaThread(() =>
		{
			var expectedLeft = SystemParameters.VirtualScreenLeft + 120;
			var expectedTop = SystemParameters.VirtualScreenTop + 140;

			var model = new LayoutAnchorable
			{
				Title = "On screen",
				ContentId = "onscreen",
				FloatingLeft = expectedLeft,
				FloatingTop = expectedTop,
				FloatingWidth = 300,
				FloatingHeight = 300,
			};

			var window = new DetachedAnchorableWindow(model, new ContentPresenter());
			startupLocation = window.WindowStartupLocation;
			left = window.Left;
			top = window.Top;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(startupLocation, Is.EqualTo(WindowStartupLocation.Manual));
			Assert.That(left, Is.EqualTo(SystemParameters.VirtualScreenLeft + 120));
			Assert.That(top, Is.EqualTo(SystemParameters.VirtualScreenTop + 140));
		});
	}

	/// <summary>Builds a layout holding a single dockable anchorable.</summary>
	/// <param name="anchorable">Receives the anchorable in the layout.</param>
	/// <returns>The layout root.</returns>
	private static LayoutRoot BuildLayout(out LayoutAnchorable anchorable)
	{
		anchorable = new LayoutAnchorable { Title = "Tool", ContentId = "tool", Content = new Grid() };

		var pane = new LayoutAnchorablePane(anchorable);
		var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
		panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

		return new LayoutRoot { RootPanel = panel };
	}

	/// <summary>Creates a docking manager holding a single anchorable.</summary>
	/// <param name="anchorable">Receives the anchorable in the manager's layout.</param>
	/// <returns>The manager.</returns>
	private static DockingManager CreateManager(out LayoutAnchorable anchorable)
	{
		var manager = new DockingManager();
		manager.Layout = BuildLayout(out anchorable);
		return manager;
	}

	/// <summary>
	/// Puts the manager into a window and shows it, so that the loaded and unloaded paths under test
	/// actually run.
	/// </summary>
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

			// Kept off screen: these tests are about lifecycle, not about what is drawn.
			Left = SystemParameters.VirtualScreenLeft - 10000,
			Top = SystemParameters.VirtualScreenTop - 10000,
		};

		host.Show();
		Pump();
		return host;
	}

	/// <summary>
	/// Lets the dispatcher run pending work, so that events raised at a lower priority - Unloaded
	/// among them - have happened before anything is asserted.
	/// </summary>
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