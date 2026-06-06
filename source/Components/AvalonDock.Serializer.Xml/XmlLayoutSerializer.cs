using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AvalonDock.Core;
using AvalonDock.Core.Serialization.Dto;

namespace AvalonDock.Serializer.Xml
{
	/// <summary>
	/// XML implementation of <see cref="ILayoutSerializer"/>.
	/// Extends <see cref="LayoutSerializerBase"/> for layout-aware deserialization
	/// with fixup (reconnecting content, previous containers, callbacks).
	/// </summary>
	public class XmlLayoutSerializer : LayoutSerializerBase
	{
		private static readonly XmlSerializer DtoSerializer = new XmlSerializer(typeof(LayoutRootDto));

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		public XmlLayoutSerializer(IDockingManager manager)
			: base(manager)
		{
		}

		/// <inheritdoc/>
		protected override void SerializeCore(Stream stream, LayoutRootDto dto)
		{
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);
			DtoSerializer.Serialize(stream, dto, ns);
		}

		/// <inheritdoc/>
		protected override LayoutRootDto DeserializeCore(Stream stream)
		{
			return (LayoutRootDto)DtoSerializer.Deserialize(stream);
		}

		/// <summary>Serialize the layout into a <see cref="XmlWriter"/>.</summary>
		/// <param name="writer">The XML writer to write to.</param>
		public void Serialize(XmlWriter writer)
		{
			var dto = Manager.DtoMapper.ToDto(Manager.Layout);
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);
			DtoSerializer.Serialize(writer, dto, ns);
		}

		/// <summary>Serialize the layout into a <see cref="TextWriter"/>.</summary>
		/// <param name="writer">The text writer to write to.</param>
		public void Serialize(TextWriter writer)
		{
			var dto = Manager.DtoMapper.ToDto(Manager.Layout);
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);
			DtoSerializer.Serialize(writer, dto, ns);
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