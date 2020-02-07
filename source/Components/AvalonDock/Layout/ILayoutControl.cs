/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>Defines a control class that hosts a <see cref="ILayoutElement"/> as its model</summary>
	public interface ILayoutControl
	{
		/// <summary>Gets the <see cref="ILayoutElement"/> model for this control.</summary>
		ILayoutElement Model
		{
			get;
		}
	}
}
