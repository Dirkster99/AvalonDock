using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AvalonDock.Core;

namespace AvalonDock.Serializer.Xml
{
	/// <summary>
	/// Generic XML implementation of <see cref="ILayoutSerializer"/>.
	/// Provides DI-injectable XML serialization independent of the legacy layout serialization API.
	/// </summary>
	public sealed class XmlDockLayoutSerializer : ILayoutSerializer
	{
		private readonly XmlWriterSettings _writerSettings;

		public XmlDockLayoutSerializer()
		{
			_writerSettings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = false
			};
		}

		public XmlDockLayoutSerializer(XmlWriterSettings writerSettings)
		{
			_writerSettings = writerSettings;
		}

		public string Serialize<T>(T value)
		{
			var serializer = new XmlSerializer(typeof(T));
			using (var stringWriter = new StringWriter())
			using (var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings))
			{
				serializer.Serialize(xmlWriter, value);
				return stringWriter.ToString();
			}
		}

		public T Deserialize<T>(string text)
		{
			var serializer = new XmlSerializer(typeof(T));
			using (var reader = new StringReader(text))
			{
				return (T)serializer.Deserialize(reader);
			}
		}

		public T Load<T>(Stream stream)
		{
			var serializer = new XmlSerializer(typeof(T));
			return (T)serializer.Deserialize(stream);
		}

		public void Save<T>(Stream stream, T value)
		{
			var serializer = new XmlSerializer(typeof(T));
			using (var xmlWriter = XmlWriter.Create(stream, _writerSettings))
			{
				serializer.Serialize(xmlWriter, value);
			}
		}
	}
}