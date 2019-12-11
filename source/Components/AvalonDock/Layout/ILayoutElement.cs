/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;

namespace AvalonDock.Layout
{
	public interface ILayoutElement : INotifyPropertyChanged, INotifyPropertyChanging
	{
		ILayoutContainer Parent
		{
			get;
		}
		ILayoutRoot Root
		{
			get;
		}
	}
}
