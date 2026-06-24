namespace AvalonDock.Layout
{
	using System.Runtime.InteropServices;
	using System.Windows;

	/// <summary>
	/// Represents a layout element for floating window extension.
	/// </summary>
	public static class ILayoutElementForFloatingWindowExtension
	{
		/// <summary>
		/// Represents a Win32 rectangle.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT
		{
			/// <summary>
			/// Gets or sets the left edge.
			/// </summary>
			public int Left;

			/// <summary>
			/// Gets or sets the top edge.
			/// </summary>
			public int Top;

			/// <summary>
			/// Gets or sets the right edge.
			/// </summary>
			public int Right;

			/// <summary>
			/// Gets or sets the bottom edge.
			/// </summary>
			public int Bottom;

			/// <summary>
			/// Initializes a new instance of the <see cref="RECT"/> struct.
			/// </summary>
			/// <param name="left">The left.</param>
			/// <param name="top">The top.</param>
			/// <param name="right">The right.</param>
			/// <param name="bottom">The bottom.</param>
			public RECT(int left, int top, int right, int bottom)
			{
				this.Left = left;
				this.Top = top;
				this.Right = right;
				this.Bottom = bottom;
			}
		}

		/// <summary>
		/// Executes the keep inside nearest monitor operation.
		/// </summary>
		/// <param name="paneInsideFloatingWindow">The pane inside floating window.</param>
		internal static void KeepInsideNearestMonitor(this ILayoutElementForFloatingWindow paneInsideFloatingWindow)
		{
			RECT normalPosition = new RECT();
			normalPosition.Left = (int)paneInsideFloatingWindow.FloatingLeft;
			normalPosition.Top = (int)paneInsideFloatingWindow.FloatingTop;
			normalPosition.Bottom = normalPosition.Top + (int)paneInsideFloatingWindow.FloatingHeight;
			normalPosition.Right = normalPosition.Left + (int)paneInsideFloatingWindow.FloatingWidth;

			// Are we using only one monitor?
			if (SystemParameters.PrimaryScreenWidth == SystemParameters.VirtualScreenWidth &&
				SystemParameters.PrimaryScreenHeight == SystemParameters.VirtualScreenHeight)
			{
				RECT primaryscreen = new RECT((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop,
											  (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

				if (!RectanglesIntersect(normalPosition, primaryscreen))
				{
					normalPosition = PlaceOnScreen(primaryscreen, normalPosition);

					paneInsideFloatingWindow.FloatingLeft = normalPosition.Left;
					paneInsideFloatingWindow.FloatingTop = normalPosition.Top;
					paneInsideFloatingWindow.FloatingHeight = normalPosition.Bottom - normalPosition.Top;
					paneInsideFloatingWindow.FloatingWidth = normalPosition.Right - normalPosition.Left;

					paneInsideFloatingWindow.RaiseFloatingPropertiesUpdated();
				}

				return;
			}
			else
			{
				RECT primaryscreen = new RECT((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop,
											  (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);

				if (!RectanglesIntersect(normalPosition, primaryscreen))
				{
					normalPosition = PlaceOnScreen(primaryscreen, normalPosition);

					paneInsideFloatingWindow.FloatingLeft = normalPosition.Left;
					paneInsideFloatingWindow.FloatingTop = normalPosition.Top;
					paneInsideFloatingWindow.FloatingHeight = normalPosition.Bottom - normalPosition.Top;
					paneInsideFloatingWindow.FloatingWidth = normalPosition.Right - normalPosition.Left;

					paneInsideFloatingWindow.RaiseFloatingPropertiesUpdated();
				}

				return;
			}
		}

		/// <summary>
		/// Executes the rectangles intersect operation.
		/// </summary>
		/// <param name="a">The a.</param>
		/// <param name="b">The b.</param>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		private static bool RectanglesIntersect(RECT a, RECT b)
		{
			if (a.Left > b.Right || a.Right < b.Left)
			{
				return false;
			}

			if (a.Top > b.Bottom || a.Bottom < b.Top)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Executes the place on screen operation.
		/// </summary>
		/// <param name="monitorRect">The monitor rect.</param>
		/// <param name="windowRect">The window rect.</param>
		/// <returns>The resulting value.</returns>
		private static RECT PlaceOnScreen(RECT monitorRect, RECT windowRect)
		{
			int monitorWidth = monitorRect.Right - monitorRect.Left;
			int monitorHeight = monitorRect.Bottom - monitorRect.Top;

			if (windowRect.Right < monitorRect.Left)
			{
				// Off left side
				int width = windowRect.Right - windowRect.Left;
				if (width > monitorWidth)
				{
					width = monitorWidth;
				}

				windowRect.Left = monitorRect.Left;
				windowRect.Right = windowRect.Left + width;
			}
			else if (windowRect.Left > monitorRect.Right)
			{
				// Off right side
				int width = windowRect.Right - windowRect.Left;
				if (width > monitorWidth)
				{
					width = monitorWidth;
				}

				windowRect.Right = monitorRect.Right;
				windowRect.Left = windowRect.Right - width;
			}

			if (windowRect.Bottom < monitorRect.Top)
			{
				// Off top
				int height = windowRect.Bottom - windowRect.Top;
				if (height > monitorHeight)
				{
					height = monitorHeight;
				}

				windowRect.Top = monitorRect.Top;
				windowRect.Bottom = windowRect.Top + height;
			}
			else if (windowRect.Top > monitorRect.Bottom)
			{
				// Off bottom
				int height = windowRect.Bottom - windowRect.Top;
				if (height > monitorHeight)
				{
					height = monitorHeight;
				}

				windowRect.Bottom = monitorRect.Bottom;
				windowRect.Top = windowRect.Bottom - height;
			}

			return windowRect;
		}
	}
}