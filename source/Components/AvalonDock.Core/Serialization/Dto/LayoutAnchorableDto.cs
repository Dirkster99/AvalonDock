#nullable disable
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// DTO for a layout anchorable (tool window).
	/// </summary>
	[XmlType("LayoutAnchorable")]
	public class LayoutAnchorableDto : LayoutContentDto
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorableDto"/> class.
		/// </summary>
		public LayoutAnchorableDto()
		{
			// Anchorables default to CanClose=false
			CanClose = false;
			CanCloseDefault = false;
		}

		/// <summary>Gets or sets a value indicating whether this anchorable can be hidden.</summary>
		[XmlAttribute]
		public bool CanHide { get; set; } = true;

		/// <summary>Gets or sets a value indicating whether this anchorable can auto-hide.</summary>
		[XmlAttribute]
		public bool CanAutoHide { get; set; } = true;

		/// <summary>Gets or sets the auto-hide width.</summary>
		[XmlAttribute]
		public double AutoHideWidth { get; set; }

		/// <summary>Gets or sets the auto-hide height.</summary>
		[XmlAttribute]
		public double AutoHideHeight { get; set; }

		/// <summary>Gets or sets the auto-hide minimum width.</summary>
		[XmlAttribute]
		public double AutoHideMinWidth { get; set; } = 100.0;

		/// <summary>Gets or sets the auto-hide minimum height.</summary>
		[XmlAttribute]
		public double AutoHideMinHeight { get; set; } = 100.0;

		/// <summary>Gets or sets a value indicating whether this anchorable can dock as a tabbed document.</summary>
		[XmlAttribute]
		public bool CanDockAsTabbedDocument { get; set; } = true;

		/// <summary>Gets or sets a value indicating whether this anchorable can be moved.</summary>
		[XmlAttribute]
		public bool CanMove { get; set; } = true;

		/// <summary>Determines whether the CanHide property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanHide() => !CanHide;

		/// <summary>Determines whether the CanAutoHide property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanAutoHide() => !CanAutoHide;

		/// <summary>Determines whether the AutoHideWidth property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeAutoHideWidth() => AutoHideWidth > 0;

		/// <summary>Determines whether the AutoHideHeight property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeAutoHideHeight() => AutoHideHeight > 0;

		/// <summary>Determines whether the AutoHideMinWidth property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeAutoHideMinWidth() => AutoHideMinWidth != 25.0;

		/// <summary>Determines whether the AutoHideMinHeight property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeAutoHideMinHeight() => AutoHideMinHeight != 25.0;

		/// <summary>Determines whether the CanDockAsTabbedDocument property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanDockAsTabbedDocument() => !CanDockAsTabbedDocument;

		/// <summary>Determines whether the CanMove property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanMove() => !CanMove;
	}
}