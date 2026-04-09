/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Controls
{
	internal class ReentrantFlag
	{
		private bool _flag = false;

		public bool CanEnter
		{
			get
			{
				return !_flag;
			}
		}

		public _ReentrantFlagHandler Enter()
		{
			if (_flag)
				throw new InvalidOperationException();
			return new _ReentrantFlagHandler(this);
		}

		public class _ReentrantFlagHandler : IDisposable
		{
			private ReentrantFlag _owner;

			public _ReentrantFlagHandler(ReentrantFlag owner)
			{
				_owner = owner;
				_owner._flag = true;
			}

			public void Dispose()
			{
				_owner._flag = false;
			}
		}
	}
}