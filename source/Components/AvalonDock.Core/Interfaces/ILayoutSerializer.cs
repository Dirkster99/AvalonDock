#nullable disable
using System;
using System.IO;

namespace AvalonDock.Core
{
	/// <summary>
	/// Abstraction for docking layout serialization, enabling pluggable
	/// serialization formats (XML, JSON, etc.) via DI.
	/// </summary>
	public interface ILayoutSerializer
	{
		/// <summary>
		/// Raised during deserialization to let the client attach content to layout items.
		/// </summary>
		event EventHandler<LayoutSerializationCallbackEventArgs> LayoutSerializationCallback;

		/// <summary>Serializes the current docking layout to a stream.</summary>
		/// <param name="stream">The stream to write to.</param>
		void Serialize(Stream stream);

		/// <summary>Deserializes a docking layout from a stream and applies fixup.</summary>
		/// <param name="stream">The stream to read from.</param>
		void Deserialize(Stream stream);

		/// <summary>Serializes the current docking layout to a file.</summary>
		/// <param name="filepath">The file path to write to.</param>
		void Serialize(string filepath);

		/// <summary>Deserializes a docking layout from a file and applies fixup.</summary>
		/// <param name="filepath">The file path to read from.</param>
		void Deserialize(string filepath);
	}
}