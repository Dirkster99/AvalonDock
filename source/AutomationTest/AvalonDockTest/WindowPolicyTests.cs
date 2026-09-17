#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDockTest.TestHelpers;
using NUnit.Framework;
using UnitTests;

namespace AvalonDockTest;

/// <summary>
/// Covers the two manager wide switches that take floating windows and standalone windows away from
/// the user: <see cref="DockingManager.AllowFloatingWindows"/> and
/// <see cref="DockingManager.AllowDetachedWindows"/>.
/// </summary>
/// <remarks>
/// Both are meant to be absolute - an application that turns them off must not be able to end up with
/// such a window through any route, including loading a layout that was saved while they were still
/// allowed. That last case is what the restore tests here pin down.
/// </remarks>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class WindowPolicyTests : AutomationTestBase
{
	/// <summary>Both switches are on unless the application says otherwise.</summary>
	[Test]
	public void Defaults_AllowBothWindowKinds()
	{
		var allowFloating = false;
		var allowDetached = false;

		var failure = RunOnStaThread(() =>
		{
			var manager = new DockingManager();
			allowFloating = manager.AllowFloatingWindows;
			allowDetached = manager.AllowDetachedWindows;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(allowFloating, Is.True, "Floating has to stay available by default.");
			Assert.That(allowDetached, Is.True, "Standalone windows have to stay available by default.");
		});
	}

	/// <summary>Floating an anchorable does nothing while floating is switched off.</summary>
	[Test]
	public void FloatingDisabled_FloatCreatesNoWindow()
	{
		var floatingWindows = -1;
		var stillDocked = false;

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			manager.AllowFloatingWindows = false;

			var host = HostInWindow(manager);
			try
			{
				anchorable.Float();
				Pump();

				floatingWindows = manager.FloatingWindows.Count();
				stillDocked = anchorable.FindParent<LayoutFloatingWindow>() == null;
			}
			finally
			{
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(floatingWindows, Is.Zero, "No floating window may be created while floating is off.");
			Assert.That(stillDocked, Is.True, "The anchorable has to stay where it was docked.");
		});
	}

	/// <summary>The factory method reports the refusal rather than handing out a window.</summary>
	[Test]
	public void FloatingDisabled_CreateFloatingWindowReturnsNull()
	{
		object? created = new object();

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			manager.AllowFloatingWindows = false;

			var host = HostInWindow(manager);
			try
			{
				created = manager.CreateFloatingWindow(anchorable, false);
			}
			finally
			{
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.That(created, Is.Null, "CreateFloatingWindow has to refuse while floating is off.");
	}

	/// <summary>
	/// Switching floating off docks the windows that are already open, even when the pane the content
	/// came from is long gone.
	/// </summary>
	/// <remarks>
	/// Floating the only anchorable of a pane leaves that pane empty, and garbage collection removes it
	/// along with the record of where the content belongs. What is left has to end up in the layout all
	/// the same, and not back in the window it is being taken out of.
	/// </remarks>
	[Test]
	public void DisablingFloating_DocksOpenFloatingWindowsBack()
	{
		var floatingWhileAllowed = -1;
		var floatingAfterDisabling = -1;
		var backInLayout = false;

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			var host = HostInWindow(manager);
			try
			{
				anchorable.Float();
				Pump();
				floatingWhileAllowed = manager.FloatingWindows.Count();

				manager.AllowFloatingWindows = false;
				Pump();

				floatingAfterDisabling = manager.FloatingWindows.Count();
				backInLayout = anchorable.FindParent<LayoutFloatingWindow>() == null
					&& anchorable.Root == manager.Layout;
			}
			finally
			{
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(floatingWhileAllowed, Is.EqualTo(1), "Floating should have produced one window.");
			Assert.That(floatingAfterDisabling, Is.Zero, "Switching floating off has to close the open windows.");
			Assert.That(backInLayout, Is.True, "The content has to end up in the layout, not be lost with the window.");
		});
	}

	/// <summary>
	/// Content that still knows the pane it was floated from is returned to exactly that pane.
	/// </summary>
	/// <remarks>
	/// The counterpart of <see cref="DisablingFloating_DocksOpenFloatingWindowsBack"/>: a pane that keeps
	/// a second anchorable survives the float, so the record of where the floated one belongs survives
	/// with it and has to be honoured rather than the content being dropped somewhere plausible.
	/// </remarks>
	[Test]
	public void DisablingFloating_ReturnsContentToThePaneItCameFrom()
	{
		var floatingAfterDisabling = -1;
		var backInOriginalPane = false;

		var failure = RunOnStaThread(() =>
		{
			var layout = BuildLayoutWithTwoAnchorables(out var floated, out var stays);
			var manager = new DockingManager { Layout = layout };
			var host = HostInWindow(manager);
			try
			{
				var originalPane = floated.Parent;

				floated.Float();
				Pump();

				manager.AllowFloatingWindows = false;
				Pump();

				floatingAfterDisabling = manager.FloatingWindows.Count();
				backInOriginalPane = ReferenceEquals(floated.Parent, originalPane)
					&& ReferenceEquals(stays.Parent, originalPane);
			}
			finally
			{
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(floatingAfterDisabling, Is.Zero, "Switching floating off has to close the open windows.");
			Assert.That(backInOriginalPane, Is.True, "The content has to return to the pane it was floated from.");
		});
	}

	/// <summary>A layout carrying floating windows is docked flat when floating is off.</summary>
	/// <remarks>
	/// Restoring a layout saved before the switch was turned off would otherwise be a way around it.
	/// </remarks>
	[Test]
	public void FloatingDisabled_LoadedLayoutWithFloatingWindowsIsDocked()
	{
		var floatingWindows = -1;
		var floatingModels = -1;

		var failure = RunOnStaThread(() =>
		{
			var manager = new DockingManager { AllowFloatingWindows = false };
			var host = HostInWindow(manager);
			try
			{
				manager.Layout = BuildLayoutWithFloatingAnchorable();
				Pump();

				floatingWindows = manager.FloatingWindows.Count();
				floatingModels = manager.Layout.FloatingWindows.Count;
			}
			finally
			{
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(floatingWindows, Is.Zero, "No window may be created for a restored floating layout.");
			Assert.That(floatingModels, Is.Zero, "The restored floating windows have to be docked away.");
		});
	}

	/// <summary>Detaching an anchorable does nothing while standalone windows are switched off.</summary>
	[Test]
	public void DetachedWindowsDisabled_DetachDoesNothing()
	{
		var detached = true;
		var openWindows = -1;

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			manager.AllowDetachedWindows = false;

			var host = HostInWindow(manager);
			try
			{
				manager.DetachAnchorableToWindow(anchorable);
				Pump();

				detached = manager.IsDetached(anchorable);
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
			Assert.That(detached, Is.False, "The anchorable must not be marked detached.");
			Assert.That(openWindows, Is.Zero, "No standalone window may be opened.");
		});
	}

	/// <summary>Switching standalone windows off returns the ones that are already open.</summary>
	[Test]
	public void DisablingDetachedWindows_ReattachesOpenWindows()
	{
		var detachedWhileAllowed = false;
		var detachedAfterDisabling = true;
		var openWindows = -1;

		var failure = RunOnStaThread(() =>
		{
			var manager = CreateManager(out var anchorable);
			var host = HostInWindow(manager);
			try
			{
				manager.DetachAnchorableToWindow(anchorable);
				Pump();
				detachedWhileAllowed = manager.IsDetached(anchorable);

				manager.AllowDetachedWindows = false;
				Pump();

				detachedAfterDisabling = manager.IsDetached(anchorable);
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
			Assert.That(detachedWhileAllowed, Is.True, "The anchorable should be detached to begin with.");
			Assert.That(detachedAfterDisabling, Is.False, "Switching the mode off has to return the anchorable.");
			Assert.That(openWindows, Is.Zero, "Switching the mode off has to close the standalone windows.");
		});
	}

	/// <summary>A layout that asks for standalone windows is restored docked when they are off.</summary>
	[Test]
	public void DetachedWindowsDisabled_RestoredLayoutStaysDocked()
	{
		var openWindows = -1;
		var stillFlagged = true;

		var failure = RunOnStaThread(() =>
		{
			var manager = new DockingManager { AllowDetachedWindows = false };
			var host = HostInWindow(manager);
			try
			{
				var layout = BuildLayout(out var anchorable);
				anchorable.IsDetached = true;

				manager.Layout = layout;
				Pump();

				openWindows = manager.DetachedAnchorables.Count();
				stillFlagged = anchorable.IsDetached;
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
			Assert.That(openWindows, Is.Zero, "A saved standalone window must not be recreated while the mode is off.");
			Assert.That(stillFlagged, Is.False, "The model must not keep claiming to be detached without a window.");
		});
	}

	/// <summary>The MVVM layout decides the policy for the manager it is bound to.</summary>
	[Test]
	public void DockLayout_PushesWindowPolicyToTheManager()
	{
		var allowFloatingAfterBinding = true;
		var allowDetachedAfterBinding = true;
		var allowFloatingAfterChange = false;

		var failure = RunOnStaThread(() =>
		{
			var manager = new DockingManager();
			var host = HostInWindow(manager);
			try
			{
				var root = new AvalonDock.Mvvm.RootDock
				{
					Id = "Root",
					AllowFloatingWindows = false,
					AllowDetachedWindows = false,
				};

				manager.DockLayout = root;
				Pump();

				allowFloatingAfterBinding = manager.AllowFloatingWindows;
				allowDetachedAfterBinding = manager.AllowDetachedWindows;

				root.AllowFloatingWindows = true;
				Pump();

				allowFloatingAfterChange = manager.AllowFloatingWindows;
			}
			finally
			{
				host.Close();
			}
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.Multiple(() =>
		{
			Assert.That(allowFloatingAfterBinding, Is.False, "Binding a layout has to apply its floating policy.");
			Assert.That(allowDetachedAfterBinding, Is.False, "Binding a layout has to apply its standalone window policy.");
			Assert.That(allowFloatingAfterChange, Is.True, "A later change of the layout has to reach the manager too.");
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

	/// <summary>
	/// Builds a layout with two anchorables in one pane, so that floating one of them leaves the pane
	/// standing and the record of where it belongs intact.
	/// </summary>
	/// <param name="floated">Receives the anchorable the test floats.</param>
	/// <param name="stays">Receives the anchorable that keeps the pane alive.</param>
	/// <returns>The layout root.</returns>
	private static LayoutRoot BuildLayoutWithTwoAnchorables(out LayoutAnchorable floated, out LayoutAnchorable stays)
	{
		floated = new LayoutAnchorable { Title = "Floated", ContentId = "floated", Content = new Grid() };
		stays = new LayoutAnchorable { Title = "Stays", ContentId = "stays", Content = new Grid() };

		var pane = new LayoutAnchorablePane(floated);
		pane.Children.Add(stays);

		var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
		panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

		return new LayoutRoot { RootPanel = panel };
	}

	/// <summary>
	/// Builds a layout whose anchorable sits in a floating window, the shape a layout saved with a
	/// floating tool window has.
	/// </summary>
	/// <returns>The layout root.</returns>
	private static LayoutRoot BuildLayoutWithFloatingAnchorable()
	{
		var docked = new LayoutAnchorable { Title = "Docked", ContentId = "docked", Content = new Grid() };
		var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
		panel.Children.Add(new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(docked)));

		var layout = new LayoutRoot { RootPanel = panel };

		var floated = new LayoutAnchorable { Title = "Floated", ContentId = "floated", Content = new Grid() };
		layout.FloatingWindows.Add(new LayoutAnchorableFloatingWindow
		{
			RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(floated))
			{
				DockWidth = new System.Windows.GridLength(200),
				DockHeight = new System.Windows.GridLength(200),
			},
		});

		return layout;
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