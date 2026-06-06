#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for the layout root.
	/// </summary>
	[XmlRoot("LayoutRoot")]
	public class LayoutRootDto : LayoutElementDto
	{
		/// <summary>Gets or sets the root panel.</summary>
		public LayoutPanelDto RootPanel { get; set; }

		/// <summary>Gets or sets the top side.</summary>
		public LayoutAnchorSideDto TopSide { get; set; }

		/// <summary>Gets or sets the right side.</summary>
		public LayoutAnchorSideDto RightSide { get; set; }

		/// <summary>Gets or sets the left side.</summary>
		public LayoutAnchorSideDto LeftSide { get; set; }

		/// <summary>Gets or sets the bottom side.</summary>
		public LayoutAnchorSideDto BottomSide { get; set; }

		/// <summary>Gets or sets the floating windows.</summary>
		[XmlArrayItem("LayoutDocumentFloatingWindow", typeof(LayoutDocumentFloatingWindowDto))]
		[XmlArrayItem("LayoutAnchorableFloatingWindow", typeof(LayoutAnchorableFloatingWindowDto))]
		public List<LayoutFloatingWindowDto> FloatingWindows { get; set; } = new List<LayoutFloatingWindowDto>();

		/// <summary>Gets or sets the hidden anchorables.</summary>
		[XmlArrayItem("LayoutAnchorable", typeof(LayoutAnchorableDto))]
		public List<LayoutAnchorableDto> Hidden { get; set; } = new List<LayoutAnchorableDto>();
	}
}