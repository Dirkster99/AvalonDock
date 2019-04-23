using System.Windows;

namespace Test_Issue19
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
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
