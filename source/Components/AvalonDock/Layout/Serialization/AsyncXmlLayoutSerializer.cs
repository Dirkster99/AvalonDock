/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.IO;
using System.Linq;
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


		#region Private Methods

		/// <summary>Performs all required actions for deserialization.</summary>
		/// <param name="function">
		/// A function, receiving the <see cref="LayoutRoot"/> <see cref="XmlSerializer"/>,
		/// that is supposed to call a deserialize method.
		/// </param>
		private Task DeserializeCommon(Func<XmlSerializer, LayoutRoot> function)
			=> DeserializeCommon(() =>
			{
				var serializer = XmlSerializer.FromTypes(new[] {typeof(LayoutRoot)}).First();
				return function(serializer);
			});

		#endregion

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
		public Task Deserialize(Stream stream)
			=> DeserializeCommon((xmlSerializer) => (LayoutRoot) xmlSerializer.Deserialize(stream));

		/// <summary>Deserialize the layout a file from a <see cref="TextReader"/>.</summary>
		/// <param name="reader"></param>
		public Task Deserialize(TextReader reader)
			=> DeserializeCommon((xmlSerializer) => (LayoutRoot) xmlSerializer.Deserialize(reader));

		/// <summary>Deserialize the layout a file from a <see cref="XmlReader"/>.</summary>
		/// <param name="reader"></param>
		public Task Deserialize(XmlReader reader)
			=> DeserializeCommon((xmlSerializer) => (LayoutRoot) xmlSerializer.Deserialize(reader));

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