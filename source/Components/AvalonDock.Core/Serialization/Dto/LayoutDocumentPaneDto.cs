#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for a document pane.
	/// </summary>
	[XmlType("LayoutDocumentPane")]
	public class LayoutDocumentPaneDto : LayoutPositionableGroupDto
	{
		/// <summary>Gets or sets the pane identifier.</summary>
		[XmlAttribute]
		public string Id { get; set; }

		/// <summary>Gets or sets a value indicating whether to show the header.</summary>
		[XmlAttribute]
		public bool ShowHeader { get; set; } = true;

		/// <summary>Gets or sets the child content items.</summary>
		[XmlElement("LayoutDocument", typeof(LayoutDocumentDto))]
		[XmlElement("LayoutAnchorable", typeof(LayoutAnchorableDto))]
		public List<LayoutContentDto> Children { get; set; } = new List<LayoutContentDto>();

		/// <summary>Determines whether the ShowHeader property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeShowHeader() => !ShowHeader;
	}
}