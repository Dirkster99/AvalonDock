namespace SerializationTestApp.Base;

public abstract class DockableObject : ObservableObject
{
	public abstract string Title { get; }
	public abstract string ContentId { get; }
}
