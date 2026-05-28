using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

public partial class EditorView : UserControl
{
	public EditorView()
	{
		InitializeComponent();
		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is EditorTabViewModel vm)
		{
			Editor.Text = vm.Content;

			try
			{
				var highlighting = HighlightingManager.Instance.GetDefinition(vm.SyntaxHighlighting);
				Editor.SyntaxHighlighting = highlighting;
			}
			catch
			{
				Editor.SyntaxHighlighting = null;
			}
		}
	}
}