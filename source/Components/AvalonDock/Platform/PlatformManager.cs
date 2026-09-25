using System;
using System.Runtime.InteropServices;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Provides platform-specific implementations of services.
	/// Uses conditional compilation to select the appropriate platform implementation.
	/// </summary>
	internal static class PlatformManager
	{
		private static INativeWindowService _nativeWindowService;
		private static ICursorService _cursorService;
		private static IDpiService _dpiService;

		/// <summary>
		/// Gets the native window service for the current platform.
		/// </summary>
		internal static INativeWindowService NativeWindowService =>
			_nativeWindowService ??= CreateNativeWindowService();

		/// <summary>
		/// Gets the cursor service for the current platform.
		/// </summary>
		internal static ICursorService CursorService =>
			_cursorService ??= CreateCursorService();

		/// <summary>
		/// Gets the DPI service for the current platform.
		/// </summary>
		internal static IDpiService DpiService =>
			_dpiService ??= CreateDpiService();

	private static INativeWindowService CreateNativeWindowService()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return new MacOSNativeWindowService();
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return new LinuxNativeWindowService();
		return new WindowsNativeWindowService();
	}

	private static ICursorService CreateCursorService()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return new MacOSCursorService();
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return new LinuxCursorService();
		return new WindowsCursorService();
	}

	private static IDpiService CreateDpiService()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return new MacOSDpiService();
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return new LinuxDpiService();
		return new WindowsDpiService();
	}
	}
}