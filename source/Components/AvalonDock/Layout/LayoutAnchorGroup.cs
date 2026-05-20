/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout anchor group.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>, ILayoutPreviousContainer, ILayoutPaneSerializable, Core.Serialization.ISerializableLayoutPane
	{
		/// <inheritdoc/>
		protected override bool GetVisibility() => Children.Count > 0;

		/// <inheritdoc/>
		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			if (_id != null) writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
			if (_previousContainer is ILayoutPaneSerializable paneSerializable) writer.WriteAttributeString("PreviousContainerId", paneSerializable.Id);
			base.WriteXml(writer);
		}

		/// <inheritdoc/>
		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id))) _id = reader.Value;
			if (reader.MoveToAttribute("PreviousContainerId")) ((ILayoutPreviousContainer)this).PreviousContainerId = reader.Value;
			base.ReadXml(reader);
		}

		[field: NonSerialized]
		private ILayoutContainer _previousContainer = null;

		/// <inheritdoc/>
		[XmlIgnore]
		ILayoutContainer ILayoutPreviousContainer.PreviousContainer
		{
			get => _previousContainer;
			set
			{
				if (value == _previousContainer) return;
				_previousContainer = value;
				RaisePropertyChanged(nameof(ILayoutPreviousContainer.PreviousContainer));
				if (_previousContainer is ILayoutPaneSerializable paneSerializable && paneSerializable.Id == null)
					paneSerializable.Id = Guid.NewGuid().ToString();
			}
		}

		/// <inheritdoc/>
		string ILayoutPreviousContainer.PreviousContainerId { get; set; }

		private string _id;

		/// <inheritdoc/>
		string ILayoutPaneSerializable.Id { get => _id; set => _id = value; }

		/// <inheritdoc/>
		string Core.Serialization.ISerializableLayoutPane.Id { get => _id; set => _id = value; }
	}
}