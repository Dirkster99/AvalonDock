using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AvalonDock.Core;
using AvalonDock.Core.Serialization;
using AvalonDock.Layout;

namespace AvalonDock.Serializer.Xml
{
	/// <summary>
	/// XML implementation of <see cref="ILayoutSerializer"/>.
	/// Extends <see cref="LayoutSerializerBase"/> for layout-aware deserialization
	/// with fixup (reconnecting content, previous containers, callbacks).
	/// </summary>
	public class XmlLayoutSerializer : LayoutSerializerBase
	{
		private readonly XmlSerializer _layoutSerializer = XmlSerializersCache.GetSerializer<LayoutRoot>();

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		public XmlLayoutSerializer(IDockingManager manager)
			: base(manager)
		{
		}

		/// <inheritdoc/>
		protected override void SerializeCore(Stream stream, ISerializableLayoutRoot layout)
		{
			_layoutSerializer.Serialize(stream, (LayoutRoot)layout);
		}

		/// <inheritdoc/>
		protected override ISerializableLayoutRoot DeserializeCore(Stream stream)
		{
			return (ISerializableLayoutRoot)_layoutSerializer.Deserialize(stream);
		}

		/// <summary>Serialize the layout into a <see cref="XmlWriter"/>.</summary>
		/// <param name="writer">The XML writer to write to.</param>
		public void Serialize(XmlWriter writer)
		{
			_layoutSerializer.Serialize(writer, (LayoutRoot)Manager.Layout);
		}

		/// <summary>Serialize the layout into a <see cref="TextWriter"/>.</summary>
		/// <param name="writer">The text writer to write to.</param>
		public void Serialize(TextWriter writer)
		{
			_layoutSerializer.Serialize(writer, (LayoutRoot)Manager.Layout);
		}

		/// <summary>Deserialize the layout from a <see cref="TextReader"/>.</summary>
		/// <param name="reader">The text reader to read from.</param>
		public void Deserialize(TextReader reader)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd());
			using (var ms = new MemoryStream(bytes))
				Deserialize(ms);
		}

		/// <summary>Deserialize the layout from a <see cref="XmlReader"/>.</summary>
		/// <param name="reader">The XML reader to read from.</param>
		public void Deserialize(XmlReader reader)
		{
			using (var ms = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(ms))
				{
					writer.WriteNode(reader, true);
					writer.Flush();
				}

				ms.Position = 0;
				Deserialize(ms);
			}
		}
	}
}