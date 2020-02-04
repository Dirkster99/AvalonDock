/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;
using System.Windows.Threading;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Abstract class to implement base for various drop target implementations on <see cref="DockingManager"/>,
	/// <see cref="LayoutAnchorablePaneControl"/>, <see cref="LayoutDocumentPaneControl"/> etc.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal abstract class DropTarget<T> : DropTargetBase, IDropTarget where T : FrameworkElement
	{
		#region fields
		private Rect[] _detectionRect;
		private T _targetElement;
		private DropTargetType _type;
		#endregion fields

		#region Constructors

		protected DropTarget(T targetElement, Rect detectionRect, DropTargetType type)
		{
			_targetElement = targetElement;
			_detectionRect = new Rect[] { detectionRect };
			_type = type;
		}

		protected DropTarget(T targetElement, IEnumerable<Rect> detectionRects, DropTargetType type)
		{
			_targetElement = targetElement;
			_detectionRect = detectionRects.ToArray();
			_type = type;
		}

		#endregion

		#region Properties

		public Rect[] DetectionRects
		{
			get
			{
				return _detectionRect;
			}
		}

		public T TargetElement
		{
			get
			{
				return _targetElement;
			}
		}

		public DropTargetType Type
		{
			get
			{
				return _type;
			}
		}

		#endregion

		#region Overrides

		protected virtual void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
		}

		protected virtual void Drop(LayoutDocumentFloatingWindow floatingWindow)
		{
		}

		#endregion

		#region Public Methods

		public void Drop(LayoutFloatingWindow floatingWindow)
		{
			var root = floatingWindow.Root;
			var currentActiveContent = floatingWindow.Root.ActiveContent;
			var fwAsAnchorable = floatingWindow as LayoutAnchorableFloatingWindow;

			if (fwAsAnchorable != null)
			{
				this.Drop(fwAsAnchorable);
			}
			else
			{
				var fwAsDocument = floatingWindow as LayoutDocumentFloatingWindow;
				this.Drop(fwAsDocument);
			}

			Dispatcher.BeginInvoke(new Action(() =>
				{
					currentActiveContent.IsSelected = false;
					currentActiveContent.IsActive = false;
					currentActiveContent.IsActive = true;
				}), DispatcherPriority.Background);
		}

		public virtual bool HitTest(Point dragPoint)
		{
			return _detectionRect.Any(dr => dr.Contains(dragPoint));
		}

		public abstract Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindow);

		public void DragEnter()
		{
			SetIsDraggingOver(TargetElement, true);
		}

		public void DragLeave()
		{
			SetIsDraggingOver(TargetElement, false);
		}

		#endregion
	}
}
