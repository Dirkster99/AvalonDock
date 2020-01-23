/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock
{
	public class DocumentClosingEventArgs : CancelEventArgs
	{
		public DocumentClosingEventArgs(LayoutDocument document)
		{
			Document = document;
		}

		public LayoutDocument Document
		{
			get;
			private set;
		}
	}
}
