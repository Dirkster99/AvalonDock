/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Standard
{
	/// <summary>
	/// Provides helper members for dpi Helper.
	/// </summary>
	internal static class DpiHelper
	{
		/// <summary>
		/// The transform To Device field.
		/// </summary>
		private static Matrix _transformToDevice;

		/// <summary>
		/// The transform To Dip field.
		/// </summary>
		private static Matrix _transformToDip;

		/// <summary>
		/// Initializes static members of the <see cref="DpiHelper"/> class.
		/// </summary>
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
		/// Executes the logical Pixels To Device operation.
		/// </summary>
		/// <param name="logicalPoint">The logical Point.</param>
		/// <returns>The result of the operation.</returns>
		public static Point LogicalPixelsToDevice(Point logicalPoint) => _transformToDevice.Transform(logicalPoint);

		/// <summary>
		/// Executes the device Pixels To Logical operation.
		/// </summary>
		/// <param name="devicePoint">The device Point.</param>
		/// <returns>The result of the operation.</returns>
		public static Point DevicePixelsToLogical(Point devicePoint) => _transformToDip.Transform(devicePoint);

		/// <summary>
		/// Executes the logical Rect To Device operation.
		/// </summary>
		/// <param name="logicalRectangle">The logical Rectangle.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static Rect LogicalRectToDevice(Rect logicalRectangle)
		{
			var topLeft = LogicalPixelsToDevice(new Point(logicalRectangle.Left, logicalRectangle.Top));
			var bottomRight = LogicalPixelsToDevice(new Point(logicalRectangle.Right, logicalRectangle.Bottom));
			return new Rect(topLeft, bottomRight);
		}

		/// <summary>
		/// Executes the device Rect To Logical operation.
		/// </summary>
		/// <param name="deviceRectangle">The device Rectangle.</param>
		/// <returns>The result of the operation.</returns>
		public static Rect DeviceRectToLogical(Rect deviceRectangle)
		{
			var topLeft = DevicePixelsToLogical(new Point(deviceRectangle.Left, deviceRectangle.Top));
			var bottomRight = DevicePixelsToLogical(new Point(deviceRectangle.Right, deviceRectangle.Bottom));
			return new Rect(topLeft, bottomRight);
		}

		/// <summary>
		/// Executes the logical Size To Device operation.
		/// </summary>
		/// <param name="logicalSize">The logical Size.</param>
		/// <returns>The result of the operation.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static Size LogicalSizeToDevice(Size logicalSize)
		{
			var pt = LogicalPixelsToDevice(new Point(logicalSize.Width, logicalSize.Height));
			return new Size { Width = pt.X, Height = pt.Y };
		}

		/// <summary>
		/// Executes the device Size To Logical operation.
		/// </summary>
		/// <param name="deviceSize">The device Size.</param>
		/// <returns>The result of the operation.</returns>
		public static Size DeviceSizeToLogical(Size deviceSize)
		{
			var pt = DevicePixelsToLogical(new Point(deviceSize.Width, deviceSize.Height));
			return new Size(pt.X, pt.Y);
		}
	}
}