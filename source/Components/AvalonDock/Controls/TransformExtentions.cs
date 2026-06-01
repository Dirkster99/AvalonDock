using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the transform extensions.
	/// </summary>
	internal static class TransformExtensions
	{
		/// <summary>
		/// Point to screen dpi.
		/// </summary>
		/// <param name="visual">The visual.</param>
		/// <param name="pt">The pt.</param>
		/// <returns>The point to screen dpi.</returns>
		public static Point PointToScreenDPI(this Visual visual, Point pt)
		{
			if (PresentationSource.FromVisual(visual) == null)
			{
				return pt;
			}

			Point resultPt = visual.PointToScreen(pt);
			return TransformToDeviceDPI(visual, resultPt);
		}

		/// <summary>
		/// Point to screen dpi without flow direction.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="point">The point.</param>
		/// <returns>The point to screen dpi without flow direction.</returns>
		public static Point PointToScreenDPIWithoutFlowDirection(this FrameworkElement element, Point point)
		{
			if (FrameworkElement.GetFlowDirection(element) == FlowDirection.RightToLeft)
			{
				var actualSize = element.TransformActualSizeToAncestor();
				Point leftToRightPoint = new Point(
					actualSize.Width - point.X,
					point.Y);
				return element.PointToScreenDPI(leftToRightPoint);
			}

			return element.PointToScreenDPI(point);
		}

		/// <summary>
		/// Gets the screen area.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>The screen area.</returns>
		public static Rect GetScreenArea(this FrameworkElement element)
		{
			// return new Rect(element.PointToScreenDPI(new Point()),
			//        element.TransformActualSizeToAncestor());
			// }

			// public static Rect GetScreenAreaWithoutFlowDirection(this FrameworkElement element)
			// {
			var point = element.PointToScreenDPI(new Point());
			if (FrameworkElement.GetFlowDirection(element) == FlowDirection.RightToLeft)
			{
				var actualSize = element.TransformActualSizeToAncestor();
				Point leftToRightPoint = new Point(
					actualSize.Width - point.X,
					point.Y);
				return new Rect(
					leftToRightPoint,
					actualSize);
			}

			return new Rect(
				point,
				element.TransformActualSizeToAncestor());
		}

		/// <summary>
		/// Transform to device dpi.
		/// </summary>
		/// <param name="visual">The visual.</param>
		/// <param name="pt">The pt.</param>
		/// <returns>The transform to device dpi.</returns>
		public static Point TransformToDeviceDPI(this Visual visual, Point pt)
		{
			var compositionTarget = PresentationSource.FromVisual(visual)?.CompositionTarget ?? null;
			if (compositionTarget == null)
				return default;
			Matrix m = compositionTarget.TransformToDevice;
			return new Point(pt.X / m.M11, pt.Y / m.M22);
		}

		/// <summary>
		/// Transform from device dpi.
		/// </summary>
		/// <param name="visual">The visual.</param>
		/// <param name="size">The size.</param>
		/// <returns>The transform from device dpi.</returns>
		public static Size TransformFromDeviceDPI(this Visual visual, Size size)
		{
			var compositionTarget = PresentationSource.FromVisual(visual)?.CompositionTarget;
			if (compositionTarget == null)
			{
				return size;
			}

			Matrix m = compositionTarget.TransformToDevice;
			return new Size(size.Width * m.M11, size.Height * m.M22);
		}

		/// <summary>
		/// Transform from device dpi.
		/// </summary>
		/// <param name="visual">The visual.</param>
		/// <param name="pt">The pt.</param>
		/// <returns>The transform from device dpi.</returns>
		public static Point TransformFromDeviceDPI(this Visual visual, Point pt)
		{
			var compositionTarget = PresentationSource.FromVisual(visual)?.CompositionTarget;
			if (compositionTarget == null)
			{
				return pt;
			}

			Matrix m = compositionTarget.TransformToDevice;
			return new Point(pt.X * m.M11, pt.Y * m.M22);
		}

		/// <summary>
		/// Can transform.
		/// </summary>
		/// <param name="visual">The visual.</param>
		/// <returns>true if the instance can transform; otherwise, false.</returns>
		public static bool CanTransform(this Visual visual)
		{
			return PresentationSource.FromVisual(visual) != null;
		}

		/// <summary>
		/// Transform actual size to ancestor.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>The transform actual size to ancestor.</returns>
		public static Size TransformActualSizeToAncestor(this FrameworkElement element)
		{
			if (PresentationSource.FromVisual(element) == null)
				return new Size(element.ActualWidth, element.ActualHeight);

			var parentWindow = PresentationSource.FromVisual(element).RootVisual;
			var transformToWindow = element.TransformToAncestor(parentWindow);
			return transformToWindow.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight)).Size;
		}

		/// <summary>
		/// Transform size to ancestor.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="sizeToTransform">The size to transform.</param>
		/// <returns>The transform size to ancestor.</returns>
		public static Size TransformSizeToAncestor(this FrameworkElement element, Size sizeToTransform)
		{
			if (PresentationSource.FromVisual(element) == null)
				return sizeToTransform;

			var parentWindow = PresentationSource.FromVisual(element).RootVisual;
			var transformToWindow = element.TransformToAncestor(parentWindow);
			return transformToWindow.TransformBounds(new Rect(0, 0, sizeToTransform.Width, sizeToTransform.Height)).Size;
		}

		/// <summary>
		/// Tansform to ancestor.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>The tansform to ancestor.</returns>
		public static GeneralTransform TansformToAncestor(this FrameworkElement element)
		{
			if (PresentationSource.FromVisual(element) == null)
				return new MatrixTransform(Matrix.Identity);

			var parentWindow = PresentationSource.FromVisual(element).RootVisual;
			return element.TransformToAncestor(parentWindow);
		}
	}
}