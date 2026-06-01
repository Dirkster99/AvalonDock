using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout floating window.
	/// </summary>
	[Serializable]
	public abstract class LayoutFloatingWindow : LayoutElement, ILayoutContainer, IXmlSerializable
	{
		/// <summary>
		/// Gets the children.
		/// </summary>
		public abstract IEnumerable<ILayoutElement> Children { get; }

		/// <summary>
		/// Gets the children count.
		/// </summary>
		public abstract int ChildrenCount { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is valid.
		/// </summary>
		public abstract bool IsValid { get; }

		/// <summary>
		/// Removes the child.
		/// </summary>
		/// <param name="element">The layout element.</param>
		public abstract void RemoveChild(ILayoutElement element);

		/// <summary>
		/// Replaces the child.
		/// </summary>
		/// <param name="oldElement">The existing layout element.</param>
		/// <param name="newElement">The replacement layout element.</param>
		public abstract void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);

		/// <summary>
		/// Gets the schema.
		/// </summary>
		/// <returns>The resulting value.</returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Reads the xml.
		/// </summary>
		/// <param name="reader">The XML reader to read from.</param>
		public abstract void ReadXml(XmlReader reader);

		/// <summary>
		/// Writes the xml.
		/// </summary>
		/// <param name="writer">The XML writer to write to.</param>
		public virtual void WriteXml(XmlWriter writer)
		{
			foreach (var child in Children)
			{
				var type = child.GetType();
				var serializer = XmlSerializersCache.GetSerializer(type);
				serializer.Serialize(writer, child);
			}
		}
	}
}