using System;
using System.Windows;
using System.Windows.Controls;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

public partial class TerminalView : UserControl
{
    public TerminalView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Terminal.LineEntered += OnLineEntered;
        if (DataContext is TerminalViewModel vm)
        {
            vm.StartProcess();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Terminal.LineEntered -= OnLineEntered;
        if (DataContext is TerminalViewModel vm)
        {
            vm.StopProcess();
        }
    }

    private void OnLineEntered(object? sender, EventArgs e)
    {
        if (DataContext is TerminalViewModel vm && Terminal.Line != null)
        {
            vm.ExecuteLine(Terminal.Line);
        }
    }
}
