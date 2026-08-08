using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AvalonDock.Core.Serialization;
using AvalonDock.Core.Serialization.Dto;
using AvalonDock.Layout;
using AvalonDock.Serialization;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Unit tests covering persistence of the detached window state of a <see cref="LayoutAnchorable"/>,
	/// so that a restored layout can recreate the standalone windows the user had open.
	/// </summary>
	[TestFixture]
	public class DetachedAnchorableSerializationTests
	{
		private static readonly ILayoutDtoMapper Mapper = new LayoutDtoMapper();
		private static readonly XmlSerializer DtoSerializer = new XmlSerializer(typeof(LayoutRootDto));

		/// <summary>Builds a layout with one docked anchorable that can be flagged as detached.</summary>
		/// <param name="isDetached">The value to put on the anchorable.</param>
		/// <returns>The layout root.</returns>
		private static LayoutRoot CreateLayout(bool isDetached)
		{
			var toolPane = new LayoutAnchorablePane();
			toolPane.Children.Add(new LayoutAnchorable
			{
				Title = "Tool1",
				ContentId = "tool1",
				IsDetached = isDetached,
				FloatingLeft = 120,
				FloatingTop = 240,
				FloatingWidth = 360,
				FloatingHeight = 480,
			});

			var panel = new LayoutPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
			panel.Children.Add(new LayoutAnchorablePaneGroup(toolPane));
			return new LayoutRoot { RootPanel = panel };
		}

		/// <summary>Round-trips a layout through the DTO serializer.</summary>
		/// <param name="root">The layout to round-trip.</param>
		/// <returns>The restored layout.</returns>
		private static LayoutRoot RoundTrip(LayoutRoot root)
		{
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);

			string xml;
			using (var sw = new StringWriter())
			{
				DtoSerializer.Serialize(sw, Mapper.ToDto(root), ns);
				xml = sw.ToString();
			}

			using (var sr = new StringReader(xml))
			{
				return (LayoutRoot)Mapper.FromDto((LayoutRootDto)DtoSerializer.Deserialize(sr));
			}
		}

		/// <summary>Finds the single anchorable of a layout.</summary>
		/// <param name="root">The layout to search.</param>
		/// <returns>The anchorable.</returns>
		private static LayoutAnchorable SingleAnchorable(LayoutRoot root) =>
			root.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "tool1");

		/// <summary>A detached anchorable must come back detached.</summary>
		[Test]
		public void RoundTrip_PreservesIsDetached()
		{
			var restored = RoundTrip(CreateLayout(true));

			Assert.That(SingleAnchorable(restored).IsDetached, Is.True,
				"An anchorable detached into a standalone window should stay flagged after a round trip.");
		}

		/// <summary>An ordinary anchorable must not come back detached.</summary>
		[Test]
		public void RoundTrip_LeavesDockedAnchorableAttached()
		{
			var restored = RoundTrip(CreateLayout(false));

			Assert.That(SingleAnchorable(restored).IsDetached, Is.False,
				"A docked anchorable should never be restored as detached.");
		}

		/// <summary>
		/// The window geometry rides on the existing floating properties, so it has to survive too -
		/// otherwise a restored window would not reappear where the user left it.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesDetachedWindowGeometry()
		{
			var restored = SingleAnchorable(RoundTrip(CreateLayout(true)));

			Assert.Multiple(() =>
			{
				Assert.That(restored.FloatingLeft, Is.EqualTo(120));
				Assert.That(restored.FloatingTop, Is.EqualTo(240));
				Assert.That(restored.FloatingWidth, Is.EqualTo(360));
				Assert.That(restored.FloatingHeight, Is.EqualTo(480));
			});
		}

		/// <summary>The flag is only written when it is set, keeping saved layouts unchanged otherwise.</summary>
		[Test]
		public void Serialize_OmitsIsDetached_WhenNotDetached()
		{
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);

			using (var sw = new StringWriter())
			{
				DtoSerializer.Serialize(sw, Mapper.ToDto(CreateLayout(false)), ns);

				Assert.That(sw.ToString(), Does.Not.Contain("IsDetached"),
					"A layout without detached anchorables should not gain a new attribute.");
			}
		}

		/// <summary>The flag is written when it is set, so the state actually reaches disk.</summary>
		[Test]
		public void Serialize_WritesIsDetached_WhenDetached()
		{
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);

			using (var sw = new StringWriter())
			{
				DtoSerializer.Serialize(sw, Mapper.ToDto(CreateLayout(true)), ns);

				Assert.That(sw.ToString(), Does.Contain("IsDetached"),
					"A detached anchorable should be recorded in the saved layout.");
			}
		}
	}
}