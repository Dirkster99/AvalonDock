namespace Xceed.Wpf.AvalonDock.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;
    using Xceed.Wpf.AvalonDock.Layout;
    using Xceed.Wpf.AvalonDock.Test.TestHelpers;
    using Xceed.Wpf.AvalonDock.Test.views;

    [TestClass]
    public class AnchorablePaneTest: AutomationTestBase
    {
        [TestMethod]
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
