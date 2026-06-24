#nullable disable
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// Abstract DTO base for floating windows.
	/// </summary>
	[XmlInclude(typeof(LayoutDocumentFloatingWindowDto))]
	[XmlInclude(typeof(LayoutAnchorableFloatingWindowDto))]
	public abstract class LayoutFloatingWindowDto : LayoutElementDto
	{
	}

	/// <summary>
	/// DTO for a document floating window.
	/// </summary>
	[XmlType("LayoutDocumentFloatingWindow")]
	public class LayoutDocumentFloatingWindowDto : LayoutFloatingWindowDto
	{
		/// <summary>Gets or sets the root panel.</summary>
		[XmlElement("LayoutDocumentPaneGroup")]
		public LayoutDocumentPaneGroupDto RootPanel { get; set; }
	}

	/// <summary>
	/// DTO for an anchorable floating window.
	/// </summary>
	[XmlType("LayoutAnchorableFloatingWindow")]
	public class LayoutAnchorableFloatingWindowDto : LayoutFloatingWindowDto
	{
		/// <summary>Gets or sets the root panel.</summary>
		[XmlElement("LayoutAnchorablePaneGroup")]
		public LayoutAnchorablePaneGroupDto RootPanel { get; set; }
	}
}