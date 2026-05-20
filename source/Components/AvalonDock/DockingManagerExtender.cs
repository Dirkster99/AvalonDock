using System.Windows;
using System.Windows.Controls;

namespace AvalonDock
{
	/// <summary>
	/// Represents the docking Manager Extender.
	/// </summary>
	public class DockingManagerExtender
	{
		/// <summary>
		/// Gets or sets the overlay Window Factory.
		/// </summary>
		public IOverlayWindowFactory OverlayWindowFactory { get; set; }

		/// <summary>
		/// Gets or sets the floating Window Factory.
		/// </summary>
		public IFloatingWindowFactory FloatingWindowFactory { get; set; }

		/// <summary>
		/// Gets or sets the pane Header Factory.
		/// </summary>
		public IPaneHeaderFactory PaneHeaderFactory { get; set; }

		/// <summary>
		/// Gets or sets the auto Hide Strip Factory.
		/// </summary>
		public IAutoHideStripFactory AutoHideStripFactory { get; set; }
	}

	/// <summary>
	/// Defines the contract for overlay Window Factory.
	/// </summary>
	public interface IOverlayWindowFactory
	{
		/// <summary>
		/// Creates the create Overlay Window.
		/// </summary>
		/// <param name="manager">The manager.</param>
		/// <returns>The created value.</returns>
		FrameworkElement CreateOverlayWindow(DockingManager manager);
	}

	/// <summary>
	/// Defines the contract for floating Window Factory.
	/// </summary>
	public interface IFloatingWindowFactory
	{
		/// <summary>
		/// Creates the create Floating Window.
		/// </summary>
		/// <param name="manager">The manager.</param>
		/// <returns>The created value.</returns>
		Window CreateFloatingWindow(DockingManager manager);
	}

	/// <summary>
	/// Defines the contract for pane Header Factory.
	/// </summary>
	public interface IPaneHeaderFactory
	{
		/// <summary>
		/// Creates the create Pane Header.
		/// </summary>
		/// <returns>The created value.</returns>
		FrameworkElement CreatePaneHeader();
	}

	/// <summary>
	/// Defines the contract for auto Hide Strip Factory.
	/// </summary>
	public interface IAutoHideStripFactory
	{
		/// <summary>
		/// Creates the create Auto Hide Strip.
		/// </summary>
		/// <returns>The created value.</returns>
		FrameworkElement CreateAutoHideStrip();
	}
}