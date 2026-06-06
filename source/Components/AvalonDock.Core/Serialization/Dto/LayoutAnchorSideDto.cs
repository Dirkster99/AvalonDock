#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for a layout anchor side (auto-hide side).
	/// </summary>
	public class LayoutAnchorSideDto : LayoutGroupDto
	{
		/// <summary>Gets or sets the child anchor groups.</summary>
		[XmlElement("LayoutAnchorGroup", typeof(LayoutAnchorGroupDto))]
		public List<LayoutAnchorGroupDto> Children { get; set; } = new List<LayoutAnchorGroupDto>();
	}
}