/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Themes
{
	using System;

	/// <summary>
	/// Implements the Arc Light theme for AvalonDock.
	/// Modern theme with compact tabs, rounded corners, and semi-transparent design.
	/// </summary>
	public class ArcLightTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri(
				"/AvalonDock.Themes.Arc;component/LightTheme.xaml",
				UriKind.Relative);
		}
	}
}