namespace AvalonDockTest
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;
	using System.Threading.Tasks;
	using AvalonDock.Layout;
	using AvalonDockTest.TestHelpers;
	using AvalonDockTest.views;
	using AvalonDock;
	using System.Collections.Generic;
	using System.Linq;
	using AvalonDock.Controls;

	[STATestClass]
	public class AnchorablePaneTest : AutomationTestBase
	{
		[STATestMethod]
		public void AnchorablePaneHideCloseTest()
		{
			TestHost.SwitchToAppThread();

			Task<AnchorablePaneTestWindow> taskResult = WindowHelpers.CreateInvisibleWindowAsync<AnchorablePaneTestWindow>();

			taskResult.Wait();

			AnchorablePaneTestWindow windows = taskResult.Result;

			ILayoutContainer expectedContainer = windows.Screen3.Parent;
			windows.Screen3.Hide();
			Assert.IsTrue(windows.Screen3.IsHidden);
			windows.Screen2.Close();
			windows.Screen3.Show();
			Assert.IsFalse(windows.Screen3.IsHidden);
			ILayoutContainer actualContainer = windows.Screen3.Parent;
			Assert.AreEqual(expectedContainer, actualContainer);
		}

		[STATestMethod]
		public void AnchorablePaneHideCloseEventsFiredTest()
		{
			// Create the window with 2 LayoutAnchorable items

			TestHost.SwitchToAppThread();

			Task<AnchorablePaneTestWindow> taskResult = WindowHelpers.CreateInvisibleWindowAsync<AnchorablePaneTestWindow>();

			taskResult.Wait();

			AnchorablePaneTestWindow windows = taskResult.Result;
			DockingManager dockingManager = windows.dockingManager;

			// These lists hold a record of the anchorable hide and close events

			List<LayoutAnchorable> isHidingRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isHiddenRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isClosingRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isClosedRaised = new List<LayoutAnchorable>();

			// Event handlers for the hide and close events

			dockingManager.AnchorableClosing += (s, e) => isClosingRaised.Add(e.Anchorable);
			dockingManager.AnchorableClosed += (s, e) => isClosedRaised.Add(e.Anchorable);
			dockingManager.AnchorableHiding += (s, e) => isHidingRaised.Add(e.Anchorable);
			dockingManager.AnchorableHidden += (s, e) => isHiddenRaised.Add(e.Anchorable);

			// Get the visual items

			LayoutAnchorableItem item2 = dockingManager.GetLayoutItemFromModel(windows.Screen2) as LayoutAnchorableItem;
			LayoutAnchorableItem item3 = dockingManager.GetLayoutItemFromModel(windows.Screen3) as LayoutAnchorableItem;

			Assert.IsNotNull(item2);
			Assert.IsNotNull(item3);

			// Ensure the items can be hidden and closed

			item2.CanClose = true;
			item3.CanClose = true;
			item2.CanHide = true;
			item3.CanHide = true;

			// Hide item3

			item3.HideCommand.Execute(null);

			// Ensure only item3 is hidden, and check the correct events were fired

			Assert.IsFalse(windows.Screen2.IsHidden);
			Assert.IsTrue(windows.Screen3.IsHidden);
			Assert.AreEqual(1, isHidingRaised.Count);
			Assert.AreEqual(1, isHiddenRaised.Count);
			Assert.AreEqual(0, isClosingRaised.Count);
			Assert.AreEqual(0, isClosedRaised.Count);

			Assert.AreEqual(windows.Screen3, isHidingRaised.First());
			Assert.AreEqual(windows.Screen3, isHiddenRaised.First());

			isHidingRaised.Clear();
			isHiddenRaised.Clear();

			// Close item2

			item2.CloseCommand.Execute(null);

			// Check the correct events were fired

			Assert.AreEqual(0, isHidingRaised.Count);
			Assert.AreEqual(0, isHiddenRaised.Count);
			Assert.AreEqual(1, isClosingRaised.Count);
			Assert.AreEqual(1, isClosedRaised.Count);

			Assert.AreEqual(windows.Screen2, isClosingRaised.First());
			Assert.AreEqual(windows.Screen2, isClosedRaised.First());
		}

		[STATestMethod]
		public void AnchorablePaneHideCloseEventsCancelledTest()
		{
			// Create the window with 2 LayoutAnchorable items

			TestHost.SwitchToAppThread();

			Task<AnchorablePaneTestWindow> taskResult = WindowHelpers.CreateInvisibleWindowAsync<AnchorablePaneTestWindow>();

			taskResult.Wait();

			AnchorablePaneTestWindow windows = taskResult.Result;
			DockingManager dockingManager = windows.dockingManager;

			// These lists hold a record of the anchorable hide and close events

			List<LayoutAnchorable> isHidingRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isHiddenRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isClosingRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isClosedRaised = new List<LayoutAnchorable>();

			// Event handlers for the hide and close events

			dockingManager.AnchorableClosing += (s, e) =>
			{
				e.Cancel = true;
				isClosingRaised.Add(e.Anchorable);
			};
			dockingManager.AnchorableClosed += (s, e) => isClosedRaised.Add(e.Anchorable);
			dockingManager.AnchorableHiding += (s, e) =>
			{
				e.Cancel = true;
				isHidingRaised.Add(e.Anchorable);
			};
			dockingManager.AnchorableHidden += (s, e) => isHiddenRaised.Add(e.Anchorable);

			// Get the visual items

			LayoutAnchorableItem item2 = dockingManager.GetLayoutItemFromModel(windows.Screen2) as LayoutAnchorableItem;
			LayoutAnchorableItem item3 = dockingManager.GetLayoutItemFromModel(windows.Screen3) as LayoutAnchorableItem;

			Assert.IsNotNull(item2);
			Assert.IsNotNull(item3);

			// Ensure the items can be hidden and closed

			item2.CanClose = true;
			item3.CanClose = true;
			item2.CanHide = true;
			item3.CanHide = true;

			// Hide item3

			item3.HideCommand.Execute(null);

			// Ensure nothing was hidden but cancelled instead, and check the correct events were fired

			Assert.IsFalse(windows.Screen2.IsHidden);
			Assert.IsFalse(windows.Screen3.IsHidden);
			Assert.AreEqual(1, isHidingRaised.Count);
			Assert.AreEqual(0, isHiddenRaised.Count);
			Assert.AreEqual(0, isClosingRaised.Count);
			Assert.AreEqual(0, isClosedRaised.Count);

			Assert.AreEqual(windows.Screen3, isHidingRaised.First());

			isHidingRaised.Clear();

			// Close item2

			item2.CloseCommand.Execute(null);

			// Ensure nothing was closed, and check the correct events were fired

			Assert.AreEqual(0, isHidingRaised.Count);
			Assert.AreEqual(0, isHiddenRaised.Count);
			Assert.AreEqual(1, isClosingRaised.Count);
			Assert.AreEqual(0, isClosedRaised.Count);

			Assert.AreEqual(windows.Screen2, isClosingRaised.First());
		}

		[STATestMethod]
		public void AnchorablePaneHideEventRedirectTest()
		{
			// Create the window with 2 LayoutAnchorable items

			TestHost.SwitchToAppThread();

			Task<AnchorablePaneTestWindow> taskResult = WindowHelpers.CreateInvisibleWindowAsync<AnchorablePaneTestWindow>();

			taskResult.Wait();

			AnchorablePaneTestWindow windows = taskResult.Result;
			DockingManager dockingManager = windows.dockingManager;

			// These lists hold a record of the anchorable hide and close events

			List<LayoutAnchorable> isHidingRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isHiddenRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isClosingRaised = new List<LayoutAnchorable>();
			List<LayoutAnchorable> isClosedRaised = new List<LayoutAnchorable>();

			// Event handlers for the hide and close events

			dockingManager.AnchorableClosing += (s, e) => isClosingRaised.Add(e.Anchorable);
			dockingManager.AnchorableClosed += (s, e) => isClosedRaised.Add(e.Anchorable);
			dockingManager.AnchorableHiding += (s, e) =>
			{
				e.CloseInsteadOfHide = true;
				isHidingRaised.Add(e.Anchorable);
			};
			dockingManager.AnchorableHidden += (s, e) => isHiddenRaised.Add(e.Anchorable);

			// Get the visual items

			LayoutAnchorableItem item2 = dockingManager.GetLayoutItemFromModel(windows.Screen2) as LayoutAnchorableItem;
			LayoutAnchorableItem item3 = dockingManager.GetLayoutItemFromModel(windows.Screen3) as LayoutAnchorableItem;

			Assert.IsNotNull(item2);
			Assert.IsNotNull(item3);

			// Ensure the items can be hidden and closed

			item2.CanClose = true;
			item3.CanClose = true;
			item2.CanHide = true;
			item3.CanHide = true;

			// Hide item3

			item3.HideCommand.Execute(null);

			// Ensure nothing was hidden but cancelled instead, and check the correct events were fired

			Assert.IsFalse(windows.Screen2.IsHidden);
			Assert.IsFalse(windows.Screen3.IsHidden);
			Assert.AreEqual(1, isHidingRaised.Count);
			Assert.AreEqual(0, isHiddenRaised.Count);
			Assert.AreEqual(1, isClosingRaised.Count);
			Assert.AreEqual(1, isClosedRaised.Count);

			Assert.AreEqual(windows.Screen3, isHidingRaised.First());
			Assert.AreEqual(windows.Screen3, isClosingRaised.First());
			Assert.AreEqual(windows.Screen3, isClosedRaised.First());
		}
	}
}
