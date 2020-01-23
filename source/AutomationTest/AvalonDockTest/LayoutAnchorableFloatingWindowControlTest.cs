namespace AvalonDockTest
{
	using System.Threading.Tasks;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using AvalonDock.Layout.Serialization;
	using AvalonDockTest.TestHelpers;
	using AvalonDockTest.Views;

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