using SerializationTestApp.Base;
using System;

namespace SerializationTestApp;

public class TextDocumentViewModel : DockableDocument
{
	public override string Title => $"Document{_i}";
	public override string ContentId => "D1";

	private readonly int _i = 0;

	public TextDocumentViewModel(MainWindowViewModel mainWindow) : base(mainWindow)
	{
		_i = (int)(Random.Shared.NextDouble() * 100);
	}
}
