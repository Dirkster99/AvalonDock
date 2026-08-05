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
	/// (PreviousContainerIndex). Serializer settings that skip members holding the default of their
	/// CLR type therefore have to stay off, or every <see langword="false"/> comes back as
	/// <see langword="true"/>.
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
	}
}
