namespace AvalonDockTest
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;
	using System.Threading.Tasks;
	using AvalonDock.Layout;
	using AvalonDockTest.TestHelpers;
	using AvalonDockTest.views;

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
	}
}
