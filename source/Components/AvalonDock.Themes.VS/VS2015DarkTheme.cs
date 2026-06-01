/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.IO;
using System.Reflection;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// VS2015 Dark theme loaded from an embedded .vstheme resource file.
	/// </summary>
	public class VS2015DarkTheme : VsTheme
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VS2015DarkTheme"/> class.
		/// </summary>
		public VS2015DarkTheme()
			: base(LoadEmbeddedResource("AvalonDock.Themes.VS.Resources.vs2015dark.vstheme"))
		{
		}

		private static Stream LoadEmbeddedResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		}
	}
}