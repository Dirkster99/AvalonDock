using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

public partial class TerminalView : UserControl
{
	public TerminalView()
	{
		InitializeComponent();
		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is TerminalViewModel oldVm)
			oldVm.PropertyChanged -= OnTerminalPropertyChanged;

		if (e.NewValue is TerminalViewModel newVm)
			newVm.PropertyChanged += OnTerminalPropertyChanged;
	}

	private void OnTerminalPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(TerminalViewModel.Output))
		{
			OutputScroller.ScrollToEnd();
		}
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Enter && DataContext is TerminalViewModel vm)
		{
			vm.SendCommandCommand.Execute(null);
			e.Handled = true;
		}

		base.OnPreviewKeyDown(e);
	}
}