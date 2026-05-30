/************************************************************************
   AvalonDock — WPF-only partial for LayoutGridControl<T>.
   Provides the ghost-resizer overlay window shown during splitter drags.
   Compiled only when HAS_UNO is NOT defined.
 ************************************************************************/

#if !HAS_UNO

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	public abstract partial class LayoutGridControl<T>
		where T : class, ILayoutPanelElement
	{
		private Border _resizerGhost;
		private Window _resizerWindowHost;
		private Vector _initialStartPoint;

		// Called from the shared file's OnSplitterDragDelta stub.
		private void OnSplitterDragDeltaWpf(System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			var rootVisual = this.FindVisualTreeRoot() as Visual;
			var tr = TransformToAncestor(rootVisual);
			var delta = tr.Transform(new Point(e.HorizontalChange, e.VerticalChange)) - tr.Transform(new Point());
			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
				Canvas.SetLeft(_resizerGhost, MathHelper.MinMax(_initialStartPoint.X + delta.X, 0.0, _resizerWindowHost.Width - _resizerGhost.Width));
			else
				Canvas.SetTop(_resizerGhost, MathHelper.MinMax(_initialStartPoint.Y + delta.Y, 0.0, _resizerWindowHost.Height - _resizerGhost.Height));
		}

		// Called from the shared file's OnSplitterDragCompleted stub.
		private void OnSplitterDragCompletedWpf(LayoutGridResizerControl splitter, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			double delta = Orientation == System.Windows.Controls.Orientation.Horizontal
				? Canvas.GetLeft(_resizerGhost) - _initialStartPoint.X
				: Canvas.GetTop(_resizerGhost)  - _initialStartPoint.Y;
			var (prev, next) = GetResizerNeighbours(splitter);
			ApplyResizeDelta(splitter, delta, prev.TransformActualSizeToAncestor(), next.TransformActualSizeToAncestor());
			HideResizerOverlayWindow();
		}

		private void ShowResizerOverlayWindow(LayoutGridResizerControl splitter)
		{
			_resizerGhost = new Border { Background = splitter.BackgroundWhileDragging, Opacity = splitter.OpacityWhileDragging };
			var idx   = Children.IndexOf(splitter);
			var prev  = Children[idx - 1] as FrameworkElement;
			var next  = GetNextVisibleChild(idx);
			var prevSz = prev.TransformActualSizeToAncestor();
			var nextSz = next.TransformActualSizeToAncestor();
			var prevM  = (ILayoutPositionableElement)(prev as ILayoutControl).Model;
			var nextM  = (ILayoutPositionableElement)(next as ILayoutControl).Model;
			var ptTopLeft = prev.PointToScreenDPIWithoutFlowDirection(new Point());
			Size actualSize;
			if (Orientation == System.Windows.Controls.Orientation.Horizontal)
			{
				actualSize = new Size(prevSz.Width - prevM.CalculatedDockMinWidth() + splitter.ActualWidth + nextSz.Width - nextM.CalculatedDockMinWidth(), nextSz.Height);
				_resizerGhost.Width  = splitter.ActualWidth; _resizerGhost.Height = actualSize.Height;
				ptTopLeft.Offset(prevM.CalculatedDockMinWidth(), 0);
			}
			else
			{
				actualSize = new Size(prevSz.Width, prevSz.Height - prevM.CalculatedDockMinHeight() + splitter.ActualHeight + nextSz.Height - nextM.CalculatedDockMinHeight());
				_resizerGhost.Height = splitter.ActualHeight; _resizerGhost.Width = actualSize.Width;
				ptTopLeft.Offset(0, prevM.CalculatedDockMinHeight());
			}
			_initialStartPoint = splitter.PointToScreenDPIWithoutFlowDirection(new Point()) - ptTopLeft;
			if (Orientation == System.Windows.Controls.Orientation.Horizontal) Canvas.SetLeft(_resizerGhost, _initialStartPoint.X);
			else                                                                Canvas.SetTop(_resizerGhost,  _initialStartPoint.Y);
			var canvas = new Canvas { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
			canvas.Children.Add(_resizerGhost);
			_resizerWindowHost = new Window
			{
				Style = new Style(typeof(Window), null), SizeToContent = SizeToContent.Manual, ResizeMode = ResizeMode.NoResize,
				WindowStyle = WindowStyle.None, ShowInTaskbar = false, AllowsTransparency = true, Background = null,
				Width = actualSize.Width, Height = actualSize.Height, Left = ptTopLeft.X, Top = ptTopLeft.Y,
				ShowActivated = false, Owner = null, Content = canvas,
			};
			_resizerWindowHost.Show();
		}

		private void HideResizerOverlayWindow()
		{
			if (_resizerWindowHost == null) return;
			_resizerWindowHost.Close(); _resizerWindowHost = null;
		}
	}
}
#endif
