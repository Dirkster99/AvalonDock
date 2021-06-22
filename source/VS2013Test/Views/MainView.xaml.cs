using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AvalonDock.VS2013Test.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

		private void MinimizeClick(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void MaximizeClick(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
		}

		private void RestoreDownClick(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
		}

		private void CloseClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
