namespace Xceed.Wpf.AvalonDock.Test
{
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Xceed.Wpf.AvalonDock.Layout.Serialization;
    using Xceed.Wpf.AvalonDock.Test.TestHelpers;
    using Xceed.Wpf.AvalonDock.Test.Views;

    [TestClass]
    public class LayoutAnchorableFloatingWindowControlTest : AutomationTestBase
    {
        [TestMethod]
        public async Task CloseWithHiddenFloatingWindowsTest()
        {
            LayoutAnchorableFloatingWindowControlTestWindow window = await WindowHelpers.CreateInvisibleWindowAsync<LayoutAnchorableFloatingWindowControlTestWindow>();
            window.Window1.Float();
            Assert.IsTrue(window.Window1.IsFloating);
            var layoutSerializer = new XmlLayoutSerializer(window.dockingManager);
            layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
            window.tabControl.SelectedIndex = 1;
            layoutSerializer.Deserialize(@".\AvalonDock.Layout.config");
            window.tabControl.SelectedIndex = 0;
            window.Close();
        }
    }
}