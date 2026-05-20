/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout previous container.
	/// </summary>
	public interface ILayoutPreviousContainer
	{
		/// <summary>
		/// Gets or sets the previous container.
		/// </summary>
		ILayoutContainer PreviousContainer { get; set; }

		/// <summary>
		/// Gets or sets the previous container id.
		/// </summary>
		string PreviousContainerId { get; set; }
	}
}