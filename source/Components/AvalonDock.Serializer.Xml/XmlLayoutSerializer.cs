using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AvalonDock.Core;
using AvalonDock.Core.Serialization;
using AvalonDock.Layout;

namespace AvalonDock.Serializer.Xml
{
	/// <summary>
	/// XML serializer that implements <see cref="ILayoutSerializer"/> for DI-injectable
	/// generic serialization, and extends <see cref="LayoutSerializerBase"/> for
	/// layout-aware deserialization with fixup (reconnecting content, previous containers).
	/// </summary>
	public class XmlLayoutSerializer : LayoutSerializerBase
	{
		private readonly XmlSerializer _layoutSerializer = XmlSerializersCache.GetSerializer<LayoutRoot>();
		private readonly XmlWriterSettings _writerSettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class
		/// for generic serialization (DI use). Layout-aware methods are not available.
		/// </summary>
		public XmlLayoutSerializer()
		{
			_writerSettings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = false,
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class
		/// with custom writer settings for generic serialization.
		/// </summary>
		/// <param name="writerSettings">The XML writer settings to use.</param>
		public XmlLayoutSerializer(XmlWriterSettings writerSettings)
		{
			_writerSettings = writerSettings;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class
		/// with a docking manager for layout-aware serialization with fixup.
		/// </summary>
		/// <param name="manager">The docking manager (must implement <see cref="ISerializableDockingManager"/>).</param>
		public XmlLayoutSerializer(ISerializableDockingManager manager)
			: base(manager)
		{
			_writerSettings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = false,
			};
		}

		private delegate LayoutRoot DeserializeFunction();

		/// <inheritdoc/>
		public override string Serialize<T>(T value)
		{
			var serializer = new XmlSerializer(typeof(T));
			using (var stringWriter = new StringWriter())
			using (var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings))
			{
				serializer.Serialize(xmlWriter, value);
				return stringWriter.ToString();
			}
		}

		/// <inheritdoc/>
		public override T Deserialize<T>(string text)
		{
			var serializer = new XmlSerializer(typeof(T));
			using (var reader = new StringReader(text))
			{
				return (T)serializer.Deserialize(reader);
			}
		}

		/// <inheritdoc/>
		public override T Load<T>(Stream stream)
		{
			var serializer = new XmlSerializer(typeof(T));
			return (T)serializer.Deserialize(stream);
		}

		/// <inheritdoc/>
		public override void Save<T>(Stream stream, T value)
		{
			var serializer = new XmlSerializer(typeof(T));
			using (var xmlWriter = XmlWriter.Create(stream, _writerSettings))
			{
				serializer.Serialize(xmlWriter, value);
			}
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

		/// <summary>Serialize the layout into a <see cref="Stream"/>.</summary>
		/// <param name="stream">The stream to write to.</param>
		public void Serialize(Stream stream)
		{
			_layoutSerializer.Serialize(stream, (LayoutRoot)Manager.Layout);
		}

		/// <summary>Serialize the layout into a file.</summary>
		/// <param name="filepath">The file path to write to.</param>
		public void Serialize(string filepath)
		{
			using (var stream = new StreamWriter(filepath))
				Serialize(stream);
		}

		/// <summary>Deserialize the layout from a <see cref="Stream"/>.</summary>
		/// <param name="stream">The stream to read from.</param>
		public void Deserialize(Stream stream)
		{
			DeserializeCommon(() => _layoutSerializer.Deserialize(stream) as LayoutRoot);
		}

		/// <summary>Deserialize the layout from a <see cref="TextReader"/>.</summary>
		/// <param name="reader">The text reader to read from.</param>
		public void Deserialize(TextReader reader)
		{
			DeserializeCommon(() => _layoutSerializer.Deserialize(reader) as LayoutRoot);
		}

		/// <summary>Deserialize the layout from a <see cref="XmlReader"/>.</summary>
		/// <param name="reader">The XML reader to read from.</param>
		public void Deserialize(XmlReader reader)
		{
			DeserializeCommon(() => _layoutSerializer.Deserialize(reader) as LayoutRoot);
		}

		/// <summary>Deserialize the layout from a file.</summary>
		/// <param name="filepath">The file path to read from.</param>
		public void Deserialize(string filepath)
		{
			using (var stream = new StreamReader(filepath))
				Deserialize(stream);
		}

		private void DeserializeCommon(DeserializeFunction function)
		{
			try
			{
				StartDeserialization();
				var layout = function();
				FixupLayout(layout);
				Manager.Layout = layout;
			}
			finally
			{
				EndDeserialization();
			}
		}
	}
}