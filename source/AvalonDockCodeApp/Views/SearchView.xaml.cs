using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

public partial class SearchView : UserControl
{
	public SearchView()
	{
		InitializeComponent();
	}

	private void OnMatchClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is FrameworkElement fe && fe.DataContext is SearchMatch match
			&& DataContext is SearchViewModel vm)
		{
			vm.OpenMatchCommand.Execute(match);
		}
	}
}