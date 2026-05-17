using System.Windows;
using System.Windows.Controls;

namespace AvalonDock
{
	/// <summary>
	/// Factory interfaces for visual component creation in DockingManager.
	/// Themes can override these to provide custom visual implementations.
	/// Inspired by DockPanelSuite's DockPanelExtender pattern.
	/// </summary>
	public class DockingManagerExtender
	{
		/// <summary>
		/// Creates overlay window controls for drag-drop indicators.
		/// </summary>
		public IOverlayWindowFactory OverlayWindowFactory { get; set; }

		/// <summary>
		/// Creates floating window controls.
		/// </summary>
		public IFloatingWindowFactory FloatingWindowFactory { get; set; }

		/// <summary>
		/// Creates pane header/title bar controls.
		/// </summary>
		public IPaneHeaderFactory PaneHeaderFactory { get; set; }

		/// <summary>
		/// Creates auto-hide strip controls for side tabs.
		/// </summary>
		public IAutoHideStripFactory AutoHideStripFactory { get; set; }
	}

	/// <summary>
	/// Factory for creating overlay window controls used during drag-drop operations.
	/// </summary>
	public interface IOverlayWindowFactory
	{
		FrameworkElement CreateOverlayWindow(DockingManager manager);
	}

	/// <summary>
	/// Factory for creating floating window controls.
	/// </summary>
	public interface IFloatingWindowFactory
	{
		Window CreateFloatingWindow(DockingManager manager);
	}

	/// <summary>
	/// Factory for creating pane header/title bar controls.
	/// </summary>
	public interface IPaneHeaderFactory
	{
		FrameworkElement CreatePaneHeader();
	}

	/// <summary>
	/// Factory for creating auto-hide strip controls.
	/// </summary>
	public interface IAutoHideStripFactory
	{
		FrameworkElement CreateAutoHideStrip();
	}
}