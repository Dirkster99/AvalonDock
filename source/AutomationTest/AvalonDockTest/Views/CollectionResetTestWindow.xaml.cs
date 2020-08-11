using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AvalonDockTest.Views
{
	/// <summary>
	/// Interaction logic for CollectionResetTestWindow.xaml
	/// </summary>
	public partial class CollectionResetTestWindow : Window
	{
		public CollectionResetTestWindow()
		{
			InitializeComponent();
			DataContext = this;
			Anchorables = new CustomObservableCollection<object>();
			Documents = new CustomObservableCollection<object>();
		}

		public CustomObservableCollection<object> Anchorables { get; private set; }
		public CustomObservableCollection<object> Documents { get; private set; }
	}
}
