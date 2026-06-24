#nullable disable
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// Abstract DTO base for content items (documents and anchorables).
	/// </summary>
	[XmlInclude(typeof(LayoutDocumentDto))]
	[XmlInclude(typeof(LayoutAnchorableDto))]
	public abstract class LayoutContentDto : LayoutElementDto
	{
		/// <summary>Gets or sets the title.</summary>
		[XmlAttribute]
		public string Title { get; set; }

		/// <summary>Gets or sets the content identifier.</summary>
		[XmlAttribute]
		public string ContentId { get; set; }

		/// <summary>Gets or sets a value indicating whether this item is selected.</summary>
		[XmlAttribute]
		public bool IsSelected { get; set; }

		/// <summary>Gets or sets a value indicating whether this is the last focused document.</summary>
		[XmlAttribute]
		public bool IsLastFocusedDocument { get; set; }

		/// <summary>Gets or sets the tooltip text.</summary>
		[XmlAttribute]
		public string ToolTip { get; set; }

		/// <summary>Gets or sets the floating left position.</summary>
		[XmlAttribute]
		public double FloatingLeft { get; set; }

		/// <summary>Gets or sets the floating top position.</summary>
		[XmlAttribute]
		public double FloatingTop { get; set; }

		/// <summary>Gets or sets the floating width.</summary>
		[XmlAttribute]
		public double FloatingWidth { get; set; }

		/// <summary>Gets or sets the floating height.</summary>
		[XmlAttribute]
		public double FloatingHeight { get; set; }

		/// <summary>Gets or sets a value indicating whether this item is maximized.</summary>
		[XmlAttribute]
		public bool IsMaximized { get; set; }

		/// <summary>Gets or sets a value indicating whether this item can be closed.</summary>
		[XmlAttribute]
		public bool CanClose { get; set; }

		/// <summary>Gets or sets a value indicating whether the default for CanClose is true (used for conditional serialization).</summary>
		[XmlIgnore]
		public bool CanCloseDefault { get; set; } = true;

		/// <summary>Gets or sets a value indicating whether this item can float.</summary>
		[XmlAttribute]
		public bool CanFloat { get; set; } = true;

		/// <summary>Gets or sets the last activation timestamp as an invariant-culture string.</summary>
		[XmlAttribute]
		public string LastActivationTimeStamp { get; set; }

		/// <summary>Gets or sets a value indicating whether this item can show on hover.</summary>
		[XmlAttribute]
		public bool CanShowOnHover { get; set; } = true;

		/// <summary>Gets or sets the previous container identifier.</summary>
		[XmlAttribute]
		public string PreviousContainerId { get; set; }

		/// <summary>Gets or sets the previous container index.</summary>
		[XmlAttribute]
		public int PreviousContainerIndex { get; set; } = -1;

		/// <summary>Determines whether the IsSelected property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeIsSelected() => IsSelected;

		/// <summary>Determines whether the IsLastFocusedDocument property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeIsLastFocusedDocument() => IsLastFocusedDocument;

		/// <summary>Determines whether the FloatingLeft property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingLeft() => FloatingLeft != 0.0;

		/// <summary>Determines whether the FloatingTop property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingTop() => FloatingTop != 0.0;

		/// <summary>Determines whether the FloatingWidth property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingWidth() => FloatingWidth != 0.0;

		/// <summary>Determines whether the FloatingHeight property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingHeight() => FloatingHeight != 0.0;

		/// <summary>Determines whether the IsMaximized property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeIsMaximized() => IsMaximized;

		/// <summary>Determines whether the CanClose property should be serialized.</summary>
		/// <returns>True if the value differs from the class default.</returns>
		public bool ShouldSerializeCanClose() => CanClose != CanCloseDefault;

		/// <summary>Determines whether the CanFloat property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanFloat() => !CanFloat;

		/// <summary>Determines whether the CanShowOnHover property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeCanShowOnHover() => !CanShowOnHover;

		/// <summary>Determines whether the PreviousContainerIndex property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializePreviousContainerIndex() => PreviousContainerId != null;
	}
}