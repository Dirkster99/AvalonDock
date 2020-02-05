/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Layout
{
	public class LayoutElementEventArgs : EventArgs
	{
		/// <summary>
		/// Class constructor
		/// </summary>
		public LayoutElementEventArgs(LayoutElement element)
		{
			Element = element;
		}

		public LayoutElement Element { get; private set; }
	}
}
