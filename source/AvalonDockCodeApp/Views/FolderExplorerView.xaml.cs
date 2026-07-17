using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

public partial class FolderExplorerView : UserControl
{
	public FolderExplorerView()
	{
		InitializeComponent();
	}

	private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource is TreeViewItem { DataContext: FileTreeItem item })
		{
			item.LoadChildren();
		}
	}

	private void OnTreeViewItemDoubleClick(object sender, MouseButtonEventArgs e)
	{
		// Prevent event bubbling from child TreeViewItems
		if (e.Handled) return;

		if (sender is TreeView tree && tree.SelectedItem is FileTreeItem { IsDirectory: false } item)
		{
			if (DataContext is FolderExplorerViewModel vm)
			{
				vm.OpenFileCommand.Execute(item);
				e.Handled = true;
			}
		}
	}
}