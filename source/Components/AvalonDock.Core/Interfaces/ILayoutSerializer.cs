using System.IO;

namespace AvalonDock.Core
{
	/// <summary>
	/// Abstraction for layout serialization, enabling pluggable serialization backends.
	/// </summary>
	public interface ILayoutSerializer
	{
		/// <summary>Serializes a value to a string.</summary>
		/// <typeparam name="T">The type of value to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The serialized string representation.</returns>
		string Serialize<T>(T value);

		/// <summary>Deserializes a value from a string.</summary>
		/// <typeparam name="T">The type to deserialize to.</typeparam>
		/// <param name="text">The serialized string.</param>
		/// <returns>The deserialized value.</returns>
		T? Deserialize<T>(string text);

		/// <summary>Loads a value from a stream.</summary>
		/// <typeparam name="T">The type to deserialize to.</typeparam>
		/// <param name="stream">The stream to read from.</param>
		/// <returns>The deserialized value.</returns>
		T? Load<T>(Stream stream);

		/// <summary>Saves a value to a stream.</summary>
		/// <typeparam name="T">The type of value to serialize.</typeparam>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="value">The value to serialize.</param>
		void Save<T>(Stream stream, T value);
	}
}