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
            ThreadExecutor.RunCodeAsSTA(
                _are,
                () =>
                {
                    GC.Collect();
                    Dispatcher.ExitAllFrames();
                    Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                    Debug.WriteLine("OneTimeTearDown");
                });

            _are.WaitOne();
            _are.Dispose();
        }
    }
}