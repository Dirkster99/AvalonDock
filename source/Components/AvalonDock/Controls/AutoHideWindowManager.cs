/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Threading;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// This class manages the timing process when the user clicks on a <see cref="LayoutAnchorControl"/>
	/// of a particular anchored item and the system pops up the corresponding <see cref="LayoutAutoHideWindowControl"/>
	/// and reduces it later back to the anchored <see cref="LayoutAnchorControl"/> if the user clicks
	/// into another focusable element.
	/// </summary>
	internal class AutoHideWindowManager
	{
		#region fields
		private DockingManager _manager;
		private WeakReference _currentAutohiddenAnchor = null;
		private DispatcherTimer _closeTimer = null;
		#endregion fields

		#region constructors
		/// <summary>Class constructor from <see cref="DockingManager"/>.</summary>
		/// <param name="manager"></param>
		internal AutoHideWindowManager(DockingManager manager)
		{
			_manager = manager;
			this.SetupCloseTimer();
		}

		#endregion constructors

		#region public methods
		/// <summary>Method is invoked to pop put an Anchorable that was in AutoHide mode.</summary>
		/// <param name="anchor"><see cref="LayoutAnchorControl"/> to pop out of the side panel.</param>
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

		/// <summary>Method is invoked to reduce the Anchorable back to AutoHide mode
		/// after waiting for a configured time in the <see cref="DockingManager.AutoHideDelay"/> property.</summary>
		/// <param name="anchor"><see cref="LayoutAnchorControl"/> to pop out of the side panel.</param>
		public void HideAutoWindow(LayoutAnchorControl anchor = null)
		{
			if (anchor == null ||
				anchor == _currentAutohiddenAnchor.GetValueOrDefault<LayoutAnchorControl>())
			{
				StopCloseTimer();
			}
			else
				System.Diagnostics.Debug.Assert(false);
		}
		#endregion public methods

		#region private methods
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
		#endregion private methods
	}
}
