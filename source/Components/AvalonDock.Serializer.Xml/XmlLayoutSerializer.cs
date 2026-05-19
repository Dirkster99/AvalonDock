using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AvalonDock.Core;
using AvalonDock.Core.Serialization;
using AvalonDock.Layout;

namespace AvalonDock.Serializer.Xml
{
	/// <summary>
	/// Layout-aware XML serializer that handles deserialization fixup (reconnecting
	/// content, previous containers, and invoking the serialization callback).
	/// Extends <see cref="LayoutSerializerBase"/> from AvalonDock.Core.
	/// </summary>
	public class XmlLayoutSerializer : LayoutSerializerBase
	{
		private readonly XmlSerializer _serializer = XmlSerializersCache.GetSerializer<LayoutRoot>();

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class.
		/// </summary>
		/// <param name="manager">The docking manager (must implement <see cref="ISerializableDockingManager"/>).</param>
		public XmlLayoutSerializer(ISerializableDockingManager manager)
			: base(manager)
		{
		}

		private delegate LayoutRoot DeserializeFunction();

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

		/// <summary>Serialize the layout into a <see cref="XmlWriter"/>.</summary>
		public void Serialize(XmlWriter writer)
		{
			_serializer.Serialize(writer, (LayoutRoot)Manager.Layout);
		}

		/// <summary>Serialize the layout into a <see cref="TextWriter"/>.</summary>
		public void Serialize(TextWriter writer)
		{
			_serializer.Serialize(writer, (LayoutRoot)Manager.Layout);
		}

		/// <summary>Serialize the layout into a <see cref="Stream"/>.</summary>
		public void Serialize(Stream stream)
		{
			_serializer.Serialize(stream, (LayoutRoot)Manager.Layout);
		}

		/// <summary>Serialize the layout into a file.</summary>
		public void Serialize(string filepath)
		{
			using (var stream = new StreamWriter(filepath))
				Serialize(stream);
		}

		/// <summary>Deserialize the layout from a <see cref="Stream"/>.</summary>
		public void Deserialize(Stream stream)
		{
			DeserializeCommon(() => _serializer.Deserialize(stream) as LayoutRoot);
		}

		/// <summary>Deserialize the layout from a <see cref="TextReader"/>.</summary>
		public void Deserialize(TextReader reader)
		{
			DeserializeCommon(() => _serializer.Deserialize(reader) as LayoutRoot);
		}

		/// <summary>Deserialize the layout from a <see cref="XmlReader"/>.</summary>
		public void Deserialize(XmlReader reader)
		{
			DeserializeCommon(() => _serializer.Deserialize(reader) as LayoutRoot);
		}

		/// <summary>Deserialize the layout from a file.</summary>
		public void Deserialize(string filepath)
		{
			using (var stream = new StreamReader(filepath))
				Deserialize(stream);
		}
	}
}