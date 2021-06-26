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
			anchorableToShow.AutoHideWidth = 256;
			anchorableToShow.AutoHideHeight = 128;

			if (destinationContainer != null && destinationContainer.FindParent<LayoutFloatingWindow>() != null)
				return false;

			if (anchorableToShow.Content is ExplorerViewModel)
			{
				var explorerPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ExplorerPane");

				if (explorerPane != null)
				{
					explorerPane.Children.Add(anchorableToShow);
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

			if (anchorableToShow.Content is OutputViewModel)
			{
				var outputPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "OutputPane");

				if (outputPane != null)
				{
					outputPane.Children.Add(anchorableToShow);
					return true;
				}
			}

			if (anchorableToShow.Content is ToolboxViewModel)
			{
				var leftGroup = new LayoutAnchorGroup();
				leftGroup.Children.Add(anchorableToShow);
				layout.LeftSide.Children.Add(leftGroup);
				return true;
			}

			if (anchorableToShow.Content is GitChangesViewModel)
			{
				var rightGroup = new LayoutAnchorGroup();
				rightGroup.Children.Add(anchorableToShow);
				layout.RightSide.Children.Add(rightGroup);
				return true;
			}

			if (anchorableToShow.Content is ErrorViewModel)
			{
				//var errorsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ErrorsPane");

				//if (errorsPane != null)
				//{
				//	errorsPane.Children.Add(anchorableToShow);
				//	anchorableToShow.Hide(false);
				//	return true;
				//}

				var bottomGroup = new LayoutAnchorGroup();
				bottomGroup.Children.Add(anchorableToShow);
				layout.BottomSide.Children.Add(bottomGroup);
				return true;
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
