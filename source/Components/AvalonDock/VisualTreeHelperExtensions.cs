/************************************************************************
   AvalonDock

   Copyright (C) 2020 ????

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock
{
    internal static class VisualTreeHelperExtensions
    {
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
    }
}
