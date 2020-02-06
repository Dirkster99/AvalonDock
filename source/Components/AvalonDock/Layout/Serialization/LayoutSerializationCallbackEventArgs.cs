/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;

namespace AvalonDock.Layout.Serialization
{
	public class LayoutSerializationCallbackEventArgs : CancelEventArgs
	{
		#region constructors

		public LayoutSerializationCallbackEventArgs(LayoutContent model, object previousContent)
		{
			Cancel = false;
			Model = model;
			Content = previousContent;
		}

		#endregion constructors

		#region Properties

		public LayoutContent Model
		{
			get; private set;
		}

		public object Content
		{
			get; set;
		}

		#endregion Properties
	}
}
