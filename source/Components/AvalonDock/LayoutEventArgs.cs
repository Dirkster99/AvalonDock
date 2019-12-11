/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using AvalonDock.Layout;

namespace AvalonDock
{
	class LayoutEventArgs : EventArgs
	{
		public LayoutEventArgs(LayoutRoot layoutRoot)
		{
			LayoutRoot = layoutRoot;
		}

		public LayoutRoot LayoutRoot
		{
			get;
			private set;
		}
	}
}
