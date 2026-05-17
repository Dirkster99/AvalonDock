using System.IO;

namespace AvalonDock.Core
{
	/// <summary>
	/// Abstraction for layout serialization, enabling pluggable serialization backends.
	/// </summary>
	public interface IDockSerializer
	{
		string Serialize<T>(T value);

		T? Deserialize<T>(string text);

		T? Load<T>(Stream stream);

		void Save<T>(Stream stream, T value);
	}
}