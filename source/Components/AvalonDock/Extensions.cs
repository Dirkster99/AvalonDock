/************************************************************************
   AvalonDock

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock
{
	internal static class Extensions
	{
		public static bool Contains(this IEnumerable collection, object item)
		{
			foreach (var o in collection)
				if (o == item) return true;
			return false;
		}


		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (var v in collection) action(v);
		}


		public static int IndexOf<T>(this T[] array, T value) where T : class
		{
			for (var i = 0; i < array.Length; i++)
				if (array[i] == value) return i;
			return -1;
		}

		public static V GetValueOrDefault<V>(this WeakReference wr)
		{
			return wr == null || !wr.IsAlive ? default : (V) wr.Target;
		}

        public static IEnumerable<DependencyObject> GetChildrenRecursive(this DependencyObject dependencyObject)
        {
            var children = dependencyObject.GetChildren();
            foreach (var child in children)
            {
                yield return child;
                foreach (var c in GetChildrenRecursive(child))
                {
                    yield return c;
                }
            }
        }

        public static IEnumerable<DependencyObject> GetChildren(this DependencyObject dependencyObject)
        {
            int n = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (int i = 0; i < n; i++)
            {
                yield return VisualTreeHelper.GetChild(dependencyObject, i);
            }
        }

        public static IEnumerable<DependencyObject> GetParents(this DependencyObject dependencyObject)
        {
            while (dependencyObject != null)
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                if (dependencyObject != null)
                    yield return dependencyObject;
            }
        }

        public static Thickness Sum<T>(this IEnumerable<T> enumerable, Func<T, Thickness> func)
        {
            double top = 0, bottom = 0, left = 0, right = 0;
            foreach (var e in enumerable)
            {
                var t = func(e);
                left = t.Left;
                top += t.Top;
                right = t.Right;
                bottom += t.Bottom;
            }
            return new Thickness(left, top, right, bottom);
        }

        public static Thickness Add(this Thickness thickness, Thickness other)
        {
            return new Thickness(thickness.Left + other.Left,
                thickness.Top + other.Top,
                thickness.Right + other.Right,
                thickness.Bottom + other.Bottom);
        }
    }
}
