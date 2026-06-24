#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock.Core.Serialization.Dto
{
	/// <summary>
	/// Abstract DTO base for layout containers (groups).
	/// </summary>
	[XmlInclude(typeof(LayoutPositionableGroupDto))]
	[XmlInclude(typeof(LayoutAnchorSideDto))]
	[XmlInclude(typeof(LayoutAnchorGroupDto))]
	public abstract class LayoutGroupDto : LayoutElementDto
	{
	}

	/// <summary>
	/// Abstract DTO base for positionable groups (panels, panes, pane groups).
	/// </summary>
	[XmlInclude(typeof(LayoutPanelDto))]
	[XmlInclude(typeof(LayoutDocumentPaneDto))]
	[XmlInclude(typeof(LayoutDocumentPaneGroupDto))]
	[XmlInclude(typeof(LayoutAnchorablePaneDto))]
	[XmlInclude(typeof(LayoutAnchorablePaneGroupDto))]
	public abstract class LayoutPositionableGroupDto : LayoutGroupDto
	{
		/// <summary>Gets or sets the dock width as a string (e.g. "1*", "200").</summary>
		[XmlAttribute]
		public string DockWidth { get; set; }

		/// <summary>Gets or sets the dock height as a string (e.g. "1*", "200").</summary>
		[XmlAttribute]
		public string DockHeight { get; set; }

		/// <summary>Gets or sets the dock minimum width.</summary>
		[XmlAttribute]
		public double DockMinWidth { get; set; } = 25.0;

		/// <summary>Gets or sets the dock minimum height.</summary>
		[XmlAttribute]
		public double DockMinHeight { get; set; } = 25.0;

		/// <summary>Gets or sets the floating width.</summary>
		[XmlAttribute]
		public double FloatingWidth { get; set; }

		/// <summary>Gets or sets the floating height.</summary>
		[XmlAttribute]
		public double FloatingHeight { get; set; }

		/// <summary>Gets or sets the floating left position.</summary>
		[XmlAttribute]
		public double FloatingLeft { get; set; }

		/// <summary>Gets or sets the floating top position.</summary>
		[XmlAttribute]
		public double FloatingTop { get; set; }

		/// <summary>Gets or sets a value indicating whether this group is maximized.</summary>
		[XmlAttribute]
		public bool IsMaximized { get; set; }

		/// <summary>Determines whether the DockMinWidth property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeDockMinWidth() => DockMinWidth != 25.0;

		/// <summary>Determines whether the DockMinHeight property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeDockMinHeight() => DockMinHeight != 25.0;

		/// <summary>Determines whether the FloatingWidth property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingWidth() => FloatingWidth != 0.0;

		/// <summary>Determines whether the FloatingHeight property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingHeight() => FloatingHeight != 0.0;

		/// <summary>Determines whether the FloatingLeft property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingLeft() => FloatingLeft != 0.0;

		/// <summary>Determines whether the FloatingTop property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeFloatingTop() => FloatingTop != 0.0;

		/// <summary>Determines whether the IsMaximized property should be serialized.</summary>
		/// <returns>True if the value differs from default.</returns>
		public bool ShouldSerializeIsMaximized() => IsMaximized;
	}
}