/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

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
