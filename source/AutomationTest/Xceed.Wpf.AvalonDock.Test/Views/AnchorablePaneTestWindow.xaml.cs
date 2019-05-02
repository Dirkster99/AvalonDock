using System.Windows;

namespace Xceed.Wpf.AvalonDock.Test.views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AnchorablePaneTestWindow : Window
    {
        public AnchorablePaneTestWindow()
        {
            InitializeComponent();
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            var test = this.dockingManager;
            Screen3.Hide();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Screen2.Close();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            Screen3.Show();
        }
    }
}
