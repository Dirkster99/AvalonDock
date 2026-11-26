using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Serializer.Json;
using AvalonDock.Serializer.Xml;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Regression tests for #696: a <see cref="LayoutAnchorGroup"/> restored from a stored layout must
	/// be relinked to its previous container, so the empty pane it came from survives the restore and
	/// docking back reuses that pane and its size.
	/// </summary>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class AnchorGroupPreviousContainerSerializationTests
	{
		private static DockingManager CreateManagerWithAutoHiddenPane(out LayoutAnchorable panel)
		{
			var manager = new DockingManager();
			panel = new LayoutAnchorable { ContentId = "RightPane", Title = "Right", Content = "right" };
			var root = new LayoutPanel { Orientation = Orientation.Horizontal };
			root.Children.Add(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			root.Children.Add(new LayoutAnchorablePane(panel) { DockWidth = new GridLength(600), DockMinWidth = 320 });
			manager.Layout = new LayoutRoot { RootPanel = root };
			panel.AutoHideMinWidth = 320;
			panel.ToggleAutoHide();
			return manager;
		}

		private static void AssertDocksBackIntoOriginalPane(DockingManager manager)
		{
			var group = manager.Layout.Descendents().OfType<LayoutAnchorGroup>().Single();
			Assert.That(((ILayoutPreviousContainer)group).PreviousContainer, Is.InstanceOf<LayoutAnchorablePane>());

			var restored = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "RightPane");
			restored.ToggleAutoHide();

			var pane = restored.Parent as LayoutAnchorablePane;
			Assert.That(pane, Is.Not.Null);
			Assert.That(pane.DockWidth.Value, Is.EqualTo(600));
		}

		[Test]
		public void XmlRoundTrip_AutoHiddenPane_DocksBackAtStoredWidth()
		{
			var manager = CreateManagerWithAutoHiddenPane(out _);

			var writer = new StringWriter();
			new XmlLayoutSerializer(manager).Serialize(writer);

			var serializer = new XmlLayoutSerializer(manager);
			serializer.LayoutSerializationCallback += (_, a) => a.Content ??= "restored";
			serializer.Deserialize(new StringReader(writer.ToString()));

			AssertDocksBackIntoOriginalPane(manager);
		}

		[Test]
		public void JsonRoundTrip_AutoHiddenPane_DocksBackAtStoredWidth()
		{
			var manager = CreateManagerWithAutoHiddenPane(out _);

			var writer = new StringWriter();
			new JsonLayoutSerializer(manager).Serialize(writer);

			var serializer = new JsonLayoutSerializer(manager);
			serializer.LayoutSerializationCallback += (_, a) => a.Content ??= "restored";
			serializer.Deserialize(new StringReader(writer.ToString()));

			AssertDocksBackIntoOriginalPane(manager);
		}
	}
}
