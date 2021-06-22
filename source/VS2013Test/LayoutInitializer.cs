using System.Linq;
using AvalonDock.Layout;
using AvalonDock.VS2013Test.ViewModels;

namespace AvalonDock.VS2013Test
{
	class LayoutInitializer : ILayoutUpdateStrategy
	{
		public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
		{
			//AD wants to add the anchorable into destinationContainer
			//just for test provide a new anchorablepane 
			//if the pane is floating let the manager go ahead
			LayoutAnchorablePane destPane = destinationContainer as LayoutAnchorablePane;

			if (destinationContainer != null && destinationContainer.FindParent<LayoutFloatingWindow>() != null)
				return false;

			if (anchorableToShow.Content is ExplorerViewModel)
			{
				anchorableToShow.AutoHideWidth = 256;

				var treeViewPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ExplorerPane");

				if (treeViewPane != null)
				{
					treeViewPane.Children.Add(anchorableToShow);
					return true;
				}
			}

			if (anchorableToShow.Content is PropertiesViewModel)
			{
				var propertiesPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "PropertiesPane");
				if (propertiesPane != null)
				{
					propertiesPane.Children.Add(anchorableToShow);
					return true;
				}
			}

			if (anchorableToShow.Content is FileStatsViewModel)
			{
				anchorableToShow.AutoHideWidth = 256;

				var controllerPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "FileStatsPane");

				if (controllerPane != null)
				{
					controllerPane.Children.Add(anchorableToShow);
					return true;
				}
			}

			if (anchorableToShow.Content is ErrorViewModel)
			{
				anchorableToShow.AutoHideHeight = 128;

				var scriptEditorPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ErrorPane");

				if (scriptEditorPane != null)
				{
					scriptEditorPane.Children.Add(anchorableToShow);
					return true;
				}
			}

			if (anchorableToShow.Content is OutputViewModel)
			{
				anchorableToShow.AutoHideHeight = 128;

				var outputPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "OutputPane");

				if (outputPane != null)
				{
					outputPane.Children.Add(anchorableToShow);
					return true;
				}
			}

			return false;
		}

		public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
		{
		}

		public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
		{
			return false;
		}

		public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
		{
		}
	}
}
