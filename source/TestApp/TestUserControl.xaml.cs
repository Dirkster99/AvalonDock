using System.Windows;
using System.Windows.Controls;

namespace TestApp
{
	/// <summary>
	/// Interaction logic for TestUserControl.xaml
	/// </summary>
	public partial class TestUserControl : UserControl
	{
		public TestUserControl()
		{
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(TestUserControl_Loaded);
			this.Unloaded += new RoutedEventHandler(TestUserControl_Unloaded);
		}

		void TestUserControl_Unloaded(object sender, RoutedEventArgs e)
		{

		}

		void TestUserControl_Loaded(object sender, RoutedEventArgs e)
		{

		}
	}
}
