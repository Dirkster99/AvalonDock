using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Serializer.Json;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Round-trip tests for <see cref="JsonLayoutSerializer"/>.
	/// </summary>
	/// <remarks>
	/// The layout DTOs carry properties whose default is <see langword="true"/> (CanHide, CanFloat,
	/// CanAutoHide, CanDockAsTabbedDocument, CanMove, CanShowOnHover) or -1
	/// (PreviousContainerIndex), so every value has to be written out. A setting that skips members
	/// holding the default of their CLR type would turn each <see langword="false"/> back into
	/// <see langword="true"/> on load.
	/// </remarks>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class JsonLayoutSerializerTests
	{
		/// <summary>Round-trips the layout of a manager through JSON.</summary>
		/// <param name="layout">The layout to store.</param>
		/// <returns>The restored layout.</returns>
		private static LayoutRoot RoundTrip(LayoutRoot layout)
		{
			var source = new DockingManager { Layout = layout };

			using var stream = new MemoryStream();
			new JsonLayoutSerializer(source).Serialize(stream);
			var json = Encoding.UTF8.GetString(stream.ToArray());

			var target = new DockingManager();
			using var input = new MemoryStream(Encoding.UTF8.GetBytes(json));

			// Without a callback an anchorable is only restored when the target already knows its
			// content id, so the anchorable of the source layout is handed over verbatim.
			var serializer = new JsonLayoutSerializer(target);
			serializer.LayoutSerializationCallback += (s, e) => e.Content = new object();
			serializer.Deserialize(input);

			return target.Layout;
		}

		/// <summary>Builds a layout with a single anchorable carrying the given flags.</summary>
		/// <param name="canHide">The value for CanHide.</param>
		/// <param name="canFloat">The value for CanFloat.</param>
		/// <param name="canDockAsTabbedDocument">The value for CanDockAsTabbedDocument.</param>
		/// <returns>The layout root.</returns>
		private static LayoutRoot LayoutWithAnchorable(bool canHide, bool canFloat, bool canDockAsTabbedDocument)
		{
			var pane = new LayoutAnchorablePane();
			pane.Children.Add(new LayoutAnchorable
			{
				Title = "Tool 1",
				ContentId = "tool1",
				CanHide = canHide,
				CanFloat = canFloat,
				CanDockAsTabbedDocument = canDockAsTabbedDocument,
			});

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));
			return new LayoutRoot { RootPanel = panel };
		}

		/// <summary>Finds the single anchorable of a layout.</summary>
		/// <param name="layout">The layout to search.</param>
		/// <returns>The anchorable.</returns>
		private static LayoutAnchorable Anchorable(LayoutRoot layout) =>
			layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "tool1");

		/// <summary>
		/// Flags that were switched off must stay off. They hold the default of their CLR type, which
		/// is exactly the case a "skip default values" serializer setting would drop.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesFlagsThatAreSwitchedOff()
		{
			var restored = Anchorable(RoundTrip(LayoutWithAnchorable(false, false, false)));

			Assert.Multiple(() =>
			{
				Assert.That(restored.CanHide, Is.False, "CanHide was stored as false.");
				Assert.That(restored.CanFloat, Is.False, "CanFloat was stored as false.");
				Assert.That(restored.CanDockAsTabbedDocument, Is.False,
					"CanDockAsTabbedDocument was stored as false.");
			});
		}

		/// <summary>Flags that were left on must stay on.</summary>
		[Test]
		public void RoundTrip_PreservesFlagsThatAreSwitchedOn()
		{
			var restored = Anchorable(RoundTrip(LayoutWithAnchorable(true, true, true)));

			Assert.Multiple(() =>
			{
				Assert.That(restored.CanHide, Is.True);
				Assert.That(restored.CanFloat, Is.True);
				Assert.That(restored.CanDockAsTabbedDocument, Is.True);
			});
		}

		/// <summary>The titles and content ids of the stored layout come back unchanged.</summary>
		[Test]
		public void RoundTrip_PreservesTitleAndContentId()
		{
			var restored = Anchorable(RoundTrip(LayoutWithAnchorable(true, true, true)));

			Assert.That(restored.Title, Is.EqualTo("Tool 1"));
			Assert.That(restored.ContentId, Is.EqualTo("tool1"));
		}

		/// <summary>
		/// The panes and pane groups of the layout are stored in collections declared with their
		/// abstract base type, so each entry has to carry a type discriminator to come back as the
		/// class it was. Without it the tree is unreadable.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesThePaneTreeStructure()
		{
			var documentPane = new LayoutDocumentPane();
			documentPane.Children.Add(new LayoutDocument { Title = "Doc 1", ContentId = "doc1" });

			var toolPane = new LayoutAnchorablePane { Name = "ToolsPane" };
			toolPane.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(documentPane))
			{
				Orientation = System.Windows.Controls.Orientation.Horizontal,
			};
			panel.Children.Add(new LayoutAnchorablePaneGroup(toolPane));

			var restored = RoundTrip(new LayoutRoot { RootPanel = panel });

			Assert.Multiple(() =>
			{
				Assert.That(restored.RootPanel.Children.OfType<LayoutDocumentPaneGroup>().Count(), Is.EqualTo(1),
					"The document pane group must come back as a document pane group.");
				Assert.That(restored.RootPanel.Children.OfType<LayoutAnchorablePaneGroup>().Count(), Is.EqualTo(1),
					"The anchorable pane group must come back as an anchorable pane group.");
				Assert.That(restored.Descendents().OfType<LayoutAnchorablePane>().Single().Name,
					Is.EqualTo("ToolsPane"));
				Assert.That(restored.Descendents().OfType<LayoutDocument>().Single().ContentId,
					Is.EqualTo("doc1"));
			});
		}

		/// <summary>
		/// A floating window is stored in a list declared with the abstract floating-window base, so
		/// it needs the same discriminator treatment as the pane tree.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesAnchorableFloatingWindows()
		{
			var floatingPane = new LayoutAnchorablePane();
			floatingPane.Children.Add(new LayoutAnchorable { Title = "Watch", ContentId = "tool1" });

			var layout = new LayoutRoot
			{
				RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())),
			};
			layout.FloatingWindows.Add(new LayoutAnchorableFloatingWindow
			{
				RootPanel = new LayoutAnchorablePaneGroup(floatingPane),
			});

			var restored = RoundTrip(layout);

			Assert.That(restored.FloatingWindows.OfType<LayoutAnchorableFloatingWindow>().Count(),
				Is.EqualTo(1),
				"The floating window must come back as an anchorable floating window.");
		}
	}
}
