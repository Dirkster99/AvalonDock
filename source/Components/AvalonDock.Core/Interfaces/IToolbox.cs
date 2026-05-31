using System;

[assembly: CLSCompliant(true)]

namespace AvalonDock.Core
{
	/// <summary>
	/// Defines the specific dock zone where a toolbox should be placed.
	/// Each side (left, right, bottom) is split into two zones for finer-grained control.
	/// </summary>
	public enum DockZone
	{
		/// <summary>Left sidebar, top section.</summary>
		LeftTop,

		/// <summary>Left sidebar, bottom section.</summary>
		LeftBottom,

		/// <summary>Right sidebar, top section.</summary>
		RightTop,

		/// <summary>Right sidebar, bottom section.</summary>
		RightBottom,

		/// <summary>Bottom panel, left section.</summary>
		BottomLeft,

		/// <summary>Bottom panel, right section.</summary>
		BottomRight
	}

	/// <summary>
	/// Identifies a side of the dock layout. Used both for coarse-grained placement
	/// and for bulk show/hide operations (e.g. VS Code-style layout toggle buttons).
	/// For finer-grained placement, see <see cref="DockZone"/>.
	/// </summary>
	public enum ToolboxSide
	{
		/// <summary>Docked to the left sidebar (<see cref="DockZone.LeftTop"/> + <see cref="DockZone.LeftBottom"/>).</summary>
		Left,

		/// <summary>Docked to the right sidebar (<see cref="DockZone.RightTop"/> + <see cref="DockZone.RightBottom"/>).</summary>
		Right,

		/// <summary>Docked to the bottom panel (<see cref="DockZone.BottomLeft"/> + <see cref="DockZone.BottomRight"/>).</summary>
		Bottom
	}

	/// <summary>
	/// Contract for toolbox (anchorable tool) view models that participate in the toggle docking system.
	/// Extends <see cref="IDockable"/> with placement and icon metadata.
	/// </summary>
	/// <remarks>
	/// <para>Implement this interface on your ViewModel and register it via DI
	/// to have it automatically placed in the ToggleDockingManager layout.</para>
	/// <para>The <see cref="Icon"/> property accepts any object that WPF can render:
	/// <c>ImageSource</c>, <c>UIElement</c> (Viewbox, Path), <c>DrawingImage</c>, or a string
	/// for font-icon glyphs.</para>
	/// <para>For a ready-to-use base class, see <c>AvalonDock.Mvvm.ToolboxBase</c>.</para>
	/// </remarks>
	public interface IToolbox : IDockable
	{
		/// <summary>Gets or sets the tooltip text shown on the sidebar toggle button.</summary>
		string? ToolTipText { get; set; }

		/// <summary>Gets or sets the dock zone where this toolbox is placed.</summary>
		DockZone Zone { get; set; }

		/// <summary>Gets or sets a value indicating whether this toolbox should be toggled open on startup.</summary>
		bool IsOpenByDefault { get; set; }

		/// <summary>Gets or sets a value indicating whether this toolbox is currently docked (visible). When <c>false</c> the toolbox is auto-hidden.</summary>
		bool IsOpen { get; set; }

		/// <summary>
		/// Gets or sets the icon content for the sidebar toggle button.
		/// Accepts any WPF-renderable object: ImageSource, UIElement (Viewbox, Path), DrawingImage, or null.
		/// </summary>
		object? Icon { get; set; }
	}
}