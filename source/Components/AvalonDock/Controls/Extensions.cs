using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Provides helper members for extensions.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Executes the find Visual Children operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="depObj">The dep Obj.</param>
		/// <returns>The result of the operation.</returns>
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj)
			where T : DependencyObject
		{
			if (depObj == null) yield break;
			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i);
				if (child is T t)
					yield return t;
				foreach (var childOfChild in FindVisualChildren<T>(child))
					yield return childOfChild;
			}
		}

		/// <summary>
		/// Executes the find Logical Children operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="depObj">The dep Obj.</param>
		/// <returns>The result of the operation.</returns>
		public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject depObj)
			where T : DependencyObject
		{
			if (depObj == null) yield break;
			foreach (var child in LogicalTreeHelper.GetChildren(depObj).OfType<DependencyObject>())
			{
				if (child is T t)
					yield return t;
				foreach (var childOfChild in FindLogicalChildren<T>(child))
					yield return childOfChild;
			}
		}

		/// <summary>
		/// Executes the find Visual Tree Root operation.
		/// </summary>
		/// <param name="initial">The initial.</param>
		/// <returns>The result of the operation.</returns>
		public static DependencyObject FindVisualTreeRoot(this DependencyObject initial)
		{
			var current = initial;
			var result = initial;
			while (current != null)
			{
				result = current;
				if (current is Visual || current is Visual3D)
				{
					current = VisualTreeHelper.GetParent(current);
				}
				else
				{
					// If we're in Logical Land then we must walk
					// up the logical tree until we find a
					// Visual/Visual3D to get us back to Visual Land.
					current = LogicalTreeHelper.GetParent(current);
				}
			}

			return result;
		}

		/// <summary>
		/// Executes the find Visual Ancestor operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="dependencyObject">The dependency Object.</param>
		/// <returns>The result of the operation.</returns>
		public static T FindVisualAncestor<T>(this DependencyObject dependencyObject)
			where T : class
		{
			var target = dependencyObject;
			do
			{
				target = VisualTreeHelper.GetParent(target);
			}
			while (target != null && !(target is T));
			return target as T;
		}

		/// <summary>
		/// Executes the find Logical Ancestor operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="dependencyObject">The dependency Object.</param>
		/// <returns>The result of the operation.</returns>
		public static T FindLogicalAncestor<T>(this DependencyObject dependencyObject)
			where T : class
		{
			var target = dependencyObject;
			do
			{
				var current = target;
				target = LogicalTreeHelper.GetParent(target) ?? VisualTreeHelper.GetParent(current);
			}
			while (target != null && !(target is T));
			return target as T;
		}
	}
}