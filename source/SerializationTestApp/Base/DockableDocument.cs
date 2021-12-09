using System;
using System.Windows.Input;

namespace SerializationTestApp.Base;

public abstract class DockableDocument : DockableObject
{
	public ICommand CloseCommand { get; }

	private readonly MainWindowViewModel _mainWindow;

	public DockableDocument(MainWindowViewModel mainWindow)
	{
		CloseCommand = new Command(Close);
		_mainWindow = mainWindow;
	}

	private void Close()
	{
		_mainWindow.Documents.Remove(this);
	}
}
