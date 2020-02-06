/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Layout
{
	/// <summary>Defines a bitwise flag to indicate a preferred anchoring location of a new <see cref="LayoutAnchorable"/> child entry into a corresponding <see cref="AnchorSide"/>.</summary>
	[Flags]
	public enum AnchorableShowStrategy : byte
	{
		Most = 0x0001,

		/// <summary>
		/// This value is equivalent to <see cref="AnchorSide.Left"/>.
		/// </summary>
		Left = 0x0002,

		/// <summary>
		/// This value is equivalent to <see cref="AnchorSide.Right"/>.
		/// </summary>
		Right = 0x0004,

		/// <summary>
		/// This value is equivalent to <see cref="AnchorSide.Top"/>.
		/// </summary>
		Top = 0x0010,

		/// <summary>
		/// This value is equivalent to <see cref="AnchorSide.Bottom"/>.
		/// </summary>
		Bottom = 0x0020,
	}
}
