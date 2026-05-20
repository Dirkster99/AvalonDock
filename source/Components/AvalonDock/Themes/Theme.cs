/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows;

namespace AvalonDock.Themes
{
	/// <summary>
	/// Represents the theme.
	/// </summary>
	public abstract class Theme : DependencyObject, Core.IThemeInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Theme"/> class.
		/// </summary>
		public Theme()
		{
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		public virtual string Name => GetType().Name.Replace("Theme", string.Empty);

		/// <inheritdoc/>
		Uri Core.IThemeInfo.ResourceUri => GetResourceUri();

		/// <summary>
		/// Gets the get Resource Uri.
		/// </summary>
		/// <returns>The requested value.</returns>
		public abstract Uri GetResourceUri();
	}
}