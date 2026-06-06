#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for a layout anchor group.
	/// </summary>
	[XmlType("LayoutAnchorGroup")]
	public class LayoutAnchorGroupDto : LayoutGroupDto
	{
		/// <summary>Gets or sets the group identifier.</summary>
		[XmlAttribute]
		public string Id { get; set; }

		/// <summary>Gets or sets the previous container identifier.</summary>
		[XmlAttribute]
		public string PreviousContainerId { get; set; }

		/// <summary>Gets or sets the child anchorables.</summary>
		[XmlElement("LayoutAnchorable", typeof(LayoutAnchorableDto))]
		public List<LayoutAnchorableDto> Children { get; set; } = new List<LayoutAnchorableDto>();
	}
}