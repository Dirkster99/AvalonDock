/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Controls
{
	internal class WindowActivateEventArgs : EventArgs
	{
		#region Constructors

		public WindowActivateEventArgs(IntPtr hwndActivating)
		{
			HwndActivating = hwndActivating;
		}

		#endregion

		#region Properties

		public IntPtr HwndActivating
		{
			get;
			private set;
		}

		#endregion
	}
}
