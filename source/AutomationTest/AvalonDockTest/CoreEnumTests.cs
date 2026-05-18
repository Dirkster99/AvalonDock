using System;
using AvalonDock.Core;
using NUnit.Framework;

namespace AvalonDockTest
{
	[TestFixture]
	public class ToolboxSideTests
	{
		[Test]
		public void ToolboxSide_HasExpectedValues()
		{
			Assert.That((int)ToolboxSide.Left, Is.EqualTo(0));
			Assert.That((int)ToolboxSide.Right, Is.EqualTo(1));
			Assert.That((int)ToolboxSide.Bottom, Is.EqualTo(2));
		}

		[Test]
		public void ToolboxSide_HasExactlyThreeValues()
		{
			var values = Enum.GetValues(typeof(ToolboxSide));
			Assert.That(values.Length, Is.EqualTo(3));
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
