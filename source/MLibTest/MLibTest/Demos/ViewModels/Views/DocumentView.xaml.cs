using MLibTest.Demos.ViewModels.AD;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MLibTest.Demos.ViewModels.Views
{
	/// <summary>
	/// Interaction logic for TextDocumentView.xaml
	/// </summary>
	public partial class DocumentView : UserControl
	{
		private bool _viewInitialized;

		/// <summary>
		/// Class constructor
		/// </summary>
		public DocumentView()
		{
			InitializeComponent();

			WeakEventManager<FrameworkElement, RoutedEventArgs>
				.AddHandler(this, "Loaded", View_LoadedAsync);
		}

		/// <summary>
		/// Initializes the viewmodel and view as soon as the view is loaded.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void View_LoadedAsync(object sender, RoutedEventArgs e)
		{
			if (this.Visibility == Visibility.Visible && _viewInitialized == false)
				await LoadContentAsync();
		}

		private async Task LoadContentAsync()
		{
			_viewInitialized = true;                    // We'll try this only once

			var vm = DataContext as IDocumentViewModel;

			if (vm == null)
				return;

			try
			{
				bool result = await vm.OpenFileWithInitialPathAsync();

				// Poor mans error handling:
				// Attempt to close this document if it appears to be invalid
				if (result == false)
					vm.CloseCommand.Execute(null);
			}
			catch
			{
			}
		}
	}
}
