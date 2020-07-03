using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AvalonDocPanelMemoryLeaks
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public HeavyLoad HeavyLoad { get; private set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserControl content = new UserControl();
            HeavyLoad = new HeavyLoad();
            content.DataContext = HeavyLoad;
            LayoutDocument docDocument = new LayoutDocument
            {
                Content = content
            };
            docGrup.Children.Add(docDocument);
            docDocument.Closed += DocClosed;
        }

        private void DocClosed(object sender, EventArgs e)
        {
            HeavyLoad.Load = null;
            GC.Collect();
        }
    }
    public class HeavyLoad:INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] Load { get; set; } = new byte[100000000];//100MB

       
    }
}
