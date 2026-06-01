using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Sample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public global::AvalonDock.DockingManager GetDockManager() => DockManager;

    private void OnSaveLayoutClick(object sender, RoutedEventArgs e)
    {
        _ = DockDiagnostics.CacheContent();
        _ = DockDiagnostics.SaveLayout();
    }

    private void OnLoadLayoutClick(object sender, RoutedEventArgs e)
    {
        _ = DockDiagnostics.LoadLayout();
    }

    private void OnFloatActiveClick(object sender, RoutedEventArgs e)
    {
        var layout = DockManager.Layout;
        var docPane = layout?.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
        if (docPane?.SelectedContent != null)
        {
            DockManager.FloatContent(docPane.SelectedContent);
        }
    }

    private void OnShowCompassClick(object sender, RoutedEventArgs e)
        => DockDiagnostics.ShowCompass();
}
