using SerializationTestApp.Base;

namespace SerializationTestApp;

public class TextDocumentViewModel : DockableDocument
{
	public override string Title => $"Document{i}";
	public override string ContentId => "D1";

	private static int i = 0;

	public TextDocumentViewModel(MainWindowViewModel mainWindow) : base(mainWindow)
	{
		i++;
	}
}
