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
	/// <summary>Defines a base class to implement a method for storing the current AvalonDock theme
	/// that provides a XAML Uri pointing to a <see cref="ResourceDictionary"/>.
	/// 
	/// This class can be used to create customized themes by loading a <see cref="ResourceDictionary"/>
	/// from an existing theme (by using theme.GetResourceUri()), and then replacing some key colors
	/// (typically the "Accent" colors).
	/// 
	/// See Issue https://github.com/Dirkster99/AvalonDock/issues/189 for more details.</summary>
	public abstract class DictionaryTheme : Theme
	{
		#region Constructors
		/// <summary>Class constructor</summary>
		public DictionaryTheme()
		{
		}

		/// <summary>Class constructor from theme specific resource dictionary.</summary>
		/// <param name="themeResourceDictionary"></param>
		public DictionaryTheme(ResourceDictionary themeResourceDictionary)
		{
			this.ThemeResourceDictionary = themeResourceDictionary;
		}

		#endregion Constructors

		/// <summary>Gets the resource dictionary that is associated with this AvalonDock theme.</summary>
		public ResourceDictionary ThemeResourceDictionary { get; private set; }

		/// <summary>Gets the <see cref="Uri"/> of the XAML that contains the definition for this AvalonDock theme.</summary>
		/// <returns><see cref="Uri"/> of the XAML that contains the definition for this custom AvalonDock theme</returns>
		public override Uri GetResourceUri()
		{
			return null;
		}
	}
}
