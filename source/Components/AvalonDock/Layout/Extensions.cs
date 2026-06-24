using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a extensions.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Executes the descendents operation.
		/// </summary>
		/// <param name="element">The layout element.</param>
		/// <returns>The resulting value.</returns>
		public static IEnumerable<ILayoutElement> Descendents(this ILayoutElement element)
		{
			if (!(element is ILayoutContainer container)) yield break;
			foreach (var childElement in container.Children)
			{
				yield return childElement;
				foreach (var childChildElement in childElement.Descendents())
					yield return childChildElement;
			}
		}

		/// <summary>
		/// Finds the parent.
		/// </summary>
		/// <typeparam name="T">The type of the related value.</typeparam>
		/// <param name="element">The layout element.</param>
		/// <returns>The resulting value.</returns>
		public static T FindParent<T>(this ILayoutElement element) // where T : ILayoutContainer
		{
			var parent = element.Parent;
			while (parent != null && !(parent is T))
				parent = parent.Parent;
			return (T)parent;
		}

		/// <summary>
		/// Gets the root.
		/// </summary>
		/// <param name="element">The layout element.</param>
		/// <returns>The resulting value.</returns>
		public static ILayoutRoot GetRoot(this ILayoutElement element) // where T : ILayoutContainer
		{
			if (element is ILayoutRoot layoutRoot) return layoutRoot;
			var parent = element.Parent;
			while (parent != null && !(parent is ILayoutRoot))
				parent = parent.Parent;
			return (ILayoutRoot)parent;
		}

		/// <summary>
		/// Executes the contains child of type operation.
		/// </summary>
		/// <typeparam name="T">The type of the related value.</typeparam>
		/// <param name="element">The layout element.</param>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		public static bool ContainsChildOfType<T>(this ILayoutContainer element)
		{
			foreach (var childElement in element.Descendents())
				if (childElement is T) return true;
			return false;
		}

		/// <summary>
		/// Determines whether the container contains a child of either specified type.
		/// </summary>
		/// <typeparam name="T">The first child type to look for.</typeparam>
		/// <typeparam name="S">The second child type to look for.</typeparam>
		/// <param name="container">The container.</param>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		public static bool ContainsChildOfType<T, S>(this ILayoutContainer container)
		{
			foreach (var childElement in container.Descendents())
				if (childElement is T || childElement is S) return true;
			return false;
		}

		/// <summary>
		/// Determines whether the container matches either specified type.
		/// </summary>
		/// <typeparam name="T">The first type to compare against.</typeparam>
		/// <typeparam name="S">The second type to compare against.</typeparam>
		/// <param name="container">The container.</param>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		public static bool IsOfType<T, S>(this ILayoutContainer container) => container is T || container is S;

		/// <summary>
		/// Gets the side.
		/// </summary>
		/// <param name="element">The layout element.</param>
		/// <returns>The resulting value.</returns>
		public static AnchorSide GetSide(this ILayoutElement element)
		{
			if (element.Parent is ILayoutOrientableGroup parentContainer)
			{
				var layoutPanel = parentContainer as LayoutPanel ?? parentContainer.FindParent<LayoutPanel>();
				if (layoutPanel != null && layoutPanel.Children.Count > 0)
				{
					if (layoutPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
						return element.IsInAnchorablePaneAtStartOfPanel(layoutPanel) ? AnchorSide.Left : AnchorSide.Right;
					return element.IsInAnchorablePaneAtStartOfPanel(layoutPanel) ? AnchorSide.Top : AnchorSide.Bottom;
				}
			}

			Debug.Fail("Unable to find the side for an element, possible layout problem!");
			return AnchorSide.Right;
		}

		/// <summary>
		/// Executes the keep inside nearest monitor issue20 operation.
		/// </summary>
		/// <param name="paneInsideFloatingWindow">The pane inside floating window.</param>
		internal static void KeepInsideNearestMonitor_Issue20(this ILayoutElementForFloatingWindow paneInsideFloatingWindow)
		{
			var r = new Win32Helper.RECT { Left = (int)paneInsideFloatingWindow.FloatingLeft, Top = (int)paneInsideFloatingWindow.FloatingTop };
			r.Bottom = r.Top + (int)paneInsideFloatingWindow.FloatingHeight;
			r.Right = r.Left + (int)paneInsideFloatingWindow.FloatingWidth;

			uint MONITOR_DEFAULTTONEAREST = 0x00000002;
			uint MONITOR_DEFAULTTONULL = 0x00000000;

			var monitor = Win32Helper.MonitorFromRect(ref r, MONITOR_DEFAULTTONULL);
			if (monitor != System.IntPtr.Zero) return;
			var nearestMonitor = Win32Helper.MonitorFromRect(ref r, MONITOR_DEFAULTTONEAREST);
			if (nearestMonitor == System.IntPtr.Zero) return;
			var monitorInfo = new Win32Helper.MonitorInfo();
			monitorInfo.Size = Marshal.SizeOf(monitorInfo);
			Win32Helper.GetMonitorInfo(nearestMonitor, monitorInfo);

			if (paneInsideFloatingWindow.FloatingLeft < monitorInfo.Work.Left)
				paneInsideFloatingWindow.FloatingLeft = monitorInfo.Work.Left + 10;
			if (paneInsideFloatingWindow.FloatingLeft + paneInsideFloatingWindow.FloatingWidth > monitorInfo.Work.Right)
				paneInsideFloatingWindow.FloatingLeft = monitorInfo.Work.Right - (paneInsideFloatingWindow.FloatingWidth + 10);
			if (paneInsideFloatingWindow.FloatingTop < monitorInfo.Work.Top)
				paneInsideFloatingWindow.FloatingTop = monitorInfo.Work.Top + 10;
			if (paneInsideFloatingWindow.FloatingTop + paneInsideFloatingWindow.FloatingHeight > monitorInfo.Work.Bottom)
				paneInsideFloatingWindow.FloatingTop = monitorInfo.Work.Bottom - (paneInsideFloatingWindow.FloatingHeight + 10);
		}

		/// <summary>
		/// Executes the is in anchorable pane at start of panel operation.
		/// </summary>
		/// <param name="element">The layout element.</param>
		/// <param name="layoutPanel">The layout panel.</param>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		private static bool IsInAnchorablePaneAtStartOfPanel(this ILayoutElement element, LayoutPanel layoutPanel)
		{
			foreach (var child in layoutPanel.Children)
			{
				if (!(child is LayoutAnchorablePane || child is LayoutAnchorablePaneGroup))
				{
					return false;
				}

				if (child.Equals(element) || child.Descendents().Contains(element))
				{
					return true;
				}
			}

			return false;
		}
	}
}