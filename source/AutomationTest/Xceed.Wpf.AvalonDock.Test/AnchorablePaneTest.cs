namespace Xceed.Wpf.AvalonDock.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Xceed.Wpf.AvalonDock.Test.TestHelpers;
    using Xceed.Wpf.AvalonDock.Test.views;

    [TestClass]
    public class AnchorablePaneTest: AutomationTestBase
    {
        [TestMethod]
        public void TempTest()
        {
            TestHost.SwitchToAppThread();

            var window = WindowHelpers.CreateInvisibleWindowAsync<AnchorablePaneTestWindow>();
        }
    }
}
