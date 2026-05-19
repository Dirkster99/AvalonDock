using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using AvalonDock.Core;
using AvalonDock.Core.Serialization;
using AvalonDock.Layout;
using Newtonsoft.Json;

namespace AvalonDock.Serializer.Json
{
	/// <summary>
	/// JSON implementation of <see cref="ILayoutSerializer"/>.
	/// Extends <see cref="LayoutSerializerBase"/> for layout-aware deserialization
	/// with fixup. Uses the proven XML serialization internally and converts to/from JSON
	/// via Newtonsoft.Json, ensuring correct handling of the polymorphic layout tree.
	/// </summary>
	public class JsonLayoutSerializer : LayoutSerializerBase
	{
		private readonly XmlSerializer _xmlSerializer = XmlSerializersCache.GetSerializer<LayoutRoot>();
		private readonly Newtonsoft.Json.Formatting _formatting;

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager whose layout is serialized.</param>
		public JsonLayoutSerializer(IDockingManager manager)
			: base(manager)
		{
			_formatting = Newtonsoft.Json.Formatting.Indented;
		}

		/// <inheritdoc/>
		protected override void SerializeCore(Stream stream, ISerializableLayoutRoot layout)
		{
			XDocument xdoc;
			using (var xmlStream = new MemoryStream())
			{
				_xmlSerializer.Serialize(xmlStream, (LayoutRoot)layout);
				xmlStream.Position = 0;
				xdoc = XDocument.Load(xmlStream);
			}

			var json = JsonConvert.SerializeXNode(xdoc, _formatting);
			var bytes = Encoding.UTF8.GetBytes(json);
			stream.Write(bytes, 0, bytes.Length);
		}

		/// <inheritdoc/>
		protected override ISerializableLayoutRoot DeserializeCore(Stream stream)
		{
			string json;
			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
			{
				json = reader.ReadToEnd();
			}

			var xdoc = JsonConvert.DeserializeXNode(json);
			using (var xmlStream = new MemoryStream())
			{
				xdoc.Save(xmlStream);
				xmlStream.Position = 0;
				return (ISerializableLayoutRoot)_xmlSerializer.Deserialize(xmlStream);
			}
		}
	}
}