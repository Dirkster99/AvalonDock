/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AvalonDock.Layout.Serialization
{
	/// <summary>Implements a layout serialization/deserialization method of the docking framework.</summary>
	public class XmlLayoutSerializer : LayoutSerializer
	{
		#region Constructors

		/// <summary>
		/// Class constructor from <see cref="DockingManager"/> instance.
		/// </summary>
		/// <param name="manager"></param>
		public XmlLayoutSerializer(DockingManager manager)
			: base(manager)
		{
		}

		#endregion Constructors

		#region Private Methods
		/// <returns>Desererialized LayoutRoot</returns>
		/// <summary> Function for LayoutRoot deserialization. </summary>

		private delegate LayoutRoot DeserializeFunction();

		/// <summary> Deserializes layout with the given function <see cref="DeserializeFunction"/>.</summary>
		/// <param name="function"></param>
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
		#endregion

		#region Public Methods

		readonly XmlSerializer _serializer = XmlSerializer.FromTypes(new[] { typeof(LayoutRoot) })[0];

		/// <summary>Serialize the layout into a <see cref="XmlWriter"/>.</summary>
		/// <param name="writer"></param>
		public void Serialize(XmlWriter writer)
		{
			_serializer.Serialize(writer, Manager.Layout);
		}

		/// <summary>Serialize the layout into a <see cref="TextWriter"/>.</summary>
		/// <param name="writer"></param>
		public void Serialize(TextWriter writer)
		{
			_serializer.Serialize(writer, Manager.Layout);
		}

		/// <summary>Serialize the layout into a <see cref="Stream"/>.</summary>
		/// <param name="stream"></param>
		public void Serialize(Stream stream)
		{
			_serializer.Serialize(stream, Manager.Layout);
		}

		/// <summary>Serialize the layout into a file using a <see cref="StreamWriter"/>.</summary>
		/// <param name="filepath"></param>
		public void Serialize(string filepath)
		{
			using (var stream = new StreamWriter(filepath))
				Serialize(stream);
		}

		/// <summary>Deserialize the layout a file from a <see cref="Stream"/>.</summary>
		/// <param name="stream"></param>
		public void Deserialize(System.IO.Stream stream)
		{
			LayoutRoot function() => _serializer.Deserialize(stream) as LayoutRoot;
			DeserializeCommon(function);
		}

		/// <summary>Deserialize the layout a file from a <see cref="TextReader"/>.</summary>
		/// <param name="reader"></param>
		public void Deserialize(TextReader reader)
		{

			LayoutRoot function() => _serializer.Deserialize(reader) as LayoutRoot;
			DeserializeCommon(function);
		}

		/// <summary>Deserialize the layout a file from a <see cref="XmlReader"/>.</summary>
		/// <param name="reader"></param>
		public void Deserialize(XmlReader reader)
		{

			LayoutRoot function() => _serializer.Deserialize(reader) as LayoutRoot;
			DeserializeCommon(function);
		}

		/// <summary>Deserialize the layout from a file using a <see cref="StreamReader"/>.</summary>
		/// <param name="filepath"></param>
		public void Deserialize(string filepath)
		{
			using (var stream = new StreamReader(filepath))
				Deserialize(stream);
		}

		#endregion Public Methods
	}
}