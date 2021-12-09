using SerializationTestApp.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SerializationTestApp
{
	public class MainWindowViewModel
	{
		public ObservableCollection<DockableWindow> Windows { get; } = new();
		public ObservableCollection<DockableDocument> Documents { get; } = new();

		public ICommand ViewExplorerCommand { get; }
		public ICommand ViewPropertiesCommand { get; }
		public ICommand CreateTextDocumentCommand { get; }

		public MainWindowViewModel()
		{
			ViewExplorerCommand = new Command(ViewExplorer);
			ViewPropertiesCommand = new Command(ViewProperties);
			CreateTextDocumentCommand = new Command(CreateTextDocument);
		}

		private void ViewExplorer()
		{
			if (Windows.OfType<ExplorerViewModel>().Any() == false)
			{
				Windows.Add(new ExplorerViewModel());
			}
		}

		private void ViewProperties()
		{
			if (Windows.OfType<PropertiesViewModel>().Any() == false)
			{
				Windows.Add(new PropertiesViewModel());
			}
		}

		private void CreateTextDocument()
		{
			Documents.Add(new TextDocumentViewModel(this));
		}
	}
}
