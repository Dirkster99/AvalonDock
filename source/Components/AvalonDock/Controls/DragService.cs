using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the drag Service.
	/// </summary>
	internal class DragService
	{
		private DockingManager _manager;
		private LayoutFloatingWindowControl _floatingWindow;

		// A list of hosts that can display an overlaywindow and offer a drop target (docking position)
		private List<IOverlayWindowHost> _overlayWindowHosts = new List<IOverlayWindowHost>();

		private IOverlayWindowHost _currentHost;
		private IOverlayWindow _currentWindow;
		private List<IDropArea> _currentWindowAreas = new List<IDropArea>();
		private IDropTarget _currentDropTarget;
		private bool _isDrag;

		/// <summary>
		/// Initializes a new instance of the <see cref="DragService"/> class.
		/// </summary>
		/// <param name="floatingWindow">The floating Window.</param>
		public DragService(LayoutFloatingWindowControl floatingWindow)
		{
			_floatingWindow = floatingWindow;
			_manager = floatingWindow.Model.Root.Manager;
		}

		/// <summary>
		/// Executes the update Mouse Location operation.
		/// </summary>
		/// <param name="dragPosition">The drag Position.</param>
		internal void UpdateMouseLocation(Point dragPosition)
		{
			////var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;
			// TODO - pass in without DPI adjustment, screen co-ords, adjust inside the target window
			if (!_isDrag)
			{
				GetOverlayWindowHosts();
				_isDrag = true;
			}

			var newHost = _overlayWindowHosts.FirstOrDefault(oh => oh.HitTestScreen(dragPosition));

			if (_currentHost != null || _currentHost != newHost)
			{
				// is mouse still inside current overlay window host?
				if ((_currentHost != null && !_currentHost.HitTestScreen(dragPosition)) ||
					_currentHost != newHost)
				{
					// esit drop target
					if (_currentDropTarget != null)
						_currentWindow.DragLeave(_currentDropTarget);

					_currentDropTarget = null;

					// exit area
					_currentWindowAreas.ForEach(a =>
						_currentWindow.DragLeave(a));
					_currentWindowAreas.Clear();

					// hide current overlay window
					if (_currentWindow != null)
						_currentWindow.DragLeave(_floatingWindow);
					if (_currentHost != null)
					{
						_currentHost.HideOverlayWindow();
						GetOverlayWindowHosts();
					}

					_currentHost = null;
				}

				if (_currentHost != newHost)
				{
					_currentHost = newHost;
					_currentWindow = _currentHost.ShowOverlayWindow(_floatingWindow);
					_currentWindow.DragEnter(_floatingWindow);

					// Set the target window to topmost
					if (_currentHost is LayoutFloatingWindowControl fwc &&
						(fwc.OwnedByDockingManagerWindow == _floatingWindow.OwnedByDockingManagerWindow || fwc.OwnedByDockingManagerWindow))
					{
						BringWindowToTop2(fwc);
					}
					else if (_currentHost is DockingManager dockingManager)
					{
						BringWindowToTop2(Window.GetWindow(dockingManager));
					}

					GetOverlayWindowHosts();

					BringWindowToTop2(_floatingWindow);
					if (_currentWindow is Window overlayWindow)
					{
						BringWindowToTop2(overlayWindow);
					}
				}
			}

			if (_currentHost == null)
				return;

			if (_currentDropTarget != null &&
				!_currentDropTarget.HitTestScreen(dragPosition))
			{
				_currentWindow.DragLeave(_currentDropTarget);
				_currentDropTarget = null;
			}

			List<IDropArea> areasToRemove = new List<IDropArea>();
			_currentWindowAreas.ForEach(a =>
			{
				// is mouse still inside this area?
				if (!a.DetectionRect.Contains(a.TransformToDeviceDPI(dragPosition)))
				{
					_currentWindow.DragLeave(a);
					areasToRemove.Add(a);
				}
			});

			areasToRemove.ForEach(a =>
				_currentWindowAreas.Remove(a));

			var areasToAdd =
				_currentHost.GetDropAreas(_floatingWindow).Where(cw => !_currentWindowAreas.Contains(cw) && cw.DetectionRect.Contains(cw.TransformToDeviceDPI(dragPosition))).ToList();

			_currentWindowAreas.AddRange(areasToAdd);

			areasToAdd.ForEach(a =>
				_currentWindow.DragEnter(a));

			if (_currentDropTarget == null)
			{
				_currentWindowAreas.ForEach(wa =>
				{
					if (_currentDropTarget != null)
						return;

					_currentDropTarget = _currentWindow.GetTargets().FirstOrDefault(dt => dt.HitTestScreen(dragPosition));

					if (_currentDropTarget != null)
					{
						_currentWindow.DragEnter(_currentDropTarget);
						BringWindowToTop2((Window)_currentWindow);
						return;
					}
				});
			}
		}

		/// <summary>
		/// Executes the drop operation.
		/// </summary>
		/// <param name="dropLocation">The drop Location.</param>
		/// <param name="dropHandled">The drop Handled.</param>
		internal void Drop(Point dropLocation, out bool dropHandled)
		{
			// TODO - pass in without DPI adjustment, screen co-ords, adjust inside the target window
			dropHandled = false;

			UpdateMouseLocation(dropLocation);

			var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;
			var root = floatingWindowModel.Root;

			if (_currentHost != null)
				_currentHost.HideOverlayWindow();

			if (_currentDropTarget != null)
			{
				_currentWindow.DragDrop(_currentDropTarget);
				root.CollectGarbage();
				dropHandled = true;
			}

			_currentWindowAreas.ForEach(a => _currentWindow.DragLeave(a));

			if (_currentDropTarget != null)
				_currentWindow.DragLeave(_currentDropTarget);

			if (_currentWindow != null)
				_currentWindow.DragLeave(_floatingWindow);

			_currentWindow = null;
			_currentHost = null;
			_isDrag = false;
		}

		/// <summary>
		/// Executes the abort operation.
		/// </summary>
		internal void Abort()
		{
			var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;

			_currentWindowAreas.ForEach(a => _currentWindow.DragLeave(a));

			if (_currentDropTarget != null)
				_currentWindow.DragLeave(_currentDropTarget);

			if (_currentWindow != null)
				_currentWindow.DragLeave(_floatingWindow);

			_currentWindow = null;

			if (_currentHost != null)
				_currentHost.HideOverlayWindow();

			_currentHost = null;
		}

		private void BringWindowToTop2(Window window)
		{
			if (window == null) return;

			Win32Helper.SetWindowPos(
				new WindowInteropHelper(window).Handle,
				IntPtr.Zero, 0, 0, 0, 0, Win32Helper.SetWindowPosFlags.IgnoreResize | Win32Helper.SetWindowPosFlags.IgnoreMove | Win32Helper.SetWindowPosFlags.DoNotActivate);
		}

		/// <summary>
		/// Gets the get Overlay Window Hosts.
		/// </summary>
		private void GetOverlayWindowHosts()
		{
			if (_manager?.Layout?.RootPanel?.CanDock == true)
			{
				_manager.GetOverlayWindowHostsByZOrder(ref _overlayWindowHosts, _floatingWindow);
			}
		}
	}
}