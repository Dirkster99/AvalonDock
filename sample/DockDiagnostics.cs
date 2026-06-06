using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Controls;
using LeXtudio.DevFlow.Agent.Core;

namespace AvalonDock.Sample;

public static class DockDiagnostics
{
    private static readonly string LayoutPath =
        Path.Combine(Path.GetTempPath(), "avalondock_sample_layout.xml");

    private static global::AvalonDock.DockingManager? GetDockManager()
        => (Application.Current as App)?.DockManager;

    private static T RunOnUi<T>(Func<T> callback)
        => Application.Current.Dispatcher.Invoke(callback);

    private static readonly Dictionary<string, object> ContentCache = new();

    [DevFlowAction("dock-save-layout",
        Description = "Serialize current DockingManager layout to the temp layout file.")]
    public static string SaveLayout() => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        var serializer = new XmlLayoutSerializer(dockManager);
        serializer.Serialize(LayoutPath);
        var size = new FileInfo(LayoutPath).Length;
        return $"saved {size} bytes to {LayoutPath}";
    });

    [DevFlowAction("dock-load-layout",
        Description = "Deserialize layout from the temp layout file.")]
    public static string LoadLayout() => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        if (!File.Exists(LayoutPath))
        {
            return $"file not found: {LayoutPath}";
        }

        var serializer = new XmlLayoutSerializer(dockManager);
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            var contentId = args.Model.ContentId;
            if (contentId != null && ContentCache.TryGetValue(contentId, out var content))
            {
                args.Content = content;
                return;
            }

            args.Cancel = true;
        };

        serializer.Deserialize(LayoutPath);

        var docPanes = dockManager.Layout?.Descendents().OfType<LayoutDocumentPane>().ToList() ?? new();
        return $"loaded from {LayoutPath} docPanes={docPanes.Count} " +
               $"tabCounts=[{string.Join(",", docPanes.Select(p => p.ChildrenCount))}]";
    });

    [DevFlowAction("dock-cache-content",
        Description = "Cache current content before layout save.")]
    public static string CacheContent() => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        ContentCache.Clear();
        foreach (var item in dockManager.Layout?.Descendents().OfType<LayoutContent>() ?? Enumerable.Empty<LayoutContent>())
        {
            if (item.ContentId != null && item.Content != null)
            {
                ContentCache[item.ContentId] = item.Content;
            }
        }

        return $"cached {ContentCache.Count} items: [{string.Join(",", ContentCache.Keys)}]";
    });

    [DevFlowAction("dock-toggle-autohide",
        Description = "Toggle auto-hide for anchorable with given ContentId.")]
    public static string ToggleAutoHide(string contentId) => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        var anchorable = dockManager.Layout?.Descendents().OfType<LayoutAnchorable>()
            .FirstOrDefault(a => a.ContentId == contentId);
        if (anchorable == null)
        {
            return $"anchorable '{contentId}' not found";
        }

        var wasDocked = !anchorable.IsAutoHidden;
        anchorable.ToggleAutoHide();
        var leftCount = dockManager.Layout?.LeftSide?.Children.Sum(group => group.Children.Count) ?? 0;
        var rightCount = dockManager.Layout?.RightSide?.Children.Sum(group => group.Children.Count) ?? 0;
        return $"{anchorable.Title}: was docked={wasDocked} now isAutoHidden={anchorable.IsAutoHidden} " +
               $"leftSide={leftCount} rightSide={rightCount}";
    });

    [DevFlowAction("dock-open-flyout",
        Description = "Open the auto-hide flyout for an anchorable by ContentId.")]
    public static string OpenFlyout(string contentId) => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        var anchorable = dockManager.Layout?.Descendents().OfType<LayoutAnchorable>()
            .FirstOrDefault(a => a.ContentId == contentId && a.IsAutoHidden);
        if (anchorable == null)
        {
            return $"anchorable '{contentId}' not found or not auto-hidden";
        }

        var anchor = FindVisualDescendants<LayoutAnchorControl>(dockManager)
            .FirstOrDefault(control => ReferenceEquals(control.Model, anchorable));
        if (anchor == null)
        {
            return $"anchor control for '{contentId}' not found";
        }

        var showMethod = typeof(global::AvalonDock.DockingManager).GetMethod(
            "ShowAutoHideWindow",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (showMethod == null)
        {
            return "ShowAutoHideWindow unavailable";
        }

        showMethod.Invoke(dockManager, new object[] { anchor });
        var isVisible = dockManager.AutoHideWindow?.Visibility == Visibility.Visible;
        var title = anchorable.Title ?? contentId;
        return $"flyout opened for {title} isOpen={isVisible}";
    });

    [DevFlowAction("dock-hide-anchorable",
        Description = "Hide the anchorable with the given ContentId.")]
    public static string HideAnchorable(string contentId) => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        var anchorable = dockManager.Layout?.Descendents().OfType<LayoutAnchorable>()
            .FirstOrDefault(a => a.ContentId == contentId);
        if (anchorable == null)
        {
            return $"anchorable '{contentId}' not found";
        }

        anchorable.Hide();
        return $"hidden: {anchorable.Title} isHidden={anchorable.IsHidden} hiddenCount={dockManager.Layout?.Hidden?.Count ?? 0}";
    });

    [DevFlowAction("dock-show-hidden",
        Description = "Show the first hidden anchorable.")]
    public static string ShowHidden() => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        var hidden = dockManager.Layout?.Hidden?.ToList() ?? new();
        if (hidden.Count == 0)
        {
            return "no hidden anchorables";
        }

        var anchorable = hidden[0];
        var title = anchorable.Title;
        anchorable.Show();
        return $"shown: {title} isVisible={anchorable.IsVisible} hiddenRemaining={dockManager.Layout?.Hidden?.Count ?? 0}";
    });

    [DevFlowAction("dock-list-hidden",
        Description = "List all hidden anchorables.")]
    public static string ListHidden() => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        var hidden = dockManager.Layout?.Hidden?.ToList() ?? new();
        if (hidden.Count == 0)
        {
            return "no hidden anchorables";
        }

        return string.Join(", ", hidden.Select(a => $"{a.Title}[{a.ContentId}]"));
    });

    [DevFlowAction("dock-active-content",
        Description = "Return the current active content.")]
    public static string GetActiveContent() => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        var active = dockManager.ActiveContent;
        var layoutActive = dockManager.Layout?.ActiveContent;
        return $"ActiveContent={active?.GetType().Name ?? "null"} LayoutActive={layoutActive?.Title ?? "null"}";
    });

    [DevFlowAction("dock-manager-border-probe",
        Description = "Report DockingManager template-root visual chain and border values.")]
    public static string ManagerBorderProbe() => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        dockManager.ApplyTemplate();
        return string.Join("; ", EnumerateVisuals(dockManager, 3).Select(DescribeVisual));
    });

    [DevFlowAction("dock-border-scan",
        Description = "Report visible Border elements under DockingManager up to the given visual-tree depth.")]
    public static string BorderScan(int maxDepth = 8) => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        dockManager.ApplyTemplate();
        var rows = EnumerateVisuals(dockManager, maxDepth)
            .Where(item => item.Visual is System.Windows.Controls.Border)
            .Select(DescribeVisual)
            .ToList();

        return rows.Count == 0 ? "no Border visuals" : string.Join("; ", rows);
    });

    [DevFlowAction("dock-tool-pane-border-probe",
        Description = "Report the first visible tool pane visual tree, including border layers and tab items.")]
    public static string ToolPaneBorderProbe(int maxDepth = 10) => RunOnUi(() =>
    {
        var dockManager = GetDockManager();
        if (dockManager == null)
        {
            return "no DockingManager";
        }

        dockManager.ApplyTemplate();
        var pane = FindVisualDescendants<LayoutAnchorablePaneControl>(dockManager)
            .FirstOrDefault(p => p.IsVisible && p.ActualWidth > 0 && p.ActualHeight > 0);
        if (pane == null)
        {
            return "no visible LayoutAnchorablePaneControl";
        }

        pane.ApplyTemplate();
        var rows = EnumerateVisuals(pane, maxDepth)
            .Where(item => item.Visual is System.Windows.Controls.Border
                           || item.Visual is System.Windows.Controls.TabItem
                           || item.Visual is LayoutAnchorableControl
                           || item.Visual.GetType().Name.Contains("AnchorablePaneTabPanel"))
            .Select(DescribeVisual)
            .ToList();

        return rows.Count == 0 ? "no tool pane visuals" : string.Join("; ", rows);
    });

    [DevFlowAction("dock-show-compass",
        Description = "Show overlay compass for parity screenshots (mirrors Uno ShowOverlayForDiagnostics).")]
    public static string ShowCompass() => RunOnUi(() =>
    {
        var dm = GetDockManager();
        if (dm == null) return "no DockingManager";
        dm.ShowOverlayForDiagnostics();
        return "compass shown";
    });

    private static IEnumerable<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root == null)
        {
            yield break;
        }

        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var nested in FindVisualDescendants<T>(child))
            {
                yield return nested;
            }
        }
    }

    private static IEnumerable<(DependencyObject Visual, int Depth)> EnumerateVisuals(DependencyObject root, int maxDepth)
    {
        var stack = new Stack<(DependencyObject Visual, int Depth)>();
        stack.Push((root, 0));
        while (stack.Count > 0)
        {
            var (visual, depth) = stack.Pop();
            yield return (visual, depth);
            if (depth >= maxDepth)
            {
                continue;
            }

            var count = VisualTreeHelper.GetChildrenCount(visual);
            for (var index = count - 1; index >= 0; index--)
            {
                stack.Push((VisualTreeHelper.GetChild(visual, index), depth + 1));
            }
        }
    }

    private static string DescribeVisual((DependencyObject Visual, int Depth) item)
    {
        var visual = item.Visual;
        var name = visual is FrameworkElement fe && !string.IsNullOrWhiteSpace(fe.Name)
            ? $"#{fe.Name}"
            : "";
        var size = visual is FrameworkElement sizeElement
            ? $" size={sizeElement.ActualWidth:F1}x{sizeElement.ActualHeight:F1}"
            : "";
        var grid = visual is UIElement uiElement
            ? $" grid=({System.Windows.Controls.Grid.GetRow(uiElement)},{System.Windows.Controls.Grid.GetColumn(uiElement)},rs={System.Windows.Controls.Grid.GetRowSpan(uiElement)},cs={System.Windows.Controls.Grid.GetColumnSpan(uiElement)})"
            : "";

        var border = visual is System.Windows.Controls.Border borderElement
            ? $" bg={BrushText(borderElement.Background)} border={BrushText(borderElement.BorderBrush)} thickness={borderElement.BorderThickness}"
            : "";

        var control = visual is System.Windows.Controls.Control controlElement
            ? $" bg={BrushText(controlElement.Background)} border={BrushText(controlElement.BorderBrush)} thickness={controlElement.BorderThickness}"
            : "";

        return $"d{item.Depth}:{visual.GetType().Name}{name}{size}{grid}{border}{control}";
    }

    private static string BrushText(Brush? brush)
        => brush switch
        {
            null => "null",
            SolidColorBrush solid => solid.Color.ToString(),
            _ => brush.ToString()
        };
}
