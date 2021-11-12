/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides an abstract class to implement a concrete floating window layout model.
	/// </summary>
	[Serializable]
	public abstract class LayoutFloatingWindow : LayoutElement, ILayoutContainer, IXmlSerializable
	{
		#region Properties

		/// <summary>Gets the list of <see cref="ILayoutElement"/> based children below this object.</summary>
		public abstract IEnumerable<ILayoutElement> Children { get; }

		/// <summary>Gets the number of children below this object.</summary>
		public abstract int ChildrenCount { get; }

		public abstract bool IsValid { get; }

		#endregion Properties

		#region Public Methods

		/// <summary>Remove the child element from the collection of children.</summary>
		/// <param name="element"></param>
		public abstract void RemoveChild(ILayoutElement element);

		/// <summary>Replace the child element with a new child in the collection of children.</summary>
		/// <param name="oldElement"></param>
		/// <param name="newElement"></param>
		public abstract void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);

		#region IXmlSerializable interface members

		/// <inheritdoc cref="IXmlSerializable"/>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <inheritdoc cref="IXmlSerializable"/>
		public abstract void ReadXml(XmlReader reader);

		/// <inheritdoc cref="IXmlSerializable"/>
		public virtual void WriteXml(XmlWriter writer)
		{
			foreach (var child in Children)
			{
				var type = child.GetType();
				var serializer = XmlSerializer.FromTypes(new[] { type })[0];
				serializer.Serialize(writer, child);
			}
		}

		#endregion IXmlSerializable interface members

		#endregion Public Methods
	}
}