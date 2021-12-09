using AvalonDock.Layout.Serialization;
using System.Windows;

namespace SerializationTestApp
{
	public partial class MainWindow : Window
    {
		private readonly string _filePath = @"C:\temp\layout.xml";
		private readonly XmlLayoutSerializer _serializer;
		private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
			_viewModel = new MainWindowViewModel();
			DataContext = _viewModel;
			_serializer = new XmlLayoutSerializer(DockingManager);
			_serializer.LayoutSerializationCallback += Serializer_LayoutSerializationCallback;
        }

		private void SerializeLayout(object sender, RoutedEventArgs e)
		{
			_serializer.Serialize(_filePath);
		}

		private void DeserializeLayout(object sender, RoutedEventArgs e)
		{
			_serializer.Deserialize(_filePath);
		}

		private void Serializer_LayoutSerializationCallback(object? sender, LayoutSerializationCallbackEventArgs e)
		{
			if (e.Model.ContentId == "W1")
			{
				var explorer = new ExplorerViewModel();
				e.Content = explorer;
				_viewModel.Windows.Add(explorer);
			}
			else if (e.Model.ContentId == "W2")
			{
				var properties = new PropertiesViewModel();
				e.Content = properties;
				_viewModel.Windows.Add(properties);
			}
			else if (e.Model.ContentId == "D1")
			{
				var textDocument = new TextDocumentViewModel(_viewModel);
				e.Content = textDocument;
				_viewModel.Documents.Add(textDocument);
			}
		}
	}
}
