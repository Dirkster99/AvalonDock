#nullable enable
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDockTest.TestHelpers;
using NUnit.Framework;
using UnitTests;

namespace AvalonDockTest;

/// <summary>
/// Regression tests for the classic auto-hide flyout guards:
/// selecting or activating an auto-hidden anchorable must never open the classic
/// flyout under the <see cref="ToggleDockingManager"/> (which presents auto-hidden
/// anchorables through its sidebar toggle buttons, see
/// <see cref="DockingManager.SupportsAutoHideFlyout"/>), and must not crash when the
/// anchorable's parent chain is detached while the layout is being restructured
/// (e.g. by the ToggleLayoutEngine during OpenDefaultToolboxes).
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class AutoHideFlyoutGuardTests : AutomationTestBase
{
	/// <summary>
	/// The classic docking manager supports the auto-hide flyout.
	/// </summary>
	[Test]
	public void DockingManager_SupportsAutoHideFlyout()
	{
		bool? supportsFlyout = null;
		var failure = RunOnAppThread(() =>
		{
			AvalonDock.Core.IDockingManager manager = new DockingManager();
			supportsFlyout = manager.SupportsAutoHideFlyout;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.That(supportsFlyout, Is.True);
	}

	/// <summary>
	/// The toggle docking manager replaces the flyout with its sidebar toggle buttons.
	/// </summary>
	[Test]
	public void ToggleDockingManager_DoesNotSupportAutoHideFlyout()
	{
		bool? supportsFlyout = null;
		var failure = RunOnAppThread(() =>
		{
			AvalonDock.Core.IDockingManager manager = new ToggleDockingManager();
			supportsFlyout = manager.SupportsAutoHideFlyout;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.That(supportsFlyout, Is.False);
	}

	/// <summary>
	/// Selecting an auto-hidden anchorable under the ToggleDockingManager must not
	/// try to open the classic flyout (which crashed with a NullReferenceException)
	/// and must reset the selection like the classic anchor behavior does.
	/// </summary>
	[Test]
	public void SelectingAutoHiddenAnchorable_UnderToggleDockingManager_DoesNotOpenFlyout()
	{
		bool? isSelectedAfter = null;
		var failure = RunOnAppThread(() =>
		{
			var manager = new ToggleDockingManager();
			var anchorable = AddAutoHiddenAnchorable(manager, out _);
			CreateAnchorControl(anchorable);

			anchorable.IsSelected = true;

			isSelectedAfter = anchorable.IsSelected;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.That(isSelectedAfter, Is.False);
	}

	/// <summary>
	/// Activating an auto-hidden anchorable under the ToggleDockingManager
	/// (e.g. because the previously active content was removed and AvalonDock
	/// activates the next content) must not try to open the classic flyout.
	/// </summary>
	[Test]
	public void ActivatingAutoHiddenAnchorable_UnderToggleDockingManager_DoesNotOpenFlyout()
	{
		var failure = RunOnAppThread(() =>
		{
			var manager = new ToggleDockingManager();
			var anchorable = AddAutoHiddenAnchorable(manager, out _);
			CreateAnchorControl(anchorable);

			anchorable.IsActive = true;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
	}

	/// <summary>
	/// While the layout is being restructured an auto-hidden anchorable can be selected
	/// in a state where its anchor group is detached from the side. The flyout can not
	/// be shown then and the selection must be handled without an exception.
	/// </summary>
	[Test]
	public void SelectingAutoHiddenAnchorable_WithDetachedAnchorGroup_DoesNotThrow()
	{
		bool? isSelectedAfter = null;
		var failure = RunOnAppThread(() =>
		{
			var manager = new DockingManager();
			var anchorable = AddAutoHiddenAnchorable(manager, out var group);
			CreateAnchorControl(anchorable);

			// Simulate the restructuring: the anchor group is detached from its side
			// while the anchorable is still auto-hidden inside it
			manager.Layout.LeftSide.Children.Remove(group);

			anchorable.IsSelected = true;

			isSelectedAfter = anchorable.IsSelected;
		});

		Assert.That(failure, Is.Null, failure?.ToString());
		Assert.That(isSelectedAfter, Is.False);
	}

	/// <summary>
	/// <see cref="LayoutAutoHideWindowControl"/>.Show must return gracefully instead of
	/// dereferencing the detached parent chain of the anchor's model
	/// (regression test for the NullReferenceException in Show).
	/// </summary>
	[Test]
	public void AutoHideWindowControl_Show_WithDetachedParentChain_DoesNotThrow()
	{
		var failure = RunOnAppThread(() =>
		{
			var manager = new DockingManager();
			var anchorable = AddAutoHiddenAnchorable(manager, out var group);
			var anchorControl = CreateAnchorControl(anchorable);
			manager.Layout.LeftSide.Children.Remove(group);

			var autoHideWindow = (LayoutAutoHideWindowControl)Activator.CreateInstance(
				typeof(LayoutAutoHideWindowControl),
				nonPublic: true)!;

			InvokeShow(autoHideWindow, anchorControl);
		});

		Assert.That(failure, Is.Null, failure?.ToString());
	}

	/// <summary>
	/// Adds an anchorable in auto-hidden state (inside an anchor group of the left side)
	/// to a fresh layout of the manager.
	/// </summary>
	/// <param name="manager">The docking manager.</param>
	/// <param name="group">The anchor group hosting the anchorable.</param>
	/// <returns>The auto-hidden anchorable.</returns>
	private static LayoutAnchorable AddAutoHiddenAnchorable(DockingManager manager, out LayoutAnchorGroup group)
	{
		var layout = new LayoutRoot();
		manager.Layout = layout;

		var anchorable = new LayoutAnchorable { Title = "Tool", ContentId = "tool" };
		group = new LayoutAnchorGroup();
		group.Children.Add(anchorable);
		layout.LeftSide.Children.Add(group);

		Assert.That(anchorable.IsAutoHidden, Is.True, "test setup: the anchorable has to be auto-hidden");
		return anchorable;
	}

	/// <summary>
	/// Creates the anchor control for the model, which subscribes to the selection and
	/// activation events like the classic side panels do. The constructor is internal,
	/// so the control is created via reflection.
	/// </summary>
	/// <param name="model">The auto-hidden anchorable.</param>
	/// <returns>The anchor control.</returns>
	private static LayoutAnchorControl CreateAnchorControl(LayoutAnchorable model)
	{
		return (LayoutAnchorControl)Activator.CreateInstance(
			typeof(LayoutAnchorControl),
			BindingFlags.Instance | BindingFlags.NonPublic,
			null,
			new object[] { model },
			null)!;
	}

	/// <summary>
	/// Invokes the internal Show method and unwraps the reflection exception so the
	/// original exception (e.g. a NullReferenceException) surfaces in the test result.
	/// </summary>
	/// <param name="window">The auto-hide window control.</param>
	/// <param name="anchor">The anchor control.</param>
	private static void InvokeShow(LayoutAutoHideWindowControl window, LayoutAnchorControl anchor)
	{
		var show = typeof(LayoutAutoHideWindowControl).GetMethod(
			"Show",
			BindingFlags.Instance | BindingFlags.NonPublic,
			null,
			new[] { typeof(LayoutAnchorControl) },
			null)!;

		try
		{
			show.Invoke(window, new object[] { anchor });
		}
		catch (TargetInvocationException ex) when (ex.InnerException != null)
		{
			throw ex.InnerException;
		}
	}

	/// <summary>
	/// Runs the action on the shared application dispatcher and captures any exception,
	/// because <see cref="ThreadExecutor.RunCodeAsSTA"/> swallows exceptions thrown on
	/// its worker thread.
	/// </summary>
	/// <param name="action">The test body.</param>
	/// <returns>The captured exception or null.</returns>
	private Exception? RunOnAppThread(Action action)
	{
		Exception? failure = null;
		ThreadExecutor.RunCodeAsSTA(
			_are,
			() =>
			{
				try
				{
					Application.Current.Dispatcher.Invoke(action);
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
