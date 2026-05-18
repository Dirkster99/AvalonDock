using System.Windows;
using System.Windows.Controls;
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

    private void OnTreeViewItemDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: FileTreeItem { IsDirectory: false } item })
        {
            if (DataContext is FolderExplorerViewModel vm)
            {
                vm.OpenFileCommand.Execute(item);
            }
        }
    }
}
