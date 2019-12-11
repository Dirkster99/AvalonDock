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
	public abstract class DictionaryTheme : Theme
	{
		#region Constructors

		public DictionaryTheme()
		{
		}

		public DictionaryTheme(ResourceDictionary themeResourceDictionary)
		{
			this.ThemeResourceDictionary = themeResourceDictionary;
		}

		#endregion

		#region Properties

		public ResourceDictionary ThemeResourceDictionary
		{
			get;
			private set;
		}

		#endregion

		#region Overrides

		public override Uri GetResourceUri()
		{
			return null;
		}

		#endregion
	}
}
