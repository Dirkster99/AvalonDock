/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Standard
{
	internal static class DpiHelper
	{
		private static Matrix _transformToDevice;
		private static Matrix _transformToDip;

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static DpiHelper()
		{
			using (var desktop = SafeDC.GetDesktop())
			{
				// Can get these in the static constructor.  They shouldn't vary window to window,
				// and changing the system DPI requires a restart.
				var pixelsPerInchX = NativeMethods.GetDeviceCaps(desktop, DeviceCap.LOGPIXELSX);
				var pixelsPerInchY = NativeMethods.GetDeviceCaps(desktop, DeviceCap.LOGPIXELSY);
				_transformToDip = Matrix.Identity;
				_transformToDip.Scale(96d / pixelsPerInchX, 96d / pixelsPerInchY);
				_transformToDevice = Matrix.Identity;
				_transformToDevice.Scale(pixelsPerInchX / 96d, pixelsPerInchY / 96d);
			}
		}

		/// <summary>
		/// Convert a point in device independent pixels (1/96") to a point in the system coordinates.
		/// </summary>
		/// <param name="logicalPoint">A point in the logical coordinate system.</param>
		/// <returns>Returns the parameter converted to the system's coordinates.</returns>
		public static Point LogicalPixelsToDevice(Point logicalPoint) => _transformToDevice.Transform(logicalPoint);

		/// <summary>
		/// Convert a point in system coordinates to a point in device independent pixels (1/96").
		/// </summary>
		/// <param name="logicalPoint">A point in the physical coordinate system.</param>
		/// <returns>Returns the parameter converted to the device independent coordinate system.</returns>
		public static Point DevicePixelsToLogical(Point devicePoint) => _transformToDip.Transform(devicePoint);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static Rect LogicalRectToDevice(Rect logicalRectangle)
		{
			var topLeft = LogicalPixelsToDevice(new Point(logicalRectangle.Left, logicalRectangle.Top));
			var bottomRight = LogicalPixelsToDevice(new Point(logicalRectangle.Right, logicalRectangle.Bottom));
			return new Rect(topLeft, bottomRight);
		}

		public static Rect DeviceRectToLogical(Rect deviceRectangle)
		{
			var topLeft = DevicePixelsToLogical(new Point(deviceRectangle.Left, deviceRectangle.Top));
			var bottomRight = DevicePixelsToLogical(new Point(deviceRectangle.Right, deviceRectangle.Bottom));
			return new Rect(topLeft, bottomRight);
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static Size LogicalSizeToDevice(Size logicalSize)
		{
			var pt = LogicalPixelsToDevice(new Point(logicalSize.Width, logicalSize.Height));
			return new Size { Width = pt.X, Height = pt.Y };
		}

		public static Size DeviceSizeToLogical(Size deviceSize)
		{
			var pt = DevicePixelsToLogical(new Point(deviceSize.Width, deviceSize.Height));
			return new Size(pt.X, pt.Y);
		}
	}
}
