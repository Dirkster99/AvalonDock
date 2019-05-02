namespace Xceed.Wpf.AvalonDock.Test.TestHelpers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Threading;

    public class SwitchContextToUiThreadAwaiter : INotifyCompletion
    {
        private readonly Dispatcher uiContext;

        public SwitchContextToUiThreadAwaiter(Dispatcher uiContext)
        {
            this.uiContext = uiContext;
        }

        public SwitchContextToUiThreadAwaiter GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted { get { return false; } }

        public void OnCompleted(Action continuation)
        {
            this.uiContext.Invoke(new Action(continuation));
        }

        public void GetResult() { }
    }
}
