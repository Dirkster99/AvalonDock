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
	/// Implements a Cancelable event that can be raised to ask the client application whether hiding this anchorable
	/// is OK or not.
	/// </summary>
	public class AnchorableHidingEventArgs : CancelEventArgs
	{
		/// <summary>
		/// Class constructor from the anchorable layout model.
		/// </summary>
		/// <param name="document"></param>
		public AnchorableHidingEventArgs(LayoutAnchorable anchorable)
		{
			Anchorable = anchorable;
		}

		/// <summary>
		/// Gets the model of the anchorable that is about to be hidden.
		/// </summary>
		public LayoutAnchorable Anchorable { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether an anchorable should be closed instead of hidden
		/// </summary>
		public bool CloseInsteadOfHide { get; set; }
	}
}