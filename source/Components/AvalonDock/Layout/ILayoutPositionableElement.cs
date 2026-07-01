using System.Windows;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout positionable element.
	/// </summary>
	internal interface ILayoutPositionableElement : ILayoutElement, ILayoutElementForFloatingWindow
	{
		/// <summary>
		/// Gets or sets the dock width.
		/// </summary>
		GridLength DockWidth { get; set; }

		/// <summary>
		/// Gets or sets the dock height.
		/// </summary>
		GridLength DockHeight { get; set; }

		/// <summary>
		/// Gets the fixed dock width.
		/// </summary>
		double FixedDockWidth { get; }

		/// <summary>
		/// Gets or sets the resizable absolute dock width.
		/// </summary>
		double ResizableAbsoluteDockWidth { get; set; }

		/// <summary>
		/// Gets the fixed dock height.
		/// </summary>
		double FixedDockHeight { get; }

		/// <summary>
		/// Gets or sets the resizable absolute dock height.
		/// </summary>
		double ResizableAbsoluteDockHeight { get; set; }

		/// <summary>
		/// Gets or sets the dock min width.
		/// </summary>
		double DockMinWidth { get; set; }

		/// <summary>
		/// Gets or sets the dock min height.
		/// </summary>
		double DockMinHeight { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether duplicate content is allowed.
		/// </summary>
		bool AllowDuplicateContent { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance is visible.
		/// </summary>
		bool IsVisible { get; }

		/// <summary>
		/// Executes the calculated dock min width operation.
		/// </summary>
		/// <returns>The resulting value.</returns>
		double CalculatedDockMinWidth();

		/// <summary>
		/// Executes the calculated dock min height operation.
		/// </summary>
		/// <returns>The resulting value.</returns>
		double CalculatedDockMinHeight();
	}

	/// <summary>
	/// Interface for layout positionable element with actual size.
	/// </summary>
	internal interface ILayoutPositionableElementWithActualSize : ILayoutPositionableElement
	{
		/// <summary>
		/// Gets or sets the actual width.
		/// </summary>
		double ActualWidth { get; set; }

		/// <summary>
		/// Gets or sets the actual height.
		/// </summary>
		double ActualHeight { get; set; }
	}

	/// <summary>
	/// Interface for layout element for floating window.
	/// </summary>
	internal interface ILayoutElementForFloatingWindow
	{
		/// <summary>
		/// Raises the floating properties updated.
		/// </summary>
		void RaiseFloatingPropertiesUpdated();

		/// <summary>
		/// Gets or sets the floating width.
		/// </summary>
		double FloatingWidth { get; set; }

		/// <summary>
		/// Gets or sets the floating height.
		/// </summary>
		double FloatingHeight { get; set; }

		/// <summary>
		/// Gets or sets the floating left.
		/// </summary>
		double FloatingLeft { get; set; }

		/// <summary>
		/// Gets or sets the floating top.
		/// </summary>
		double FloatingTop { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is maximized.
		/// </summary>
		bool IsMaximized { get; set; }
	}
}