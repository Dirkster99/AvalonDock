namespace Xceed.Wpf.AvalonDock.Test.TestHelpers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// This is the base class for all of our UI tests.
    /// </summary>
    [TestClass]
    public class AutomationTestBase
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Before()
        {
            var message = $"Setup for test '{this.TestContext.TestName}' with Thread.CurrentThread: {Thread.CurrentThread.ManagedThreadId}" +
                          $" and Current.Dispatcher.Thread: {Application.Current.Dispatcher.Thread.ManagedThreadId}";
            this.TestContext.WriteLine(message);
        }

        [TestCleanup]
        public void After()
        {
            Application.Current.Invoke(() =>
            {
                var windows = Application.Current.Windows.OfType<Window>().ToList();
                foreach (Window window in windows)
                {
                    window.Close();
                }
            });

            var message = $"TearDown for test '{this.TestContext.TestName}' with Thread.CurrentThread: {Thread.CurrentThread.ManagedThreadId}" +
                          $" and Current.Dispatcher.Thread: {Application.Current.Dispatcher.Thread.ManagedThreadId}";
            this.TestContext.WriteLine(message);
        }

        [AssemblyInitialize]
        public static void ApplicationInitialize(TestContext testContext)
        {
            Debug.WriteLine("AssemblyInitialize");
            TestHost.Initialize();
        }

        [AssemblyCleanup]
        public static void ApplicationCleanup()
        {
            GC.Collect();
            Dispatcher.ExitAllFrames();
            Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
            Debug.WriteLine("AssemblyCleanup");
        }
    }
}