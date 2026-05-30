/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;
using System.Linq;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	// Shared overlay rules that do not depend on WPF visual APIs.
	// This file is intended to be reused by platform-specific overlay implementations.
	internal static class OverlayDropRules
	{
		internal static bool ShouldShowDropTargetInto(ILayoutPositionableElement positionableElement, ILayoutElement floatingModel)
		{
			if (positionableElement == null || floatingModel == null || positionableElement.AllowDuplicateContent)
			{
				return true;
			}

			var contentLayoutsOnPositionableElementPane = GetAllLayoutContents(positionableElement);
			var contentLayoutsOnFloatingWindow = GetAllLayoutContents(floatingModel);

			foreach (var content in contentLayoutsOnFloatingWindow)
			{
				if (contentLayoutsOnPositionableElementPane.Any(item =>
					item.Title == content.Title &&
					item.ContentId == content.ContentId))
				{
					return false;
				}
			}

			return true;
		}

		private static List<LayoutContent> GetAllLayoutContents(object source)
		{
			var result = new List<LayoutContent>();
			if (source == null)
			{
				return result;
			}

			if (source is LayoutDocumentFloatingWindow documentFloatingWindow)
			{
				foreach (var layoutElement in documentFloatingWindow.Children)
				{
					result.AddRange(GetAllLayoutContents(layoutElement));
				}
			}

			if (source is LayoutAnchorableFloatingWindow anchorableFloatingWindow)
			{
				foreach (var layoutElement in anchorableFloatingWindow.Children)
				{
					result.AddRange(GetAllLayoutContents(layoutElement));
				}
			}

			if (source is LayoutDocumentPaneGroup documentPaneGroup)
			{
				foreach (var layoutDocumentPane in documentPaneGroup.Children)
				{
					result.AddRange(GetAllLayoutContents(layoutDocumentPane));
				}
			}

			if (source is LayoutAnchorablePaneGroup anchorablePaneGroup)
			{
				foreach (var layoutDocumentPane in anchorablePaneGroup.Children)
				{
					result.AddRange(GetAllLayoutContents(layoutDocumentPane));
				}
			}

			if (source is LayoutDocumentPane documentPane)
			{
				foreach (var layoutContent in documentPane.Children)
				{
					result.Add(layoutContent);
				}
			}

			if (source is LayoutAnchorablePane anchorablePane)
			{
				foreach (var layoutContent in anchorablePane.Children)
				{
					result.Add(layoutContent);
				}
			}

			if (source is LayoutDocument document)
			{
				result.Add(document);
			}

			if (source is LayoutAnchorable anchorable)
			{
				result.Add(anchorable);
			}

			return result;
		}
	}
}
