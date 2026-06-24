using System;
using AvalonDock.Core;
using NUnit.Framework;

namespace AvalonDockTest
{
	[TestFixture]
	public class DockZoneTests
	{
		[Test]
		public void DockZone_HasExpectedValues()
		{
			Assert.That((int)DockZone.LeftTop, Is.EqualTo(0));
			Assert.That((int)DockZone.LeftBottom, Is.EqualTo(1));
			Assert.That((int)DockZone.RightTop, Is.EqualTo(2));
			Assert.That((int)DockZone.RightBottom, Is.EqualTo(3));
			Assert.That((int)DockZone.BottomLeft, Is.EqualTo(4));
			Assert.That((int)DockZone.BottomRight, Is.EqualTo(5));
		}

		[Test]
		public void DockZone_HasExactlySixValues()
		{
			var values = Enum.GetValues(typeof(DockZone));
			Assert.That(values.Length, Is.EqualTo(6));
		}
	}

	[TestFixture]
	public class DockStateTests
	{
		[Test]
		public void DockState_HasExpectedValues()
		{
			Assert.That((int)DockState.Docked, Is.EqualTo(0));
			Assert.That((int)DockState.AutoHidden, Is.EqualTo(1));
			Assert.That((int)DockState.Float, Is.EqualTo(2));
			Assert.That((int)DockState.Hidden, Is.EqualTo(3));
		}
	}

	[TestFixture]
	public class DockAlignmentTests
	{
		[Test]
		public void DockAlignment_HasExpectedValues()
		{
			Assert.That((int)DockAlignment.Left, Is.EqualTo(0));
			Assert.That((int)DockAlignment.Right, Is.EqualTo(1));
			Assert.That((int)DockAlignment.Top, Is.EqualTo(2));
			Assert.That((int)DockAlignment.Bottom, Is.EqualTo(3));
		}
	}
}
