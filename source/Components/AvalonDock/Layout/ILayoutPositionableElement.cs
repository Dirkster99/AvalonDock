/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Layout
{
	internal interface ILayoutPositionableElement : ILayoutElement, ILayoutElementForFloatingWindow
	{
		GridLength DockWidth
		{
			get;
			set;
		}

		double FixedDockWidth { get; }

		double ResizableAbsoluteDockWidth
		{
			get;
			set;
		}

		GridLength DockHeight
		{
			get;
			set;
		}

		double FixedDockHeight { get; }

		double ResizableAbsoluteDockHeight
		{
			get;
			set;
		}

		double CalculatedDockMinWidth();

		double DockMinWidth
		{
			get; set;
		}

		double CalculatedDockMinHeight();

		double DockMinHeight
		{
			get; set;
		}

		bool AllowDuplicateContent
		{
			get; set;
		}

		bool IsVisible
		{
			get;
		}
	}


	internal interface ILayoutPositionableElementWithActualSize : ILayoutPositionableElement
	{
		double ActualWidth
		{
			get; set;
		}
		double ActualHeight
		{
			get; set;
		}
	}

	internal interface ILayoutElementForFloatingWindow
	{
		void RaiseFloatingPropertiesUpdated();

		double FloatingWidth
		{
			get; set;
		}
		double FloatingHeight
		{
			get; set;
		}
		double FloatingLeft
		{
			get; set;
		}
		double FloatingTop
		{
			get; set;
		}
		bool IsMaximized
		{
			get; set;
		}
	}
}
