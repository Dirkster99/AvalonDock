using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

public partial class TerminalView : UserControl
{
    private int _inputStartIndex;

    public TerminalView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        TerminalBox.PreviewKeyDown += OnTerminalKeyDown;
        TerminalBox.PreviewTextInput += OnPreviewTextInput;
        DataObject.AddPastingHandler(TerminalBox, OnPaste);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is TerminalViewModel oldVm)
        {
            oldVm.OutputReceived -= OnOutputReceived;
            oldVm.PromptReady -= OnPromptReady;
        }

        if (e.NewValue is TerminalViewModel newVm)
        {
            newVm.OutputReceived += OnOutputReceived;
            newVm.PromptReady += OnPromptReady;
            TerminalBox.Text = "PowerShell\n";
            ShowPrompt(newVm.CurrentDirectory);
        }
    }

    private void OnOutputReceived(object? sender, string text)
    {
        TerminalBox.Text += text;
        OutputScroller.ScrollToEnd();
    }

    private void OnPromptReady(object? sender, string directory)
    {
        ShowPrompt(directory);
    }

    private void ShowPrompt(string directory)
    {
        var prompt = $"PS {directory}> ";
        TerminalBox.Text += prompt;
        _inputStartIndex = TerminalBox.Text.Length;
        TerminalBox.CaretIndex = _inputStartIndex;
        TerminalBox.Focus();
        OutputScroller.ScrollToEnd();
    }

    private void OnTerminalKeyDown(object sender, KeyEventArgs e)
    {
        // Prevent editing before the input area
        if (TerminalBox.CaretIndex < _inputStartIndex &&
            e.Key != Key.Left && e.Key != Key.Right &&
            e.Key != Key.Up && e.Key != Key.Down &&
            e.Key != Key.Home && e.Key != Key.End)
        {
            TerminalBox.CaretIndex = TerminalBox.Text.Length;
        }

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            var command = TerminalBox.Text.Substring(_inputStartIndex);
            TerminalBox.Text += "\n";
            _inputStartIndex = TerminalBox.Text.Length;

            if (DataContext is TerminalViewModel vm)
            {
                vm.ExecuteCommand(command);
            }
        }
        else if (e.Key == Key.Back)
        {
            if (TerminalBox.CaretIndex <= _inputStartIndex)
                e.Handled = true;
        }
        else if (e.Key == Key.Home)
        {
            e.Handled = true;
            TerminalBox.CaretIndex = _inputStartIndex;
        }
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (TerminalBox.CaretIndex < _inputStartIndex)
        {
            TerminalBox.CaretIndex = TerminalBox.Text.Length;
        }
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (TerminalBox.CaretIndex < _inputStartIndex)
        {
            TerminalBox.CaretIndex = TerminalBox.Text.Length;
        }
    }
}
