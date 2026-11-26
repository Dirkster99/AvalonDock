using System;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Linux implementation of <see cref="IDpiService"/> for LibreWPF's portable backend.
	/// <para>
	/// With no <c>gdi32</c>/monitor P/Invoke available, DPI is read from the WPF composition
	/// target's device transform (<see cref="CompositionTarget.TransformToDevice"/>) and the work
	/// area from <see cref="SystemParameters"/>. On the current portable Linux backend the device
	/// transform is identity (DPI scale 1.0), so these values also match the assumptions baked
	/// into the portable caption-drag path.
	/// </para>
	/// </summary>
	internal class LinuxDpiService : IDpiService
	{
		public double GetPrimaryMonitorDpi()
			=> DpiScaleOf(LinuxPlatformInterop.GetReferenceWindow());

		public double GetMonitorDpi(IntPtr windowHandle)
			=> DpiScaleOf(LinuxPlatformInterop.WindowFromHandle(windowHandle)
				?? LinuxPlatformInterop.GetReferenceWindow());

		public Rect GetPrimaryMonitorWorkArea()
		{
			// SystemParameters.WorkArea is the taskbar-excluded desktop rectangle and is the
			// closest managed equivalent of the Win32 MONITORINFO.rcWork the Windows service uses.
			try
			{
				var work = SystemParameters.WorkArea;
				if (work.Width > 0 && work.Height > 0)
					return work;
			}
			catch
			{
				// Some portable backends leave WorkArea unimplemented; fall through to full screen.
			}

			try
			{
				var w = SystemParameters.PrimaryScreenWidth;
				var h = SystemParameters.PrimaryScreenHeight;
				if (w > 0 && h > 0)
					return new Rect(0, 0, w, h);
			}
			catch
			{
			}

			return new Rect(0, 0, 1920, 1080); // Last-resort fallback, mirrors the Windows service.
		}

		public Rect GetMonitorWorkArea(IntPtr windowHandle)
		{
			// The portable backend is effectively single-surface; there is no per-monitor work area
			// to distinguish, so defer to the primary work area for every handle.
			return GetPrimaryMonitorWorkArea();
		}

		private static double DpiScaleOf(Window window)
		{
			if (window == null)
				return 1.0;

			try
			{
				var source = PresentationSource.FromVisual(window);
				var transform = source?.CompositionTarget?.TransformToDevice;
				if (transform.HasValue)
				{
					var scale = transform.Value.M11;
					if (scale > 0)
						return scale;
				}
			}
			catch
			{
				// TransformToDevice can throw while a window is tearing down; assume unscaled.
			}

			return 1.0;
		}
	}
}
