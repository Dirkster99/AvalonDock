#nullable disable
using System.IO;

namespace AvalonDock.Core
{
	/// <summary>
	/// Extension methods for <see cref="ILayoutSerializer"/> providing
	/// convenience overloads for <see cref="TextWriter"/> and <see cref="TextReader"/>.
	/// </summary>
	public static class LayoutSerializerExtensions
	{
		/// <summary>Serializes the current docking layout to a <see cref="TextWriter"/>.</summary>
		/// <param name="serializer">The layout serializer.</param>
		/// <param name="writer">The text writer to write to.</param>
		public static void Serialize(this ILayoutSerializer serializer, TextWriter writer)
		{
			using var stream = new MemoryStream();
			serializer.Serialize(stream);
			writer.Write(System.Text.Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length));
		}

		/// <summary>Deserializes a docking layout from a <see cref="TextReader"/> and applies fixup.</summary>
		/// <param name="serializer">The layout serializer.</param>
		/// <param name="reader">The text reader to read from.</param>
		public static void Deserialize(this ILayoutSerializer serializer, TextReader reader)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd());
			using var stream = new MemoryStream(bytes);
			serializer.Deserialize(stream);
		}
	}
}