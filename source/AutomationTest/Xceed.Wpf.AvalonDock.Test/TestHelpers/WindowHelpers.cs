namespace Xceed.Wpf.AvalonDock.Test.TestHelpers
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    public static class WindowHelpers
    {
        public static Task<T> CreateInvisibleWindowAsync<T>(Action<T> changeAdditionalProperties = null) where T : Window, new()
        {
            var window = new T()
            {
                Visibility = Visibility.Hidden,
                ShowInTaskbar = false
            };

            changeAdditionalProperties?.Invoke(window);

            var completionSource = new TaskCompletionSource<T>();

            EventHandler handler = null;

            handler = (sender, args) =>
            {
                window.Activated -= handler;
                completionSource.SetResult(window);
            };

            window.Activated += handler;
            
            window.Show();

            return completionSource.Task;
        }
    }
}
