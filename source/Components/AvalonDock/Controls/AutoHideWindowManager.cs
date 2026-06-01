using System;
using System.Windows.Threading;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the auto Hide Window Manager.
	/// </summary>
	internal class AutoHideWindowManager
	{
		private DockingManager _manager;
		private WeakReference _currentAutohiddenAnchor = null;
		private DispatcherTimer _closeTimer = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoHideWindowManager"/> class.
		/// </summary>
		/// <param name="manager">The manager.</param>
		internal AutoHideWindowManager(DockingManager manager)
		{
			_manager = manager;
			this.SetupCloseTimer();
		}

		/// <summary>
		/// Executes the show Auto Hide Window operation.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		public void ShowAutoHideWindow(LayoutAnchorControl anchor)
		{
			if (_currentAutohiddenAnchor.GetValueOrDefault<LayoutAnchorControl>() != anchor)
			{
				StopCloseTimer();
				_currentAutohiddenAnchor = new WeakReference(anchor);
				_manager.AutoHideWindow.Show(anchor);
				StartCloseTimer();
			}
		}

		/// <summary>
		/// Executes the hide Auto Window operation.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		public void HideAutoWindow(LayoutAnchorControl anchor = null)
		{
			if (anchor == null ||
				anchor == _currentAutohiddenAnchor.GetValueOrDefault<LayoutAnchorControl>())
			{
				StopCloseTimer();
			}
			else
			{
				System.Diagnostics.Debug.Assert(false);
			}
		}

		private void SetupCloseTimer()
		{
			_closeTimer = new DispatcherTimer(DispatcherPriority.Background);
			_closeTimer.Interval = TimeSpan.FromMilliseconds(_manager.AutoHideDelay);
			_closeTimer.Tick += (s, e) =>
			{
				if (_manager.AutoHideWindow.IsWin32MouseOver ||
					((LayoutAnchorable)_manager.AutoHideWindow.Model).IsActive ||
					_manager.AutoHideWindow.IsResizing)
					return;

				StopCloseTimer();
			};
		}

		private void StartCloseTimer()
		{
			_closeTimer.Start();
		}

		private void StopCloseTimer()
		{
			_closeTimer.Stop();
			_manager.AutoHideWindow.Hide();
			_currentAutohiddenAnchor = null;
		}
	}
}