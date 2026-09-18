using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using AvalonDock.Layout;
using AvalonDock.Platform;

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
		private bool _isUpdatingMouseLocation;

		/// <summary>
		/// The overlay window currently showing compass drop-target indicators for whichever host the
		/// pointer is presently over, or null between/before drag positions have selected a host. Test-only
		/// accessor (see TestInternalsVisibleTo.cs) so a test can enumerate <see cref="IOverlayWindow.GetTargets"/>
		/// by <see cref="DropTargetType"/> and get each indicator's real screen bounds during a live drag.
		/// </summary>
		internal IOverlayWindow CurrentOverlayWindow => _currentWindow;
		internal IDropTarget CurrentDropTarget => _currentDropTarget;
		internal string CurrentDropTargetType => _currentDropTarget?.Type.ToString();

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
			if (_isUpdatingMouseLocation)
				return;

			_isUpdatingMouseLocation = true;
			try
			{
			// The floating window's layout model can be detached from the layout tree by an unrelated,
			// concurrent operation (docking, re-floating, a layout reset) while this drag is still being
			// tracked - most commonly by a drag whose end this class never learned about (see the
			// watchdogs in LayoutFloatingWindowControl), but also possibly mid-drag for a still-active
			// one. Once that happens this DragService can no longer do anything meaningful with it, so
			// treat it the same as an aborted drag instead of dereferencing a broken Model.Root chain.
			if (_floatingWindow?.Model?.Root == null)
			{
				Abort();
				return;
			}

			////var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;
			// TODO - pass in without DPI adjustment, screen co-ords, adjust inside the target window
			if (!_isDrag)
			{
				// A previous drag that never reported its end - Windows can drop the modal move loop
				// without a WM_EXITSIZEMOVE when a window crosses monitors of a different DPI - may still
				// have an overlay window on screen. Clearing them here keeps them from accumulating into
				// empty windows that only disappear with the application (issue #587).
				_manager?.HideAllOverlayWindows();
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

					if (_currentHost != newHost && newHost != null)
					{
						_currentHost = newHost;
						_currentWindow = _currentHost.ShowOverlayWindow(_floatingWindow);
						if (_currentWindow == null)
						{
							_currentHost = null;
							return;
						}
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

			// _currentWindow is set together with _currentHost in the host-transition block above, but a
			// race between this call and a concurrent one (observed in practice between the managed
			// OnMouseMove path and the native-drag DispatcherTimer tick, which both mutate this
			// lock-free, multi-field state) can leave them briefly inconsistent - guard the invariant
			// explicitly here rather than assuming it holds for every _currentWindow access below.
			if (_currentHost == null || _currentWindow == null)
				return;
			if (_currentWindow is OverlayWindow overlay)
				overlay.EnsureNativePositionDuringDrag();

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
			finally
			{
				_isUpdatingMouseLocation = false;
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

			// See the matching guard in UpdateMouseLocation: the model can be detached from the layout
			// tree (by a concurrent dock/re-float/layout reset) between the drag being tracked and this
			// Drop() call - Root being null here just means there's nothing to drop onto or collect.
			var floatingWindowModel = _floatingWindow?.Model as LayoutFloatingWindow;
			var root = floatingWindowModel?.Root;

			if (_currentHost != null)
				_currentHost.HideOverlayWindow();

			if (_currentDropTarget != null && root != null && _currentWindow != null)
			{
				_currentWindow.DragDrop(_currentDropTarget);
				root.CollectGarbage();
				dropHandled = true;
			}

			if (_currentWindow != null)
			{
				_currentWindowAreas.ForEach(a => _currentWindow.DragLeave(a));

				if (_currentDropTarget != null)
					_currentWindow.DragLeave(_currentDropTarget);

				_currentWindow.DragLeave(_floatingWindow);
			}

			_currentWindow = null;
			_currentHost = null;
			_isDrag = false;

			// The host tracked above is not necessarily the only one that has been asked to show an
			// overlay window during this drag, so every host is cleared (issue #587).
			_manager?.HideAllOverlayWindows();
		}

		/// <summary>
		/// Executes the abort operation.
		/// </summary>
		internal void Abort()
		{
			// An abort also runs when the dragged window is closed while the drag is still in progress,
			// where there may be no overlay window to leave any more (issue #587).
			if (_currentWindow != null)
			{
				_currentWindowAreas.ForEach(a => _currentWindow.DragLeave(a));

				if (_currentDropTarget != null)
					_currentWindow.DragLeave(_currentDropTarget);

				_currentWindow.DragLeave(_floatingWindow);
			}

			_currentWindowAreas.Clear();
			_currentDropTarget = null;
			_currentWindow = null;

			if (_currentHost != null)
				_currentHost.HideOverlayWindow();

			_currentHost = null;
			_isDrag = false;

			// The host tracked above is not necessarily the only one that has been asked to show an
			// overlay window during this drag, so every host is cleared (issue #587).
			_manager?.HideAllOverlayWindows();
		}

		/// <summary>
		/// Raises a window without activating it.
		/// </summary>
		/// <remarks>
		/// The call order above is what puts the drop-target compass on top: the host is raised, then
		/// the dragged floating window, then the overlay. That only worked on Windows, because
		/// SetWindowPos is a user32 import and WindowInteropHelper.Handle is not a real handle off
		/// Windows - the whole ordering became a silent no-op and the floating window ended up
		/// covering the compass. Route non-Windows through the platform service instead, which maps
		/// to [NSWindow orderFront:] on macOS - the same "raise but do not activate" semantics as the
		/// DoNotActivate flag below.
		/// </remarks>
		/// <param name="window">The window to raise.</param>
		private void BringWindowToTop2(Window window)
		{
			if (window == null) return;

			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var nativeHandle = PlatformHelper.GetNativeWindowHandle(window);
				if (nativeHandle != IntPtr.Zero)
					PlatformHelper.BringWindowToFront(nativeHandle);
				return;
			}

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
