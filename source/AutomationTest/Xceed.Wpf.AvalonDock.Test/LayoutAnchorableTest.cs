using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Converters;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Test
{
  [TestClass]
  public sealed class LayoutAnchorableTest
  {
    [TestMethod]
    public void ClearBindingOfHiddenWindowTest()
    {
      LayoutAnchorable layoutAnchorable = new LayoutAnchorable
      {
        FloatingWidth = 50,
        FloatingHeight = 100,
        ContentId = "Test"
      };

      LayoutAnchorablePane layoutAnchorablePane = new LayoutAnchorablePane( layoutAnchorable );
      LayoutAnchorablePaneGroup layoutAnchorablePaneGroup = new LayoutAnchorablePaneGroup( layoutAnchorablePane );
      LayoutAnchorableFloatingWindow layoutFloatingWindow = new LayoutAnchorableFloatingWindow
      {
        RootPanel = layoutAnchorablePaneGroup
      };

      var ctor = typeof( LayoutAnchorableFloatingWindowControl )
        .GetTypeInfo()
        .GetConstructors( BindingFlags.NonPublic | BindingFlags.Instance )
        .First( x => x.GetParameters().Length == 1 );

      LayoutAnchorableFloatingWindowControl floatingWindowControl = ctor.Invoke( new object[] {layoutFloatingWindow} ) as LayoutAnchorableFloatingWindowControl;
      floatingWindowControl.SetBinding(
        UIElement.VisibilityProperty,
        new Binding( "IsVisible" )
        {
          Source = floatingWindowControl.Model,
          Converter = new BoolToVisibilityConverter(),
          Mode = BindingMode.OneWay,
          ConverterParameter = Visibility.Hidden
        } );

      BindingExpression visibilityBinding = floatingWindowControl.GetBindingExpression( UIElement.VisibilityProperty );
      Assert.IsNotNull( visibilityBinding );

      layoutAnchorable.Show();
      layoutAnchorable.Hide();
      
      visibilityBinding = floatingWindowControl.GetBindingExpression( UIElement.VisibilityProperty );
      Assert.IsNotNull( visibilityBinding );

      floatingWindowControl.Hide();

      visibilityBinding = floatingWindowControl.GetBindingExpression( UIElement.VisibilityProperty );
      Assert.IsNull( visibilityBinding );
    }
  }
}
