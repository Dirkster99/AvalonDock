using System;
using System.Threading;
using System.Windows;
using UnitTests;

namespace AvalonDockTest
{
	using System.Threading.Tasks;

	using NUnit.Framework;

	using AvalonDock.Layout.Serialization;
	using TestHelpers;
	using AvalonDockTest.Views;

	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class LayoutAnchorableFloatingWindowControlTest : AutomationTestBase
	{
		[Test]
		public void CloseWithHiddenFloatingWindowsTest()
        {
            var manualResetEvent = new ManualResetEvent(false);
			ThreadExecutor.RunCodeAsSTA(_are,  void () =>
			{
                var window = new LayoutAnchorableFloatingWindowControlTestWindow()
                {
                    Visibility = Visibility.Hidden,
                    ShowInTaskbar = false
                };
                
                window.Show();
                
                window.Window1.Float();
				Assert.IsTrue(window.Window1.IsFloating);
				var layoutSerializer = new XmlLayoutSerializer(window.dockingManager);
				layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
				window.tabControl.SelectedIndex = 1;
				layoutSerializer.Deserialize(@".\AvalonDock.Layout.config");
				window.tabControl.SelectedIndex = 0;
				window.Close();
                manualResetEvent.Set();
			});

            manualResetEvent.WaitOne(3000);
			_are.WaitOne();
		}
	}
}