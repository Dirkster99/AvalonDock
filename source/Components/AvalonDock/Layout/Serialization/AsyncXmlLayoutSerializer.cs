/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AvalonDock.Layout.Serialization
{
	/// <summary>Implements a layout serialization/deserialization method of the docking framework.</summary>
	public class AsyncXmlLayoutSerializer : LayoutSerializerBase
	{
		#region Constructors

		/// <summary>
		/// Class constructor from <see cref="DockingManager"/> instance.
		/// </summary>
		/// <param name="manager"></param>
		public AsyncXmlLayoutSerializer(DockingManager manager)
			: base(manager)
		{
		}

		#endregion Constructors

		#region Public Methods

		/// <summary>Serialize the layout into a <see cref="XmlWriter"/>.</summary>
		/// <param name="writer"></param>
		public void Serialize(XmlWriter writer)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			serializer.Serialize(writer, Manager.Layout);
		}

		/// <summary>Serialize the layout into a <see cref="TextWriter"/>.</summary>
		/// <param name="writer"></param>
		public void Serialize(TextWriter writer)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			serializer.Serialize(writer, Manager.Layout);
		}

		/// <summary>Serialize the layout into a <see cref="Stream"/>.</summary>
		/// <param name="stream"></param>
		public void Serialize(Stream stream)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			serializer.Serialize(stream, Manager.Layout);
		}

		/// <summary>Serialize the layout into a file using a <see cref="StreamWriter"/>.</summary>
		/// <param name="filepath"></param>
		public void Serialize(string filepath)
		{
			using (var stream = new StreamWriter(filepath))
			{
				Serialize(stream);
			}
		}

		/// <summary>Deserialize the layout a file from a <see cref="Stream"/>.</summary>
		/// <param name="stream"></param>
		public async Task Deserialize(System.IO.Stream stream)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			var layout = (LayoutRoot)serializer.Deserialize(stream);
			await FixupLayout(layout);
			Manager.Layout = layout;
		}

		/// <summary>Deserialize the layout a file from a <see cref="TextReader"/>.</summary>
		/// <param name="reader"></param>
		public async Task Deserialize(TextReader reader)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			var layout = (LayoutRoot)serializer.Deserialize(reader);
			await FixupLayout(layout);
			Manager.Layout = layout;
		}

		/// <summary>Deserialize the layout a file from a <see cref="XmlReader"/>.</summary>
		/// <param name="reader"></param>
		public async Task Deserialize(XmlReader reader)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			var layout = (LayoutRoot)serializer.Deserialize(reader);
			await FixupLayout(layout);
			Manager.Layout = layout;
		}

		/// <summary>Deserialize the layout from a file using a <see cref="StreamReader"/>.</summary>
		/// <param name="filepath"></param>
		public async Task Deserialize(string filepath)
		{
			using (var stream = new StreamReader(filepath))
			{
				await Deserialize(stream);
			}
		}

		#endregion Public Methods
	}
}