using UnitTests;

namespace AvalonDockTest.TestHelpers
{
    using NUnit.Framework;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// This is the base class for all of our UI tests.
    /// </summary>
    [TestFixture]
    public abstract class AutomationTestBase
    {
        protected AutoResetEvent _are;
        
        [TearDown]
        public void After()
        {
            ThreadExecutor.RunCodeAsSTA(
                _are,
                () =>
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        var windows = Application.Current?.Windows?.OfType<Window>().ToList();
                        if (windows == null)
                        {
                            return;
                        }
                        
                        foreach (Window window in windows)
                        {
                            window.Close();
                        }
                    });
                });

            _are.WaitOne();
        }

        [OneTimeSetUp]
        public void ApplicationInitialize()
        {
            _are = new AutoResetEvent(false);
            ThreadExecutor.RunCodeAsSTA(
                _are,
                () =>
                {
                    Debug.WriteLine("OneTimeSetUp");
                    TestHost.Initialize();
                });

            _are.WaitOne();
        }

        [OneTimeTearDown]
        public void ApplicationCleanup()
        {
            // Never call Application.Current.Shutdown() here: the TestApp hosted by
            // TestHost is a process-wide singleton that cannot be recreated, and WPF's
            // Application.IsShuttingDown is a one-way latch. Shutting the app down in a
            // per-fixture OneTimeTearDown permanently breaks pack://application resource
            // loading (e.g. loading Themes/generic.xaml) for every fixture that runs
            // afterwards. The TestApp thread is a background thread, so the test runner
            // exits without an explicit shutdown.
            Debug.WriteLine("OneTimeTearDown");
            _are.Dispose();
        }
    }
}