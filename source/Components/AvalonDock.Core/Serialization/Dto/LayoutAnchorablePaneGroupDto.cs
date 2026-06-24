#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for an anchorable pane group.
	/// </summary>
	[XmlType("LayoutAnchorablePaneGroup")]
	public class LayoutAnchorablePaneGroupDto : LayoutPositionableGroupDto
	{
		/// <summary>Gets or sets the orientation as a string ("Horizontal" or "Vertical").</summary>
		[XmlAttribute]
		public string Orientation { get; set; } = "Horizontal";

		/// <summary>Gets or sets the child anchorable panes and pane groups.</summary>
		[XmlElement("LayoutAnchorablePane", typeof(LayoutAnchorablePaneDto))]
		[XmlElement("LayoutAnchorablePaneGroup", typeof(LayoutAnchorablePaneGroupDto))]
		public List<LayoutPositionableGroupDto> Children { get; set; } = new List<LayoutPositionableGroupDto>();
	}
}