#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for a document pane group.
	/// </summary>
	[XmlType("LayoutDocumentPaneGroup")]
	public class LayoutDocumentPaneGroupDto : LayoutPositionableGroupDto
	{
		/// <summary>Gets or sets the orientation as a string ("Horizontal" or "Vertical").</summary>
		[XmlAttribute]
		public string Orientation { get; set; } = "Horizontal";

		/// <summary>Gets or sets the child document panes and pane groups.</summary>
		[XmlElement("LayoutDocumentPane", typeof(LayoutDocumentPaneDto))]
		[XmlElement("LayoutDocumentPaneGroup", typeof(LayoutDocumentPaneGroupDto))]
		public List<LayoutPositionableGroupDto> Children { get; set; } = new List<LayoutPositionableGroupDto>();
	}
}