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
	/// Implements the layout model for the <see cref="Controls.LayoutAnchorGroupControl"/>.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>, ILayoutPreviousContainer, ILayoutPaneSerializable
	{
		#region Overrides

		/// <inheritdoc />
		protected override bool GetVisibility() => Children.Count > 0;

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			if (_id != null) writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
			if (_previousContainer is ILayoutPaneSerializable paneSerializable) writer.WriteAttributeString("PreviousContainerId", paneSerializable.Id);
			base.WriteXml(writer);
		}

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id))) _id = reader.Value;
			if (reader.MoveToAttribute("PreviousContainerId")) ((ILayoutPreviousContainer)this).PreviousContainerId = reader.Value;
			base.ReadXml(reader);
		}

		#endregion

		#region ILayoutPreviousContainer Interface

		#region PreviousContainer

		[field: NonSerialized]
		private ILayoutContainer _previousContainer = null;
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

		#endregion

		string ILayoutPreviousContainer.PreviousContainerId { get; set; }

		#endregion

		#region ILayoutPaneSerializable Interface

		string _id;
		/// <inheritdoc />
		string ILayoutPaneSerializable.Id { get => _id; set => _id = value; }

		#endregion
	}
}
