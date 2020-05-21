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
	/// Defines an abstract class to implement a custom AvalonDock theme
	/// based on a <see cref="ResourceDictionary"/> object rather than an XAML Uri.
	/// 
	/// Dirk: Class is referenced in code but is never used since there is no concrete inheriting class.
	/// ToDo: Remove this class and code references in AvalonDock?
	/// </summary>
	public abstract class DictionaryTheme : Theme
	{
		#region Constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		public DictionaryTheme()
		{
		}

		/// <summary>
		/// Class constructor from theme specific resource dictionary.
		/// </summary>
		/// <param name="themeResourceDictionary"></param>
		public DictionaryTheme(ResourceDictionary themeResourceDictionary)
		{
			this.ThemeResourceDictionary = themeResourceDictionary;
		}

		#endregion Constructors
		/// <summary>
		/// Gets the resource dictionary that is associated with this AvalonDock theme.
		/// </summary>
		public ResourceDictionary ThemeResourceDictionary { get; private set; }

		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return null;
		}
	}
}
