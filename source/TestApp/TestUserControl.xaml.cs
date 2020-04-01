/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

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
			this.SizeChanged += TestUserControl_SizeChanged;
		}

		private void TestUserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
		}

		void TestUserControl_Unloaded(object sender, RoutedEventArgs e)
		{

		}

		void TestUserControl_Loaded(object sender, RoutedEventArgs e)
		{

		}
	}
}
