using System.IO;
using System.Text;
using System.Threading;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.Layout;
using AvalonDock.Serializer.Json;
using AvalonDock.Serializer.Xml;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Pins down that the tool tip stays out of the layout file.
	/// </summary>
	/// <remarks>
	/// A tool tip belongs to the content, not to the layout: <c>LayoutItem</c> pushes it down from the
	/// view every time the view is attached, and it is routinely a control or a binding, neither of
	/// which a layout file can describe. It used to be written as text and read back by nobody, which
	/// left a value in the file that nothing consumed. Storing it is what these tests guard against.
	/// </remarks>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class ToolTipNotSerializedTests
	{
		/// <summary>The serializers under test, so every case runs through both formats.</summary>
		/// <returns>The names of the formats.</returns>
		private static string[] Formats() => new[] { "Xml", "Json" };

		/// <summary>Builds a serializer of the named format for a docking manager.</summary>
		/// <param name="format">The format name.</param>
		/// <param name="manager">The docking manager.</param>
		/// <returns>The serializer.</returns>
		private static ILayoutSerializer CreateSerializer(string format, DockingManager manager) =>
			format == "Xml"
				? (ILayoutSerializer)new XmlLayoutSerializer(manager)
				: new JsonLayoutSerializer(manager);

		/// <summary>Builds a layout whose document and tool window both carry a tool tip.</summary>
		/// <returns>The layout root.</returns>
		private static LayoutRoot BuildLayout()
		{
			var document = new LayoutDocument
			{
				Title = "Readme",
				ContentId = "readme",
				ToolTip = "The project readme",
			};

			var anchorable = new LayoutAnchorable
			{
				Title = "Explorer",
				ContentId = "explorer",
				ToolTip = "Files of the solution",
			};

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane(document)));
			panel.Children.Add(new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(anchorable)));

			return new LayoutRoot { RootPanel = panel };
		}

		/// <summary>Stores a layout and hands back what was written.</summary>
		/// <param name="format">The format to store in.</param>
		/// <param name="layout">The layout to store.</param>
		/// <returns>The stored document.</returns>
		private static string Store(string format, LayoutRoot layout)
		{
			var manager = new DockingManager { Layout = layout };
			using var stream = new MemoryStream();
			CreateSerializer(format, manager).Serialize(stream);
			return Encoding.UTF8.GetString(stream.ToArray());
		}

		/// <summary>The stored layout must not mention the tool tip at all.</summary>
		/// <param name="format">The format to store in.</param>
		[Test]
		[TestCaseSource(nameof(Formats))]
		public void Serialize_DoesNotWriteTheToolTip(string format)
		{
			var stored = Store(format, BuildLayout());

			Assert.Multiple(() =>
			{
				Assert.That(stored, Does.Not.Contain("ToolTip"), "The tool tip is not part of a layout.");
				Assert.That(stored, Does.Not.Contain("The project readme"));
				Assert.That(stored, Does.Not.Contain("Files of the solution"));
				Assert.That(stored, Does.Contain("explorer"), "The layout itself was not stored.");
			});
		}

		/// <summary>
		/// A layout file left over from v4 carries a ToolTip attribute. Reading it has to ignore that
		/// attribute rather than fail over it.
		/// </summary>
		[Test]
		public void Deserialize_IgnoresAToolTipLeftInAnOlderXmlFile()
		{
			var stored = Store("Xml", BuildLayout())
				.Replace("ContentId=\"explorer\"", "ContentId=\"explorer\" ToolTip=\"Files of the solution\"");

			Assert.That(stored, Does.Contain("ToolTip"), "The older attribute was not injected.");

			var manager = new DockingManager();
			var serializer = new XmlLayoutSerializer(manager);
			serializer.LayoutSerializationCallback += (s, e) => e.Content = new object();

			using var input = new MemoryStream(Encoding.UTF8.GetBytes(stored));

			Assert.DoesNotThrow(() => serializer.Deserialize(input));
		}
	}
}
