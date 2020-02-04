/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a base implementation for the abstract <see cref="DropTarget{T}"/> class.
	/// </summary>
	internal abstract class DropTargetBase : DependencyObject
	{
		#region Properties

		#region IsDraggingOver

		/// <summary>
		/// IsDraggingOver Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsDraggingOverProperty = DependencyProperty.RegisterAttached("IsDraggingOver", typeof(bool), typeof(DropTargetBase),
				new FrameworkPropertyMetadata((bool)false));

		/// <summary>
		/// Gets the IsDraggingOver property.
		/// This dependency property indicates if user is dragging a window over the target element.
		/// </summary>
		public static bool GetIsDraggingOver(DependencyObject d)
		{
			return (bool)d.GetValue(IsDraggingOverProperty);
		}

		/// <summary>
		/// Sets the IsDraggingOver property.
		/// This dependency property indicates if user is dragging away a window from the target element.
		/// </summary>
		public static void SetIsDraggingOver(DependencyObject d, bool value)
		{
			d.SetValue(IsDraggingOverProperty, value);
		}

		#endregion IsDraggingOver

		#endregion Properties
	}
}
