using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Serializer.Json;
using AvalonDock.Serializer.Xml;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Cross-checks the XML and the JSON serializer against each other.
	/// </summary>
	/// <remarks>
	/// Both write the same DTOs but decide differently what to leave out: the XML serializer omits a
	/// value that equals the default of its DTO through the ShouldSerialize convention, while
	/// System.Text.Json does not honour that convention and writes everything. Restoring either has
	/// to end at the same layout, so any drift between the two shows up here rather than in an
	/// application that switched formats.
	/// </remarks>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class SerializerParityTests
	{
		/// <summary>
		/// Builds a layout touching the parts the two formats have to agree on: both content types,
		/// both pane types, nesting, a side, the hidden list and a floating window.
		/// </summary>
		/// <returns>The layout root.</returns>
		private static LayoutRoot BuildLayout()
		{
			var documentPane = new LayoutDocumentPane { ShowHeader = false };
			((ILayoutPaneSerializable)documentPane).Id = "docPane";
			documentPane.Children.Add(new LayoutDocument
			{
				Title = "Readme",
				ContentId = "readme",
				Description = "Docs",
				CanClose = false,
				CanMove = false,
				IsSelected = true,
			});

			var toolPane = new LayoutAnchorablePane { Name = "ToolsPane", DockWidth = new System.Windows.GridLength(220) };
			((ILayoutPaneSerializable)toolPane).Id = "toolsPane";
			toolPane.Children.Add(new LayoutAnchorable
			{
				Title = "Explorer",
				ContentId = "explorer",
				CanHide = false,
				CanClose = true,
				CanFloat = false,
				CanAutoHide = false,
				CanMove = false,
				CanShowOnHover = false,
				CanDockAsTabbedDocument = false,
			});

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(documentPane));
			panel.Children.Add(new LayoutAnchorablePaneGroup(toolPane));

			var layout = new LayoutRoot { RootPanel = panel };

			var group = new LayoutAnchorGroup();
			group.Children.Add(new LayoutAnchorable { Title = "Output", ContentId = "output" });
			layout.LeftSide.Children.Add(group);

			layout.Hidden.Add(new LayoutAnchorable { Title = "Hidden Tool", ContentId = "hiddenTool" });

			var floatingPane = new LayoutAnchorablePane();
			floatingPane.Children.Add(new LayoutAnchorable { Title = "Watch", ContentId = "watch" });
			layout.FloatingWindows.Add(new LayoutAnchorableFloatingWindow
			{
				RootPanel = new LayoutAnchorablePaneGroup(floatingPane),
			});

			return layout;
		}

		/// <summary>Round-trips a layout through the XML serializer.</summary>
		/// <param name="layout">The layout to store.</param>
		/// <returns>The restored layout.</returns>
		private static LayoutRoot RoundTripXml(LayoutRoot layout)
		{
			var source = new DockingManager { Layout = layout };
			using var stream = new MemoryStream();
			new XmlLayoutSerializer(source).Serialize(stream);

			var target = new DockingManager();
			var serializer = new XmlLayoutSerializer(target);
			serializer.LayoutSerializationCallback += (s, e) => e.Content = new object();

			using var input = new MemoryStream(stream.ToArray());
			serializer.Deserialize(input);
			return target.Layout;
		}

		/// <summary>Round-trips a layout through the JSON serializer.</summary>
		/// <param name="layout">The layout to store.</param>
		/// <returns>The restored layout.</returns>
		private static LayoutRoot RoundTripJson(LayoutRoot layout)
		{
			var source = new DockingManager { Layout = layout };
			using var stream = new MemoryStream();
			new JsonLayoutSerializer(source).Serialize(stream);

			var target = new DockingManager();
			var serializer = new JsonLayoutSerializer(target);
			serializer.LayoutSerializationCallback += (s, e) => e.Content = new object();

			using var input = new MemoryStream(stream.ToArray());
			serializer.Deserialize(input);
			return target.Layout;
		}

		/// <summary>
		/// Describes a layout as a flat list of lines, so two layouts can be compared with a diff
		/// that names the element that drifted rather than just reporting inequality.
		/// </summary>
		/// <param name="layout">The layout to describe.</param>
		/// <returns>One line per element.</returns>
		private static List<string> Describe(LayoutRoot layout)
		{
			var lines = new List<string>();

			foreach (var element in layout.Descendents())
			{
				switch (element)
				{
					case LayoutAnchorable anchorable:
						lines.Add(
							$"Anchorable id={anchorable.ContentId} title={anchorable.Title} "
							+ $"canHide={anchorable.CanHide} canClose={anchorable.CanClose} "
							+ $"canFloat={anchorable.CanFloat} canAutoHide={anchorable.CanAutoHide} "
							+ $"canMove={anchorable.CanMove} canShowOnHover={anchorable.CanShowOnHover} "
							+ $"canDockAsTabbedDocument={anchorable.CanDockAsTabbedDocument} "
							+ $"autoHideMinWidth={anchorable.AutoHideMinWidth} hidden={anchorable.IsHidden}");
						break;

					case LayoutDocument document:
						lines.Add(
							$"Document id={document.ContentId} title={document.Title} "
							+ $"description={document.Description} canClose={document.CanClose} "
							+ $"canMove={document.CanMove} selected={document.IsSelected}");
						break;

					case LayoutAnchorablePane anchorablePane:
						lines.Add(
							$"AnchorablePane id={((ILayoutPaneSerializable)anchorablePane).Id} "
							+ $"name={anchorablePane.Name} widthStar={anchorablePane.DockWidth.IsStar} "
							+ $"width={anchorablePane.DockWidth.Value} minWidth={anchorablePane.DockMinWidth}");
						break;

					case LayoutDocumentPane documentPane:
						lines.Add(
							$"DocumentPane id={((ILayoutPaneSerializable)documentPane).Id} "
							+ $"showHeader={documentPane.ShowHeader}");
						break;

					case LayoutAnchorablePaneGroup anchorableGroup:
						lines.Add($"AnchorablePaneGroup orientation={anchorableGroup.Orientation}");
						break;

					case LayoutDocumentPaneGroup documentGroup:
						lines.Add($"DocumentPaneGroup orientation={documentGroup.Orientation}");
						break;

					case LayoutAnchorGroup anchorGroup:
						lines.Add($"AnchorGroup children={anchorGroup.Children.Count}");
						break;

					case LayoutAnchorSide anchorSide:
						lines.Add($"AnchorSide {anchorSide.Side} groups={anchorSide.Children.Count}");
						break;

					case LayoutPanel panelElement:
						lines.Add($"Panel orientation={panelElement.Orientation}");
						break;

					case LayoutAnchorableFloatingWindow anchorableFloating:
						lines.Add($"AnchorableFloatingWindow valid={anchorableFloating.IsValid}");
						break;

					case LayoutDocumentFloatingWindow documentFloating:
						lines.Add($"DocumentFloatingWindow valid={documentFloating.IsValid}");
						break;
				}
			}

			return lines;
		}

		/// <summary>Both formats have to restore the same tree from the same layout.</summary>
		[Test]
		public void XmlAndJson_RestoreTheSameLayout()
		{
			var viaXml = Describe(RoundTripXml(BuildLayout()));
			var viaJson = Describe(RoundTripJson(BuildLayout()));

			Assert.That(viaJson, Is.EqualTo(viaXml));
		}

		/// <summary>Neither format may lose the tool windows parked in the hidden list.</summary>
		[Test]
		public void XmlAndJson_KeepTheSameHiddenList()
		{
			var viaXml = RoundTripXml(BuildLayout()).Hidden.Select(a => a.ContentId).OrderBy(id => id);
			var viaJson = RoundTripJson(BuildLayout()).Hidden.Select(a => a.ContentId).OrderBy(id => id);

			Assert.That(viaJson, Is.EqualTo(viaXml));
			Assert.That(viaJson, Does.Contain("hiddenTool"));
		}

		/// <summary>
		/// The flags that are switched off are the ones a "skip default values" rule would drop, and
		/// they have to survive both formats identically.
		/// </summary>
		[Test]
		public void XmlAndJson_KeepTheSameSwitchedOffFlags()
		{
			var fromXml = RoundTripXml(BuildLayout()).Descendents()
				.OfType<LayoutAnchorable>().Single(a => a.ContentId == "explorer");
			var fromJson = RoundTripJson(BuildLayout()).Descendents()
				.OfType<LayoutAnchorable>().Single(a => a.ContentId == "explorer");

			var xmlFlags = Flags(fromXml);
			var jsonFlags = Flags(fromJson);

			Assert.Multiple(() =>
			{
				Assert.That(jsonFlags, Is.EqualTo(xmlFlags), "The two formats disagree about the flags.");
				Assert.That(
					jsonFlags,
					Is.EqualTo(new[] { false, false, false, false, false, false, true }),
					"CanHide, CanFloat, CanAutoHide, CanMove, CanShowOnHover and "
					+ "CanDockAsTabbedDocument were stored off and CanClose on.");
			});
		}

		/// <summary>
		/// The auto hide minimums are the one pair whose "skip the default" rule was comparing against
		/// a value neither the model nor the DTO uses any more. Both formats have to restore the
		/// default and a changed value alike.
		/// </summary>
		/// <param name="stored">The value stored on the anchorable.</param>
		[TestCase(100.0)]
		[TestCase(240.0)]
		public void XmlAndJson_RestoreTheSameAutoHideMinimums(double stored)
		{
			LayoutRoot Build()
			{
				var layout = BuildLayout();
				var anchorable = layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "explorer");
				anchorable.AutoHideMinWidth = stored;
				anchorable.AutoHideMinHeight = stored;
				return layout;
			}

			double[] Minimums(LayoutRoot layout)
			{
				var restored = layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "explorer");
				return new[] { restored.AutoHideMinWidth, restored.AutoHideMinHeight };
			}

			var viaXml = Minimums(RoundTripXml(Build()));
			var viaJson = Minimums(RoundTripJson(Build()));

			Assert.Multiple(() =>
			{
				Assert.That(viaJson, Is.EqualTo(viaXml), "The two formats disagree about the minimums.");
				Assert.That(viaXml, Is.EqualTo(new[] { stored, stored }));
			});
		}

		/// <summary>Reads the flags of an anchorable in a fixed order.</summary>
		/// <param name="anchorable">The anchorable to read.</param>
		/// <returns>The flag values.</returns>
		private static bool[] Flags(LayoutAnchorable anchorable) => new[]
		{
			anchorable.CanHide,
			anchorable.CanFloat,
			anchorable.CanAutoHide,
			anchorable.CanMove,
			anchorable.CanShowOnHover,
			anchorable.CanDockAsTabbedDocument,
			anchorable.CanClose,
		};
	}
}
