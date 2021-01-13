/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections;
using System.Collections.Generic;

namespace AvalonDock
{
	public sealed class ReferenceEqualityComparer : IEqualityComparer, IEqualityComparer<object>
	{
		public static ReferenceEqualityComparer Default { get; } = new ReferenceEqualityComparer();

		public new bool Equals(object x, object y) => ReferenceEquals(x, y);
		public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}
}