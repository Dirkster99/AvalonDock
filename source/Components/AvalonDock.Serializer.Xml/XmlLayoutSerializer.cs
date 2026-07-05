using System.IO;
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
		private static readonly XmlSerializer DtoSerializer = new XmlSerializer(typeof(LayoutRootDto), new XmlAttributeOverrides());

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
	}
}