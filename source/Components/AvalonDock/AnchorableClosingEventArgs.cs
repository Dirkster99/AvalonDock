/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System.ComponentModel;

namespace AvalonDock
{
	/// <summary>
	/// Implements a Cancelable event that can be raised to ask the client application whether closing this anchorable
	/// and removing its content (viewmodel) is OK or not.
	/// </summary>
	public class AnchorableClosingEventArgs : CancelEventArgs
	{
		/// <summary>
		/// Class constructor from the anchorable layout model.
		/// </summary>
		/// <param name="document"></param>
		public AnchorableClosingEventArgs(LayoutAnchorable anchorable)
		{
			Anchorable = anchorable;
		}

		/// <summary>
		/// Gets the model of the anchorable that is about to be closed.
		/// </summary>
		public LayoutAnchorable Anchorable { get; private set; }
	}
}