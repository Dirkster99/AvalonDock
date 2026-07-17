using AvalonDock.Core;
using NUnit.Framework;

namespace AvalonDockTest
{
	[TestFixture]
	public class IToolboxContractTests
	{
		[Test]
		public void IToolbox_ExtendsIDockable()
		{
			Assert.That(typeof(IDockable).IsAssignableFrom(typeof(IToolbox)), Is.True);
		}

		[Test]
		public void IToolbox_HasToolTipTextProperty()
		{
			var prop = typeof(IToolbox).GetProperty("ToolTipText");
			Assert.That(prop, Is.Not.Null);
			Assert.That(prop!.PropertyType, Is.EqualTo(typeof(string)));
			Assert.That(prop.GetMethod, Is.Not.Null);
			Assert.That(prop.SetMethod, Is.Not.Null);
		}

		[Test]
		public void IToolbox_HasZoneProperty()
		{
			var prop = typeof(IToolbox).GetProperty("Zone");
			Assert.That(prop, Is.Not.Null);
			Assert.That(prop!.PropertyType, Is.EqualTo(typeof(DockZone)));
			Assert.That(prop.GetMethod, Is.Not.Null);
			Assert.That(prop.SetMethod, Is.Not.Null);
		}

		[Test]
		public void IToolbox_HasIsOpenByDefaultProperty()
		{
			var prop = typeof(IToolbox).GetProperty("IsOpenByDefault");
			Assert.That(prop, Is.Not.Null);
			Assert.That(prop!.PropertyType, Is.EqualTo(typeof(bool)));
		}

		[Test]
		public void IToolbox_HasIconProperty()
		{
			var prop = typeof(IToolbox).GetProperty("Icon");
			Assert.That(prop, Is.Not.Null);
			Assert.That(prop!.PropertyType, Is.EqualTo(typeof(object)));
		}

		[Test]
		public void IDockable_HasRequiredProperties()
		{
			var expectedProps = new[] { "Id", "Title", "Context", "Owner", "Factory",
				"CanClose", "CanPin", "CanFloat", "CanDrag", "CanDrop",
				"IsModified", "IsActive", "DockState" };

			foreach (var name in expectedProps)
			{
				var prop = typeof(IDockable).GetProperty(name);
				Assert.That(prop, Is.Not.Null, $"Property '{name}' not found on IDockable");
			}
		}

		[Test]
		public void IDockable_HasOnCloseMethod()
		{
			var method = typeof(IDockable).GetMethod("OnClose");
			Assert.That(method, Is.Not.Null);
			Assert.That(method!.ReturnType, Is.EqualTo(typeof(bool)));
		}

		[Test]
		public void IDockable_HasOnSelectedMethod()
		{
			var method = typeof(IDockable).GetMethod("OnSelected");
			Assert.That(method, Is.Not.Null);
			Assert.That(method!.ReturnType, Is.EqualTo(typeof(void)));
		}
	}
}
