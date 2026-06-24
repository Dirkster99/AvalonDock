#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for an anchorable pane.
	/// </summary>
	[XmlType("LayoutAnchorablePane")]
	public class LayoutAnchorablePaneDto : LayoutPositionableGroupDto
	{
		/// <summary>Gets or sets the pane identifier.</summary>
		[XmlAttribute]
		public string Id { get; set; }

		/// <summary>Gets or sets the pane name.</summary>
		[XmlAttribute]
		public string Name { get; set; }

		/// <summary>Gets or sets the child anchorables.</summary>
		[XmlElement("LayoutAnchorable", typeof(LayoutAnchorableDto))]
		public List<LayoutAnchorableDto> Children { get; set; } = new List<LayoutAnchorableDto>();
	}
}