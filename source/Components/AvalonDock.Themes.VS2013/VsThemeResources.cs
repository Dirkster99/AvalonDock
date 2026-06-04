/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Reflection;
using AvalonDock.Themes.VS;

namespace AvalonDock.Themes
{
	/// <summary>
	/// Provides access to the embedded GZIP-compressed VS2013 .vstheme resources
	/// as raw byte arrays, loaded via the shared <see cref="VsThemeResourceLoader"/>.
	/// </summary>
	internal static class VsThemeResources
	{
		private static readonly Assembly Asm = typeof(VsThemeResources).Assembly;
		private const string Prefix = "AvalonDock.Themes.Resources.";

		/// <summary>Gets the GZIP-compressed bytes of the VS2013 Blue theme.</summary>
		public static byte[] Blue => VsThemeResourceLoader.Load(Asm, Prefix + "vs2013blue.vstheme.gz");

		/// <summary>Gets the GZIP-compressed bytes of the VS2013 Dark theme.</summary>
		public static byte[] Dark => VsThemeResourceLoader.Load(Asm, Prefix + "vs2013dark.vstheme.gz");

		/// <summary>Gets the GZIP-compressed bytes of the VS2013 Light theme.</summary>
		public static byte[] Light => VsThemeResourceLoader.Load(Asm, Prefix + "vs2013light.vstheme.gz");
	}
}