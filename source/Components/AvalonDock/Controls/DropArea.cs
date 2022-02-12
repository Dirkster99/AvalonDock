/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;
using System.Windows;

namespace AvalonDock.Controls
{
	/// <summary>Determines the type of drop area of a <see cref="FrameworkElement"/> that may be valid as a drop target of a drag and drop operation.</summary>
	public enum DropAreaType
	{
		/// <summary> This type of drop area identifies a <seealso cref="AvalonDock.DockingManager"/> which is the visual root of the AvalonDock control library.</summary>
		DockingManager,

		/// <summary>This type of drop area identifies a <see cref="LayoutDocumentPaneControl"/>.</summary>
		DocumentPane,

		/// <summary>This type of drop area identifies a <see cref="LayoutDocumentPaneGroupControl"/>.</summary>
		DocumentPaneGroup,

		/// <summary>This type of drop area identifies a <see cref="LayoutAnchorablePaneControl"/>.</summary>
		AnchorablePane,
	}

	/// <summary>Describes a drop target which can be the final position of an item that is being dragged and dropped to dock it somewhere else in the UI of the framework.</summary>
	public interface IDropArea
	{
		/// <summary>Gets the width, height, and location of a rectangle that describes the drop target of a drag and drop operation on the users screen.</summary>
		Rect DetectionRect { get; }

		/// <summary> Gets the type of drop area for this drop target.</summary>
		DropAreaType Type { get; }

		Point TransformToDeviceDPI(Point dragPosition);
	}

	/// <inheritdoc />
	/// <summary>
	/// Implements a control class that can act as a drop target. A drop target is a control that can
	/// be the target of drag & drop (dock) operation of a second control.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="IDropArea"/>
	/// <seealso cref="FrameworkElement"/>
	public class DropArea<T> : IDropArea where T : FrameworkElement
	{
		#region Constructors

		/// <summary>Class constructor from control that can be used as drop target and it's type of drop area. </summary>
		/// <param name="areaElement"></param>
		/// <param name="type">the type of drop area for this drop target.</param>
		internal DropArea(T areaElement, DropAreaType type)
		{
			AreaElement = areaElement;
			DetectionRect = areaElement.GetScreenArea();
			Type = type;
		}

		#endregion Constructors

		#region IDropArea

		/// <inheritdoc />
		public Rect DetectionRect { get; }

		/// <inheritdoc />
		public DropAreaType Type { get; }

		public Point TransformToDeviceDPI(Point dragPosition)
		{
			return AreaElement.TransformToDeviceDPI(dragPosition);
		}

		#endregion IDropArea

		#region Properties

		/// <summary>Gets the <see cref="FrameworkElement"/> that implements a drop target for a drag & drop (dock) operation.</summary>
		[Bindable(false), Description("Gets the FrameworkElement that implements a drop target for a drag & drop (dock) operation."), Category("Other")]
		public T AreaElement { get; }

		#endregion Properties
	}
}