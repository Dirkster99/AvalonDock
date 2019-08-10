namespace Xceed.Wpf.AvalonDock.Test
{
  using System;
  using System.Windows.Controls;

  using Microsoft.VisualStudio.TestTools.UnitTesting;

  using Xceed.Wpf.AvalonDock.Layout;

  [TestClass]
  public sealed class DockingUtilitiesTest
  {
    [TestMethod]
    public void CalculatedDockMinWidthHeightTest()
    {
      double defaultDockMinHeight = 25;
      double defaultDockMinWidth = 25;

      const double documentPaneDockMinHeight = 200;
      const double documentPaneDockMinWidth = 400;
      LayoutDocumentPane layoutDocumentPane = new LayoutDocumentPane { DockMinHeight = documentPaneDockMinHeight, DockMinWidth = documentPaneDockMinWidth };
      layoutDocumentPane.InsertChildAt( 0, new LayoutDocument { ContentId = "Document" } );

      LayoutDocumentPaneGroup layoutDocumentPaneGroup = new LayoutDocumentPaneGroup();
      layoutDocumentPaneGroup.InsertChildAt( 0, layoutDocumentPane );

      const double anchorablePaneDockMinHeight = 80;
      const double anchorablePaneDockMinWidth = 160;
      LayoutAnchorablePane layoutAnchorablePane = new LayoutAnchorablePane { DockMinHeight = anchorablePaneDockMinHeight, DockMinWidth = anchorablePaneDockMinWidth };
      layoutAnchorablePane.InsertChildAt( 0, new LayoutAnchorable { ContentId = "Anchorable" } );

      LayoutAnchorablePaneGroup layoutAnchorablePaneGroup = new LayoutAnchorablePaneGroup();
      layoutAnchorablePaneGroup.InsertChildAt( 0, layoutAnchorablePane );

      LayoutPanel layoutPanel = new LayoutPanel();
      layoutPanel.InsertChildAt( 0, layoutDocumentPaneGroup );
      layoutPanel.InsertChildAt( 1, layoutAnchorablePaneGroup );

      Assert.AreEqual( defaultDockMinWidth, layoutPanel.DockMinWidth );
      Assert.AreEqual( defaultDockMinHeight, layoutPanel.DockMinHeight );
      Assert.AreEqual( documentPaneDockMinWidth + anchorablePaneDockMinWidth, layoutPanel.CalculatedDockMinWidth() );
      Assert.AreEqual( Math.Max(documentPaneDockMinHeight, anchorablePaneDockMinHeight), layoutPanel.CalculatedDockMinHeight() );

      Assert.AreEqual( documentPaneDockMinWidth, layoutDocumentPane.DockMinWidth );
      Assert.AreEqual( documentPaneDockMinHeight, layoutDocumentPane.DockMinHeight );
      Assert.AreEqual( layoutDocumentPane.DockMinWidth, layoutDocumentPane.CalculatedDockMinWidth() );
      Assert.AreEqual( layoutDocumentPane.DockMinHeight, layoutDocumentPane.CalculatedDockMinHeight() );

      Assert.AreEqual( defaultDockMinWidth, layoutDocumentPaneGroup.DockMinWidth );
      Assert.AreEqual( defaultDockMinWidth, layoutDocumentPaneGroup.DockMinHeight );
      Assert.AreEqual( documentPaneDockMinWidth, layoutDocumentPaneGroup.CalculatedDockMinWidth() );
      Assert.AreEqual( documentPaneDockMinHeight, layoutDocumentPaneGroup.CalculatedDockMinHeight() );

      Assert.AreEqual( anchorablePaneDockMinWidth, layoutAnchorablePane.DockMinWidth );
      Assert.AreEqual( anchorablePaneDockMinHeight, layoutAnchorablePane.DockMinHeight );
      Assert.AreEqual( layoutAnchorablePane.DockMinWidth, layoutAnchorablePane.CalculatedDockMinWidth() );
      Assert.AreEqual( layoutAnchorablePane.DockMinHeight, layoutAnchorablePane.CalculatedDockMinHeight() );

      Assert.AreEqual( defaultDockMinWidth, layoutAnchorablePaneGroup.DockMinWidth );
      Assert.AreEqual( defaultDockMinWidth, layoutAnchorablePaneGroup.DockMinHeight );
      Assert.AreEqual( anchorablePaneDockMinWidth, layoutAnchorablePaneGroup.CalculatedDockMinWidth() );
      Assert.AreEqual( anchorablePaneDockMinHeight, layoutAnchorablePaneGroup.CalculatedDockMinHeight() );

      layoutPanel.RemoveChild( layoutDocumentPaneGroup );
      Assert.AreEqual( anchorablePaneDockMinWidth, layoutPanel.CalculatedDockMinWidth() );
      Assert.AreEqual( anchorablePaneDockMinHeight, layoutPanel.CalculatedDockMinHeight() );
    }

    [TestMethod]
    public void UpdateDocMinWidthHeightTest()
    {
      double documentPaneDockMinHeight = 100;
      double documentPaneDockMinWidth = 101;
      LayoutDocumentPane layoutDocumentPane = new LayoutDocumentPane { DockMinHeight = documentPaneDockMinHeight, DockMinWidth = documentPaneDockMinWidth };
      layoutDocumentPane.InsertChildAt( 0, new LayoutDocument { ContentId = "Document" } );

      LayoutDocumentPaneGroup layoutDocumentPaneGroup = new LayoutDocumentPaneGroup();
      layoutDocumentPaneGroup.InsertChildAt( 0, layoutDocumentPane );

      double anchorablePane1DockMinHeight = 150;
      double anchorablePane1DockMinWidth = 151;
      LayoutAnchorablePane layoutAnchorablePane1 = new LayoutAnchorablePane { DockMinHeight = anchorablePane1DockMinHeight, DockMinWidth = anchorablePane1DockMinWidth };
      layoutAnchorablePane1.InsertChildAt( 0, new LayoutAnchorable { ContentId = "Anchorable1" } );

      double anchorablePane2DockMinHeight = 200;
      double anchorablePane2DockMinWidth = 201;
      LayoutAnchorablePane layoutAnchorablePane2 = new LayoutAnchorablePane { DockMinHeight = anchorablePane2DockMinHeight, DockMinWidth = anchorablePane2DockMinWidth };
      layoutAnchorablePane2.InsertChildAt( 0, new LayoutAnchorable { ContentId = "Anchorable2" } );

      LayoutAnchorablePaneGroup layoutAnchorablePaneGroup = new LayoutAnchorablePaneGroup { Orientation = Orientation.Horizontal };
      layoutAnchorablePaneGroup.InsertChildAt( 0, layoutAnchorablePane1 );
      layoutAnchorablePaneGroup.InsertChildAt( 0, layoutAnchorablePane2 );

      LayoutPanel layoutPanel = new LayoutPanel { Orientation = Orientation.Vertical };
      layoutPanel.InsertChildAt( 0, layoutDocumentPaneGroup );
      layoutPanel.InsertChildAt( 1, layoutAnchorablePaneGroup );

      Assert.AreEqual( anchorablePane2DockMinWidth + anchorablePane1DockMinWidth, layoutAnchorablePaneGroup.CalculatedDockMinWidth() );
      Assert.AreEqual( Math.Max( anchorablePane2DockMinHeight, anchorablePane1DockMinHeight ), layoutAnchorablePaneGroup.CalculatedDockMinHeight() );

      Assert.AreEqual( documentPaneDockMinWidth, layoutDocumentPaneGroup.CalculatedDockMinWidth() );
      Assert.AreEqual( documentPaneDockMinHeight, layoutDocumentPaneGroup.CalculatedDockMinHeight() );

      Assert.AreEqual(
          Math.Max( anchorablePane1DockMinWidth + anchorablePane2DockMinWidth, documentPaneDockMinWidth ),
          layoutPanel.CalculatedDockMinWidth() );

      Assert.AreEqual( documentPaneDockMinHeight + anchorablePane2DockMinHeight, layoutPanel.CalculatedDockMinHeight() );
    }
  }
}
