using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using AvalonDock;
using NUnit.Framework;

namespace AvalonDockTest;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class WindowChromeRegionTests
{
	[TestCase(0, 0)]
	[TestCase(4, 8)]
	[TestCase(8, 16)]
	[TestCase(10, 20)]
	[TestCase(12, 24)]
	[TestCase(16, 32)]
	[TestCase(7.25, 15)]
	public void CornerRadius_CreatesRegionWithMatchingEllipseDiameter(double radius, int diameter)
	{
		var worker = typeof(DockingManager).Assembly.GetType("Microsoft.Windows.Shell.WindowChromeWorker", true);
		var createRegion = worker.GetMethod("_CreateRoundRectRgn", BindingFlags.Static | BindingFlags.NonPublic);
		Assert.That(createRegion, Is.Not.Null);
		var bounds = new Rect(-2.25, 3.5, 103.5, 78.25);
		var actual = (IntPtr)createRegion.Invoke(null, new object[] { bounds, radius });
		var expected = radius == 0
			? CreateRectRgn(-3, 3, 102, 82)
			: CreateRoundRectRgn(-3, 3, 103, 83, diameter, diameter);
		try
		{
			Assert.That(actual, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(expected, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(EqualRgn(actual, expected), Is.True,
				"The native ellipse width and height must be the diameter, not the corner radius");
		}
		finally
		{
			if (actual != IntPtr.Zero) DeleteObject(actual);
			if (expected != IntPtr.Zero) DeleteObject(expected);
		}
	}

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);

	[DllImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EqualRgn(IntPtr first, IntPtr second);

	[DllImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DeleteObject(IntPtr region);
}
