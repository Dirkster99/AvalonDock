#nullable disable
using System;
using System.IO;

namespace AvalonDock.Core
{
	/// <summary>
	/// Extension methods for <see cref="ILayoutSerializer"/> providing
	/// convenience overloads for <see cref="TextWriter"/> and <see cref="TextReader"/>.
	/// </summary>
	/// <remarks>
	/// These carry the one implementation of the text based overloads.
	/// <see cref="LayoutSerializerBase"/> exposes them as instance methods too, forwarding here, so
	/// that a caller holding a concrete serializer sees them at all - an instance method wins
	/// overload resolution over an extension method.
	/// </remarks>
	public static class LayoutSerializerExtensions
	{
		/// <summary>Serializes the current docking layout to a <see cref="TextWriter"/>.</summary>
		/// <param name="serializer">The layout serializer.</param>
		/// <param name="writer">The text writer to write to.</param>
		/// <exception cref="ArgumentNullException">Any of the arguments is <c>null</c>.</exception>
		public static void Serialize(this ILayoutSerializer serializer, TextWriter writer)
		{
			if (serializer is null) throw new ArgumentNullException(nameof(serializer));
			if (writer is null) throw new ArgumentNullException(nameof(writer));

			using var stream = new MemoryStream();
			serializer.Serialize(stream);
			writer.Write(System.Text.Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length));
		}

		/// <summary>Deserializes a docking layout from a <see cref="TextReader"/> and applies fixup.</summary>
		/// <param name="serializer">The layout serializer.</param>
		/// <param name="reader">The text reader to read from.</param>
		/// <exception cref="ArgumentNullException">Any of the arguments is <c>null</c>.</exception>
		public static void Deserialize(this ILayoutSerializer serializer, TextReader reader)
		{
			if (serializer is null) throw new ArgumentNullException(nameof(serializer));
			if (reader is null) throw new ArgumentNullException(nameof(reader));

			var bytes = System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd());
			using var stream = new MemoryStream(bytes);
			serializer.Deserialize(stream);
		}
	}
}