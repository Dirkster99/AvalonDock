#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for a layout panel.
	/// </summary>
	[XmlType("LayoutPanel")]
	public class LayoutPanelDto : LayoutPositionableGroupDto
	{
		/// <summary>Gets or sets the orientation as a string ("Horizontal" or "Vertical").</summary>
		[XmlAttribute]
		public string Orientation { get; set; } = "Horizontal";

		/// <summary>Gets or sets a value indicating whether docking is allowed.</summary>
		[XmlAttribute]
		public bool CanDock { get; set; } = true;

		/// <summary>Gets or sets the child elements.</summary>
		[XmlElement("LayoutPanel", typeof(LayoutPanelDto))]
		[XmlElement("LayoutDocumentPaneGroup", typeof(LayoutDocumentPaneGroupDto))]
		[XmlElement("LayoutDocumentPane", typeof(LayoutDocumentPaneDto))]
		[XmlElement("LayoutAnchorablePaneGroup", typeof(LayoutAnchorablePaneGroupDto))]
		[XmlElement("LayoutAnchorablePane", typeof(LayoutAnchorablePaneDto))]
		public List<LayoutPositionableGroupDto> Children { get; set; } = new List<LayoutPositionableGroupDto>();

		/// <summary>Determines whether the CanDock property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanDock() => !CanDock;
	}
}