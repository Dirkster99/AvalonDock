/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Xml.Serialization;
using System.IO;
using System.Xml;

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
				Serialize(stream);
		}


		/// <summary>Deserialize the layout a file from a <see cref="Stream"/>.</summary>
		/// <param name="stream"></param>
		public void Deserialize(System.IO.Stream stream)
		{
			try
			{
				StartDeserialization();
				var serializer = new XmlSerializer(typeof(LayoutRoot));
				var layout = serializer.Deserialize(stream) as LayoutRoot;
				FixupLayout(layout);
				Manager.Layout = layout;
			}
			finally
			{
				EndDeserialization();
			}
		}

		/// <summary>Deserialize the layout a file from a <see cref="TextReader"/>.</summary>
		/// <param name="reader"></param>
		public void Deserialize(TextReader reader)
		{
			try
			{
				StartDeserialization();
				var serializer = new XmlSerializer(typeof(LayoutRoot));
				var layout = serializer.Deserialize(reader) as LayoutRoot;
				FixupLayout(layout);
				Manager.Layout = layout;
			}
			finally
			{
				EndDeserialization();
			}
		}

		/// <summary>Deserialize the layout a file from a <see cref="XmlReader"/>.</summary>
		/// <param name="reader"></param>
		public void Deserialize(XmlReader reader)
		{
			try
			{
				StartDeserialization();
				var serializer = new XmlSerializer(typeof(LayoutRoot));
				var layout = serializer.Deserialize(reader) as LayoutRoot;
				FixupLayout(layout);
				Manager.Layout = layout;
			}
			finally
			{
				EndDeserialization();
			}
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
