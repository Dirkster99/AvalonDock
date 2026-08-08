using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AvalonDock;
using AvalonDock.Core.Serialization.Dto;
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

		/// <summary>Serializes a layout and returns the JSON text.</summary>
		/// <param name="layout">The layout to store.</param>
		/// <returns>The JSON.</returns>
		private static string Serialize(LayoutRoot layout)
		{
			var manager = new DockingManager { Layout = layout };

			using var stream = new MemoryStream();
			new JsonLayoutSerializer(manager).Serialize(stream);
			return Encoding.UTF8.GetString(stream.ToArray());
		}

		/// <summary>Restores a layout from JSON, attaching content to every item.</summary>
		/// <param name="json">The JSON to read.</param>
		/// <returns>The restored layout.</returns>
		private static LayoutRoot Deserialize(string json)
		{
			var target = new DockingManager();
			var serializer = new JsonLayoutSerializer(target);
			serializer.LayoutSerializationCallback += (s, e) => e.Content = new object();

			using var input = new MemoryStream(Encoding.UTF8.GetBytes(json));
			serializer.Deserialize(input);
			return target.Layout;
		}

		/// <summary>Builds a layout holding a single anchorable inside an anchorable pane.</summary>
		/// <param name="configure">Applied to the anchorable before it is stored.</param>
		/// <returns>The layout root.</returns>
		private static LayoutRoot LayoutWith(System.Action<LayoutAnchorable> configure)
		{
			var anchorable = new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" };
			configure?.Invoke(anchorable);

			var pane = new LayoutAnchorablePane();
			pane.Children.Add(anchorable);

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));
			return new LayoutRoot { RootPanel = panel };
		}

		/// <summary>Round-trips a layout holding one configured anchorable and returns it.</summary>
		/// <param name="configure">Applied to the anchorable before it is stored.</param>
		/// <returns>The restored anchorable.</returns>
		private static LayoutAnchorable RoundTripAnchorable(System.Action<LayoutAnchorable> configure) =>
			Anchorable(RoundTrip(LayoutWith(configure)));

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

		/// <summary>A document floating window has to come back as one too, not as its sibling type.</summary>
		[Test]
		public void RoundTrip_PreservesDocumentFloatingWindows()
		{
			var floatingPane = new LayoutDocumentPane();
			floatingPane.Children.Add(new LayoutDocument { Title = "Report", ContentId = "doc1" });

			var layout = new LayoutRoot
			{
				RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())),
			};
			layout.FloatingWindows.Add(new LayoutDocumentFloatingWindow
			{
				RootPanel = new LayoutDocumentPaneGroup(floatingPane),
			});

			var restored = RoundTrip(layout);

			Assert.That(restored.FloatingWindows.OfType<LayoutDocumentFloatingWindow>().Count(),
				Is.EqualTo(1));
			Assert.That(restored.FloatingWindows.OfType<LayoutAnchorableFloatingWindow>(), Is.Empty);
		}

		#region Content flags

		/// <summary>An anchorable defaults to CanClose false, so true is the value worth checking.</summary>
		[Test]
		public void RoundTrip_PreservesCanCloseSwitchedOnForAnAnchorable()
		{
			Assert.That(RoundTripAnchorable(a => a.CanClose = true).CanClose, Is.True);
		}

		/// <summary>A document defaults to CanClose true, so false is the value worth checking.</summary>
		[Test]
		public void RoundTrip_PreservesCanCloseSwitchedOffForADocument()
		{
			var pane = new LayoutDocumentPane();
			pane.Children.Add(new LayoutDocument { Title = "Doc", ContentId = "doc1", CanClose = false });

			var restored = RoundTrip(new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(pane)) });
			var document = restored.Descendents().OfType<LayoutDocument>().Single(d => d.ContentId == "doc1");

			Assert.That(document.CanClose, Is.False);
		}

		/// <summary>Every flag that defaults to true is checked in its switched-off state.</summary>
		[Test]
		public void RoundTrip_PreservesTheRemainingFlagsSwitchedOff()
		{
			var restored = RoundTripAnchorable(a =>
			{
				a.CanAutoHide = false;
				a.CanMove = false;
				a.CanShowOnHover = false;
			});

			Assert.Multiple(() =>
			{
				Assert.That(restored.CanAutoHide, Is.False);
				Assert.That(restored.CanMove, Is.False);
				Assert.That(restored.CanShowOnHover, Is.False);
			});
		}

		/// <summary>
		/// The selected flag survives, so the tab that was in front comes back in front.
		/// </summary>
		/// <remarks>
		/// Its companion IsLastFocusedDocument is not covered here: the setter is internal, driven by
		/// the layout root when the active content changes, so a test cannot put a content item into
		/// that state without going through a docking manager.
		/// </remarks>
		[Test]
		public void RoundTrip_PreservesTheSelectedFlag()
		{
			Assert.That(RoundTripAnchorable(a => a.IsSelected = true).IsSelected, Is.True);
		}

		#endregion

		#region Sizes and positions

		/// <summary>The floating bounds a content item remembers survive.</summary>
		[Test]
		public void RoundTrip_PreservesFloatingBounds()
		{
			var restored = RoundTripAnchorable(a =>
			{
				a.FloatingLeft = 120.5;
				a.FloatingTop = 240.25;
				a.FloatingWidth = 360;
				a.FloatingHeight = 480;
				a.IsMaximized = true;
			});

			Assert.Multiple(() =>
			{
				Assert.That(restored.FloatingLeft, Is.EqualTo(120.5));
				Assert.That(restored.FloatingTop, Is.EqualTo(240.25));
				Assert.That(restored.FloatingWidth, Is.EqualTo(360));
				Assert.That(restored.FloatingHeight, Is.EqualTo(480));
				Assert.That(restored.IsMaximized, Is.True);
			});
		}

		/// <summary>
		/// The auto-hide sizes survive. The values stay above the minimums, which the setters clamp to.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesAutoHideSizes()
		{
			var restored = RoundTripAnchorable(a =>
			{
				a.AutoHideMinWidth = 120;
				a.AutoHideMinHeight = 130;
				a.AutoHideWidth = 300;
				a.AutoHideHeight = 250;
			});

			Assert.Multiple(() =>
			{
				Assert.That(restored.AutoHideMinWidth, Is.EqualTo(120));
				Assert.That(restored.AutoHideMinHeight, Is.EqualTo(130));
				Assert.That(restored.AutoHideWidth, Is.EqualTo(300));
				Assert.That(restored.AutoHideHeight, Is.EqualTo(250));
			});
		}

		/// <summary>An absolute dock length survives as an absolute length of the same size.</summary>
		[Test]
		public void RoundTrip_PreservesAbsoluteDockLengths()
		{
			var pane = new LayoutAnchorablePane { DockWidth = new System.Windows.GridLength(275) };
			pane.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

			var restored = RoundTrip(new LayoutRoot { RootPanel = panel });
			var restoredPane = restored.Descendents().OfType<LayoutAnchorablePane>().Single();

			Assert.Multiple(() =>
			{
				Assert.That(restoredPane.DockWidth.IsAbsolute, Is.True);
				Assert.That(restoredPane.DockWidth.Value, Is.EqualTo(275));
			});
		}

		/// <summary>A star length is the default and has to survive as one.</summary>
		[Test]
		public void RoundTrip_PreservesStarDockLengths()
		{
			var restored = RoundTrip(LayoutWith(null));
			var pane = restored.Descendents().OfType<LayoutAnchorablePane>().Single();

			Assert.That(pane.DockWidth.IsStar, Is.True);
		}

		/// <summary>The dock minimums survive when they differ from the default of 25.</summary>
		[Test]
		public void RoundTrip_PreservesDockMinimums()
		{
			var pane = new LayoutAnchorablePane { DockMinWidth = 80, DockMinHeight = 90 };
			pane.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

			var restored = RoundTrip(new LayoutRoot { RootPanel = panel });
			var restoredPane = restored.Descendents().OfType<LayoutAnchorablePane>().Single();

			Assert.Multiple(() =>
			{
				Assert.That(restoredPane.DockMinWidth, Is.EqualTo(80));
				Assert.That(restoredPane.DockMinHeight, Is.EqualTo(90));
			});
		}

		#endregion

		#region Identity and placement

		/// <summary>
		/// The pane an item was last docked in is remembered by id and index. Index 0 is the value a
		/// serializer that skips CLR defaults would drop, and the item would come back at the end.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesPreviousContainerIdAndIndexZero()
		{
			var pane = new LayoutAnchorablePane();
			((ILayoutPaneSerializable)pane).Id = "toolsPane";
			pane.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });

			var hidden = new LayoutAnchorable { Title = "Hidden Tool", ContentId = "hiddenTool" };
			((ILayoutPreviousContainer)hidden).PreviousContainerId = "toolsPane";
			hidden.PreviousContainerIndex = 0;

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

			var layout = new LayoutRoot { RootPanel = panel };
			layout.Hidden.Add(hidden);

			var restored = RoundTrip(layout);
			var restoredHidden = restored.Hidden.Single(a => a.ContentId == "hiddenTool");

			Assert.Multiple(() =>
			{
				Assert.That(restoredHidden.PreviousContainerIndex, Is.EqualTo(0));
				Assert.That(((ILayoutPreviousContainer)restoredHidden).PreviousContainerId,
					Is.EqualTo("toolsPane"));
			});
		}

		/// <summary>The hidden list survives, so a tool window parked there is not lost.</summary>
		[Test]
		public void RoundTrip_PreservesTheHiddenList()
		{
			var layout = LayoutWith(null);
			layout.Hidden.Add(new LayoutAnchorable { Title = "Hidden Tool", ContentId = "hiddenTool" });

			var restored = RoundTrip(layout);

			Assert.That(restored.Hidden.Select(a => a.ContentId), Does.Contain("hiddenTool"));
		}

		/// <summary>The pane identifiers survive, which is what previous containers are matched by.</summary>
		[Test]
		public void RoundTrip_PreservesPaneIds()
		{
			var pane = new LayoutAnchorablePane { Name = "ToolsPane" };
			((ILayoutPaneSerializable)pane).Id = "toolsPane";
			pane.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

			var restored = RoundTrip(new LayoutRoot { RootPanel = panel });
			var restoredPane = restored.Descendents().OfType<LayoutAnchorablePane>().Single();

			Assert.Multiple(() =>
			{
				Assert.That(((ILayoutPaneSerializable)restoredPane).Id, Is.EqualTo("toolsPane"));
				Assert.That(restoredPane.Name, Is.EqualTo("ToolsPane"));
			});
		}

		/// <summary>
		/// An anchorable parked on a side comes back on that side, which is what the toggle docking
		/// manager builds its sidebar from.
		/// </summary>
		[TestCase(AnchorSide.Left)]
		[TestCase(AnchorSide.Right)]
		[TestCase(AnchorSide.Top)]
		[TestCase(AnchorSide.Bottom)]
		public void RoundTrip_PreservesAutoHiddenAnchorablesPerSide(AnchorSide side)
		{
			var layout = new LayoutRoot
			{
				RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())),
			};

			var group = new LayoutAnchorGroup();
			group.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });
			SideOf(layout, side).Children.Add(group);

			var restored = RoundTrip(layout);

			Assert.That(SideOf(restored, side).Children.SelectMany(g => g.Children).Select(a => a.ContentId),
				Does.Contain("tool1"));
		}

		/// <summary>Gets the anchor side of a layout.</summary>
		/// <param name="layout">The layout.</param>
		/// <param name="side">The wanted side.</param>
		/// <returns>The side.</returns>
		private static LayoutAnchorSide SideOf(LayoutRoot layout, AnchorSide side)
		{
			switch (side)
			{
				case AnchorSide.Left: return layout.LeftSide;
				case AnchorSide.Right: return layout.RightSide;
				case AnchorSide.Top: return layout.TopSide;
				default: return layout.BottomSide;
			}
		}

		#endregion

		#region Type discriminators

		/// <summary>
		/// The discriminator is written where a value is stored under an abstract base and nowhere
		/// else. The layout below holds exactly two such slots: the pane group inside the root panel
		/// and the pane inside that group. The anchorable sits in a list declared with its own type.
		/// </summary>
		[Test]
		public void Serialize_WritesADiscriminatorOnlyWhereTheDeclaredTypeIsAbstract()
		{
			var pane = new LayoutAnchorablePane();
			pane.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });

			var panel = new LayoutPanel();
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

			var json = Serialize(new LayoutRoot { RootPanel = panel });
			var discriminators = System.Text.RegularExpressions.Regex.Matches(json, "\"_type\"").Count;

			Assert.That(discriminators, Is.EqualTo(2),
				$"Expected a discriminator on the pane group and the pane only. JSON was:{System.Environment.NewLine}{json}");
		}

		/// <summary>Each concrete type is stored under its own name.</summary>
		[Test]
		public void Serialize_NamesTheConcreteTypeInTheDiscriminator()
		{
			var json = Serialize(LayoutWith(null));

			Assert.Multiple(() =>
			{
				Assert.That(json, Does.Contain(nameof(LayoutAnchorablePaneGroupDto)));
				Assert.That(json, Does.Contain(nameof(LayoutAnchorablePaneDto)));
				Assert.That(json, Does.Contain(nameof(LayoutDocumentPaneGroupDto)));
			});
		}

		/// <summary>A discriminator naming a type this version does not know is refused, not guessed.</summary>
		[Test]
		public void Deserialize_WithUnknownDiscriminator_Throws()
		{
			var json = Serialize(LayoutWith(null))
				.Replace(nameof(LayoutAnchorablePaneDto), "SomeTypeFromTheFuture");

			Assert.Throws<System.Text.Json.JsonException>(() => Deserialize(json));
		}

		#endregion
	}
}
