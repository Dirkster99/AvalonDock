/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Layout
{
	/// <summary>Defines a layout element that can be positioned in a Grid like environment.</summary>
	internal interface ILayoutPositionableElement : ILayoutElement, ILayoutElementForFloatingWindow
	{
		/// <summary>Gets/sets the <see cref="GridLength"/> of the dock width for this positionable layout element.</summary>
		GridLength DockWidth { get; set; }

		/// <summary>Gets/sets the <see cref="GridLength"/> of the dock height for this positionable layout element.</summary>
		GridLength DockHeight { get; set; }

		/// <summary>Gets the current DockWidth value if the DockWidth is absolute and the DockWidth value is greater equal DockMinWidth
		/// or the DockMinWidth, otherwise.</summary>
		double FixedDockWidth { get; }

		double ResizableAbsoluteDockWidth { get; set; }

		/// <summary>Gets the current DockHeight value if the DockHeight is absolute and the DockHeight value is greater equal DockMinHeight
		/// or the DockMinHeight, otherwise.</summary>
		double FixedDockHeight { get; }

		double ResizableAbsoluteDockHeight { get; set; }

		/// <summary>Gets/sets the minimum width of the docked positionable element.</summary>
		double DockMinWidth { get; set; }

		/// <summary>Gets/sets the minimum height of the docked positionable element.</summary>
		double DockMinHeight { get; set; }

		/// <summary>Gets/sets whether duplicated content is allowed or not.</summary>
		bool AllowDuplicateContent { get; set; }

		/// <summary>Gets whether the <see cref="ILayoutElement"/> is currently visible or not.</summary>
		bool IsVisible { get; }

		/// <summary>Computes the minimum dock with of this element including its childrens minimum dock witdh.</summary>
		double CalculatedDockMinWidth();

		/// <summary>Computes the minimum dock with of this element including its childrens minimum dock witdh.</summary>
		double CalculatedDockMinHeight();
	}


	/// <summary>Defines a layout element that supports actual width and height properties.</summary>
	internal interface ILayoutPositionableElementWithActualSize : ILayoutPositionableElement
	{
		/// <summary>Gets/sets the actual width the positionable layout element.</summary>
		double ActualWidth { get; set; }

		/// <summary>Gets/sets the actual height the positionable layout element.</summary>
		double ActualHeight { get; set; }
	}

	/// <summary>Defines a layout element that supports position properties for a floating window.</summary>
	internal interface ILayoutElementForFloatingWindow
	{
		/// <summary>Invoke this method to raise the FloatingPropertiesUpdated event to inform subscribers of the change.</summary>
		void RaiseFloatingPropertiesUpdated();

		/// <summary>Gets/sets the width of the floating window.</summary>
		double FloatingWidth { get; set; }

		/// <summary>Gets/sets the height of the floating window.</summary>
		double FloatingHeight { get; set; }

		/// <summary>Gets/sets the left position of the floating window.</summary>
		double FloatingLeft { get; set; }

		/// <summary>Gets/sets the top position of the floating window.</summary>
		double FloatingTop { get; set; }

		/// <summary>Gets/sets whether the floating window is maximized or not.</summary>
		bool IsMaximized { get; set; }
	}
}
