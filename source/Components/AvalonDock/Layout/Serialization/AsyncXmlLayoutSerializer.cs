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
	/// <summary>
	/// Implements a layout serialization/deserialization method of the docking framework.
	/// </summary>
	/// <example>
	/// <code>
	/// private void Serialize()
	/// {
	///     // The serialization is not done async as the XmlSerializer used is not having async overloads
	///     var serializer = new AvalonDock.Layout.Serialization.AsyncXmlLayoutSerializer(dockManager);
	///     serializer.Serialize(@".\AvalonDock.config");
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// private async Task DeserializeAsync(AvalonDock.DockingManager dockManager)
	/// {
	///     using (var serializer = new AvalonDock.Layout.Serialization.AsyncXmlLayoutSerializer(dockManager))
	///	    {
	///	        serializer.LayoutRestore += async (s, args) =>
	///         {
	///             // Emulate an async operation for this
	///             await Task.Delay(1000);
	///             // Required for each interaction with actual AvalonDock, as these are STA components
	///             await Dispatcher.InvokeAsync(() => args.Content = args.Content);
	///         };
	///         if (File.Exists(@".\AvalonDock.config"))
	///             await serializer.DeserializeAsync(@".\AvalonDock.config");
	///     }
	/// }
	/// </code>
	/// </example>
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
		private Task DeserializeCommonAsync(Func<XmlSerializer, LayoutRoot> function)
			=> base.DeserializeCommonAsync(() =>
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
		public Task DeserializeAsync(Stream stream)
			=> DeserializeCommonAsync((xmlSerializer) => (LayoutRoot) xmlSerializer.Deserialize(stream));

		/// <summary>Deserialize the layout a file from a <see cref="TextReader"/>.</summary>
		/// <param name="reader"></param>
		public Task DeserializeAsync(TextReader reader)
			=> DeserializeCommonAsync((xmlSerializer) => (LayoutRoot) xmlSerializer.Deserialize(reader));

		/// <summary>Deserialize the layout a file from a <see cref="XmlReader"/>.</summary>
		/// <param name="reader"></param>
		public Task DeserializeAsync(XmlReader reader)
			=> DeserializeCommonAsync((xmlSerializer) => (LayoutRoot) xmlSerializer.Deserialize(reader));

		/// <summary>Deserialize the layout from a file using a <see cref="StreamReader"/>.</summary>
		/// <param name="filepath"></param>
		public async Task DeserializeAsync(string filepath)
		{
			using (var stream = new StreamReader(filepath))
			{
				await DeserializeAsync(stream);
			}
		}

		#endregion Public Methods
	}
}