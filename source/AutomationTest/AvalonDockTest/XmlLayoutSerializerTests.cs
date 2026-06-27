using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;
using AvalonDock.Core;
using AvalonDock.Core.Events;
using AvalonDock.Core.Serialization;
using AvalonDock.Core.Serialization.Dto;
using AvalonDock.Serializer.Xml;

namespace AvalonDockTest
{
	/// <summary>
	/// Unit tests for <see cref="XmlLayoutSerializer"/>.
	/// Tests round-trip serialization/deserialization of layout DTOs through XML,
	/// verifying structure preservation, edge cases, and integration with
	/// <see cref="LayoutSerializerBase"/>.
	/// </summary>
	[TestFixture]
	public class XmlLayoutSerializerTests
	{
		#region Test helpers / fakes

		/// <summary>
		/// Minimal fake <see cref="IDockingManager"/> for testing serialization
		/// without WPF dependencies.
		/// </summary>
		private sealed class FakeDockingManager : IDockingManager
		{
			public FakeDockingManager()
			{
				DtoMapper = new FakeDtoMapper();
			}

			public ISerializableLayoutRoot Layout { get; set; }
			public bool SuspendDocumentsSourceBinding { get; set; }
			public bool SuspendAnchorablesSourceBinding { get; set; }
			public ILayoutDtoMapper DtoMapper { get; }

			// IDockingManager members (unused in serialization tests)
			public IRootDock DockLayout { get; set; }
			public object ActiveContent { get; set; }
			public System.Collections.IEnumerable DocumentsSource { get; set; }
			public System.Collections.IEnumerable AnchorablesSource { get; set; }
			public int AutoHideDelay { get; set; }
			public bool AllowMixedOrientation { get; set; }

			public event EventHandler ActiveContentChanged;
			public event EventHandler LayoutChanged;
			public event EventHandler LayoutChanging;
			public event EventHandler<DocumentCancelEventArgs> DocumentClosing;
			public event EventHandler<DocumentEventArgs> DocumentClosed;
			public event EventHandler<AnchorableCancelEventArgs> AnchorableClosing;
			public event EventHandler<AnchorableEventArgs> AnchorableClosed;
			public event EventHandler<AnchorableCancelEventArgs> AnchorableHiding;
			public event EventHandler<AnchorableEventArgs> AnchorableHidden;
			public event EventHandler<ContentCancelEventArgs> ContentFloating;
			public event EventHandler<ContentEventArgs> ContentFloated;
			public event EventHandler<ContentCancelEventArgs> ContentDocking;
			public event EventHandler<ContentEventArgs> ContentDocked;

			// Suppress unused event warnings
			internal void SuppressWarnings()
			{
				ActiveContentChanged?.Invoke(this, EventArgs.Empty);
				LayoutChanged?.Invoke(this, EventArgs.Empty);
				LayoutChanging?.Invoke(this, EventArgs.Empty);
				DocumentClosing?.Invoke(this, null);
				DocumentClosed?.Invoke(this, null);
				AnchorableClosing?.Invoke(this, null);
				AnchorableClosed?.Invoke(this, null);
				AnchorableHiding?.Invoke(this, null);
				AnchorableHidden?.Invoke(this, null);
				ContentFloating?.Invoke(this, null);
				ContentFloated?.Invoke(this, null);
				ContentDocking?.Invoke(this, null);
				ContentDocked?.Invoke(this, null);
			}
		}

		/// <summary>
		/// Identity DTO mapper: wraps a <see cref="LayoutRootDto"/> in a
		/// <see cref="FakeLayoutRoot"/> so the serializer can call Descendents()
		/// and CollectGarbage() without a real layout tree.
		/// </summary>
		private sealed class FakeDtoMapper : ILayoutDtoMapper
		{
			public LayoutRootDto LastToDto { get; private set; }
			public LayoutRootDto LastFromDtoInput { get; private set; }

			public LayoutRootDto ToDto(ISerializableLayoutRoot layout)
			{
				if (layout is FakeLayoutRoot fake)
				{
					LastToDto = fake.Dto;
					return fake.Dto;
				}

				return new LayoutRootDto();
			}

			public ISerializableLayoutRoot FromDto(LayoutRootDto dto)
			{
				LastFromDtoInput = dto;
				return new FakeLayoutRoot(dto);
			}
		}

		/// <summary>
		/// Fake layout root that wraps a DTO and provides empty
		/// Descendents/CollectGarbage for the base class fixup logic.
		/// </summary>
		private sealed class FakeLayoutRoot : ISerializableLayoutRoot
		{
			public FakeLayoutRoot(LayoutRootDto dto) => Dto = dto;
			public LayoutRootDto Dto { get; }
			public bool GarbageCollected { get; private set; }

			public System.Collections.Generic.IEnumerable<ISerializableLayoutElement> Descendents()
				=> Enumerable.Empty<ISerializableLayoutElement>();

			public void CollectGarbage() => GarbageCollected = true;
		}

		/// <summary>Creates a minimal valid <see cref="LayoutRootDto"/>.</summary>
		private static LayoutRootDto CreateMinimalDto()
		{
			return new LayoutRootDto
			{
				RootPanel = new LayoutPanelDto
				{
					Orientation = "Horizontal",
					Children = { new LayoutDocumentPaneDto { Id = "docPane1" } }
				},
				TopSide = new LayoutAnchorSideDto(),
				RightSide = new LayoutAnchorSideDto(),
				LeftSide = new LayoutAnchorSideDto(),
				BottomSide = new LayoutAnchorSideDto()
			};
		}

		/// <summary>Creates a <see cref="LayoutRootDto"/> with documents and anchorables.</summary>
		private static LayoutRootDto CreateRichDto()
		{
			var docPane = new LayoutDocumentPaneDto
			{
				Id = "docPane1",
				Children =
				{
					new LayoutDocumentDto { Title = "Document1", ContentId = "doc1", IsSelected = true },
					new LayoutDocumentDto { Title = "Document2", ContentId = "doc2" }
				}
			};

			var anchorablePane = new LayoutAnchorablePaneDto
			{
				Id = "anchorPane1",
				Name = "ToolPane",
				Children =
				{
					new LayoutAnchorableDto
					{
						Title = "Properties",
						ContentId = "props",
						CanHide = true,
						CanAutoHide = true
					}
				}
			};

			return new LayoutRootDto
			{
				RootPanel = new LayoutPanelDto
				{
					Orientation = "Horizontal",
					Children = { anchorablePane, docPane }
				},
				TopSide = new LayoutAnchorSideDto(),
				RightSide = new LayoutAnchorSideDto(),
				LeftSide = new LayoutAnchorSideDto(),
				BottomSide = new LayoutAnchorSideDto()
			};
		}

		/// <summary>
		/// Serializes a DTO to XML using the <see cref="XmlLayoutSerializer"/>
		/// and returns the XML string.
		/// </summary>
		private static string SerializeToXml(LayoutRootDto dto, FakeDockingManager manager)
		{
			manager.Layout = new FakeLayoutRoot(dto);
			var serializer = new XmlLayoutSerializer(manager);

			using var stream = new MemoryStream();
			serializer.Serialize(stream);
			return Encoding.UTF8.GetString(stream.ToArray());
		}

		/// <summary>
		/// Deserializes XML back into the manager layout using the
		/// <see cref="XmlLayoutSerializer"/> and returns the resulting DTO
		/// from the mapper.
		/// </summary>
		private static LayoutRootDto DeserializeFromXml(string xml, FakeDockingManager manager)
		{
			var serializer = new XmlLayoutSerializer(manager);
			var bytes = Encoding.UTF8.GetBytes(xml);
			using var stream = new MemoryStream(bytes);
			serializer.Deserialize(stream);
			return ((FakeDtoMapper)manager.DtoMapper).LastFromDtoInput;
		}

		#endregion

		#region Constructor tests

		[Test]
		public void Constructor_NullManager_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new XmlLayoutSerializer(null));
		}

		[Test]
		public void Constructor_ValidManager_SetsManagerProperty()
		{
			var manager = new FakeDockingManager();
			var serializer = new XmlLayoutSerializer(manager);

			Assert.That(serializer.Manager, Is.SameAs(manager));
		}

		#endregion

		#region Serialize tests

		[Test]
		public void Serialize_MinimalDto_ProducesValidXml()
		{
			var manager = new FakeDockingManager();
			var xml = SerializeToXml(CreateMinimalDto(), manager);

			Assert.That(xml, Does.Contain("<LayoutRoot"));
			Assert.That(xml, Does.Contain("<RootPanel"));
			Assert.That(xml, Does.Contain("<LayoutDocumentPane"));
		}

		[Test]
		public void Serialize_RichDto_ContainsDocumentsAndAnchorables()
		{
			var manager = new FakeDockingManager();
			var xml = SerializeToXml(CreateRichDto(), manager);

			Assert.That(xml, Does.Contain("Document1"));
			Assert.That(xml, Does.Contain("Document2"));
			Assert.That(xml, Does.Contain("Properties"));
			Assert.That(xml, Does.Contain("doc1"));
			Assert.That(xml, Does.Contain("doc2"));
			Assert.That(xml, Does.Contain("props"));
		}

		[Test]
		public void Serialize_ProducesXmlWithoutDefaultNamespaces()
		{
			var manager = new FakeDockingManager();
			var xml = SerializeToXml(CreateMinimalDto(), manager);

			// The serializer adds empty namespace to suppress xmlns:xsi and xmlns:xsd
			Assert.That(xml, Does.Not.Contain("xmlns:xsi"));
			Assert.That(xml, Does.Not.Contain("xmlns:xsd"));
		}

		[Test]
		public void Serialize_EmptyRootPanel_ProducesValidXml()
		{
			var dto = new LayoutRootDto
			{
				RootPanel = new LayoutPanelDto(),
				TopSide = new LayoutAnchorSideDto(),
				RightSide = new LayoutAnchorSideDto(),
				LeftSide = new LayoutAnchorSideDto(),
				BottomSide = new LayoutAnchorSideDto()
			};

			var manager = new FakeDockingManager();
			var xml = SerializeToXml(dto, manager);

			Assert.That(xml, Does.Contain("<LayoutRoot"));
			Assert.That(xml, Does.Contain("<RootPanel"));
		}

		[Test]
		public void Serialize_HiddenAnchorables_IncludedInXml()
		{
			var dto = CreateMinimalDto();
			dto.Hidden.Add(new LayoutAnchorableDto
			{
				Title = "HiddenTool",
				ContentId = "hidden1"
			});

			var manager = new FakeDockingManager();
			var xml = SerializeToXml(dto, manager);

			Assert.That(xml, Does.Contain("HiddenTool"));
			Assert.That(xml, Does.Contain("hidden1"));
			Assert.That(xml, Does.Contain("<Hidden"));
		}

		[Test]
		public void Serialize_FloatingWindow_IncludedInXml()
		{
			var dto = CreateMinimalDto();
			dto.FloatingWindows.Add(new LayoutAnchorableFloatingWindowDto
			{
				RootPanel = new LayoutAnchorablePaneGroupDto
				{
					Children =
					{
						new LayoutAnchorablePaneDto
						{
							Id = "floatPane1",
							Children =
							{
								new LayoutAnchorableDto { Title = "FloatingTool", ContentId = "ft1" }
							}
						}
					}
				}
			});

			var manager = new FakeDockingManager();
			var xml = SerializeToXml(dto, manager);

			Assert.That(xml, Does.Contain("LayoutAnchorableFloatingWindow"));
			Assert.That(xml, Does.Contain("FloatingTool"));
		}

		[Test]
		public void Serialize_AnchorableAttributes_PreservedInXml()
		{
			var dto = CreateMinimalDto();
			((LayoutDocumentPaneDto)dto.RootPanel.Children[0]).Children.Add(
				new LayoutAnchorableDto
				{
					Title = "Tool",
					ContentId = "t1",
					CanHide = false,
					CanAutoHide = false,
					AutoHideWidth = 250.0,
					AutoHideHeight = 150.0,
					CanDockAsTabbedDocument = false,
					CanFloat = false
				});

			var manager = new FakeDockingManager();
			var xml = SerializeToXml(dto, manager);

			Assert.That(xml, Does.Contain("CanHide=\"false\""));
			Assert.That(xml, Does.Contain("CanAutoHide=\"false\""));
			Assert.That(xml, Does.Contain("AutoHideWidth=\"250\""));
			Assert.That(xml, Does.Contain("CanFloat=\"false\""));
		}

		#endregion

		#region Deserialize tests

		[Test]
		public void Deserialize_ValidXml_ProducesMatchingDto()
		{
			var manager = new FakeDockingManager();
			var originalDto = CreateRichDto();
			var xml = SerializeToXml(originalDto, manager);

			var roundTripped = DeserializeFromXml(xml, manager);

			Assert.That(roundTripped, Is.Not.Null);
			Assert.That(roundTripped.RootPanel, Is.Not.Null);
			Assert.That(roundTripped.RootPanel.Children.Count, Is.EqualTo(2));
		}

		[Test]
		public void Deserialize_SuspendsAndResumesBindings()
		{
			var manager = new FakeDockingManager();
			var xml = SerializeToXml(CreateMinimalDto(), manager);

			// After deserialization, bindings should be resumed (set back to false)
			DeserializeFromXml(xml, manager);

			Assert.That(manager.SuspendDocumentsSourceBinding, Is.False);
			Assert.That(manager.SuspendAnchorablesSourceBinding, Is.False);
		}

		[Test]
		public void Deserialize_SetsManagerLayout()
		{
			var manager = new FakeDockingManager();
			var xml = SerializeToXml(CreateMinimalDto(), manager);

			manager.Layout = null;
			DeserializeFromXml(xml, manager);

			Assert.That(manager.Layout, Is.Not.Null);
			Assert.That(manager.Layout, Is.InstanceOf<FakeLayoutRoot>());
		}

		[Test]
		public void Deserialize_CallsCollectGarbage()
		{
			var manager = new FakeDockingManager();
			var xml = SerializeToXml(CreateMinimalDto(), manager);

			DeserializeFromXml(xml, manager);

			var layout = (FakeLayoutRoot)manager.Layout;
			Assert.That(layout.GarbageCollected, Is.True);
		}

		#endregion

		#region Round-trip tests

		[Test]
		public void RoundTrip_MinimalLayout_PreservesStructure()
		{
			var manager = new FakeDockingManager();
			var original = CreateMinimalDto();
			var xml = SerializeToXml(original, manager);
			var result = DeserializeFromXml(xml, manager);

			Assert.That(result.RootPanel.Orientation, Is.EqualTo("Horizontal"));
			Assert.That(result.RootPanel.Children.Count, Is.EqualTo(1));

			var docPane = result.RootPanel.Children[0] as LayoutDocumentPaneDto;
			Assert.That(docPane, Is.Not.Null);
			Assert.That(docPane.Id, Is.EqualTo("docPane1"));
		}

		[Test]
		public void RoundTrip_DocumentProperties_Preserved()
		{
			var manager = new FakeDockingManager();
			var original = CreateRichDto();
			var xml = SerializeToXml(original, manager);
			var result = DeserializeFromXml(xml, manager);

			var docPane = result.RootPanel.Children
				.OfType<LayoutDocumentPaneDto>().First();
			var doc1 = docPane.Children.OfType<LayoutDocumentDto>()
				.First(d => d.ContentId == "doc1");

			Assert.That(doc1.Title, Is.EqualTo("Document1"));
			Assert.That(doc1.IsSelected, Is.True);
		}

		[Test]
		public void RoundTrip_AnchorableProperties_Preserved()
		{
			var manager = new FakeDockingManager();
			var original = CreateRichDto();
			var xml = SerializeToXml(original, manager);
			var result = DeserializeFromXml(xml, manager);

			var anchorPane = result.RootPanel.Children
				.OfType<LayoutAnchorablePaneDto>().First();
			var anchor = anchorPane.Children.First();

			Assert.That(anchor.Title, Is.EqualTo("Properties"));
			Assert.That(anchor.ContentId, Is.EqualTo("props"));
			Assert.That(anchor.CanHide, Is.True);
			Assert.That(anchor.CanAutoHide, Is.True);
		}

		[Test]
		public void RoundTrip_HiddenAnchorables_Preserved()
		{
			var manager = new FakeDockingManager();
			var dto = CreateMinimalDto();
			dto.Hidden.Add(new LayoutAnchorableDto
			{
				Title = "HiddenTool",
				ContentId = "hidden1",
				CanHide = true
			});

			var xml = SerializeToXml(dto, manager);
			var result = DeserializeFromXml(xml, manager);

			Assert.That(result.Hidden.Count, Is.EqualTo(1));
			Assert.That(result.Hidden[0].ContentId, Is.EqualTo("hidden1"));
			Assert.That(result.Hidden[0].Title, Is.EqualTo("HiddenTool"));
		}

		[Test]
		public void RoundTrip_FloatingWindows_Preserved()
		{
			var manager = new FakeDockingManager();
			var dto = CreateMinimalDto();
			dto.FloatingWindows.Add(new LayoutDocumentFloatingWindowDto
			{
				RootPanel = new LayoutDocumentPaneGroupDto
				{
					Children =
					{
						new LayoutDocumentPaneDto
						{
							Id = "floatDocPane",
							Children =
							{
								new LayoutDocumentDto { Title = "FloatingDoc", ContentId = "fd1" }
							}
						}
					}
				}
			});

			var xml = SerializeToXml(dto, manager);
			var result = DeserializeFromXml(xml, manager);

			Assert.That(result.FloatingWindows.Count, Is.EqualTo(1));
			var floatWindow = result.FloatingWindows[0] as LayoutDocumentFloatingWindowDto;
			Assert.That(floatWindow, Is.Not.Null);
			Assert.That(floatWindow.RootPanel.Children.Count, Is.EqualTo(1));
		}

		[Test]
		public void RoundTrip_AnchorSideWithGroups_Preserved()
		{
			var manager = new FakeDockingManager();
			var dto = CreateMinimalDto();
			dto.LeftSide = new LayoutAnchorSideDto
			{
				Children =
				{
					new LayoutAnchorGroupDto
					{
						Id = "group1",
						Children =
						{
							new LayoutAnchorableDto { Title = "AutoHideTool", ContentId = "ah1" }
						}
					}
				}
			};

			var xml = SerializeToXml(dto, manager);
			var result = DeserializeFromXml(xml, manager);

			Assert.That(result.LeftSide.Children.Count, Is.EqualTo(1));
			Assert.That(result.LeftSide.Children[0].Id, Is.EqualTo("group1"));
			Assert.That(result.LeftSide.Children[0].Children.Count, Is.EqualTo(1));
			Assert.That(result.LeftSide.Children[0].Children[0].ContentId, Is.EqualTo("ah1"));
		}

		[Test]
		public void RoundTrip_NestedPanels_Preserved()
		{
			var manager = new FakeDockingManager();
			var dto = new LayoutRootDto
			{
				RootPanel = new LayoutPanelDto
				{
					Orientation = "Vertical",
					Children =
					{
						new LayoutPanelDto
						{
							Orientation = "Horizontal",
							Children =
							{
								new LayoutDocumentPaneDto { Id = "topLeft" },
								new LayoutAnchorablePaneDto { Id = "topRight" }
							}
						},
						new LayoutDocumentPaneDto { Id = "bottom" }
					}
				},
				TopSide = new LayoutAnchorSideDto(),
				RightSide = new LayoutAnchorSideDto(),
				LeftSide = new LayoutAnchorSideDto(),
				BottomSide = new LayoutAnchorSideDto()
			};

			var xml = SerializeToXml(dto, manager);
			var result = DeserializeFromXml(xml, manager);

			Assert.That(result.RootPanel.Orientation, Is.EqualTo("Vertical"));
			Assert.That(result.RootPanel.Children.Count, Is.EqualTo(2));

			var nestedPanel = result.RootPanel.Children[0] as LayoutPanelDto;
			Assert.That(nestedPanel, Is.Not.Null);
			Assert.That(nestedPanel.Orientation, Is.EqualTo("Horizontal"));
			Assert.That(nestedPanel.Children.Count, Is.EqualTo(2));
		}

		[Test]
		public void RoundTrip_ContentFloatingPosition_Preserved()
		{
			var manager = new FakeDockingManager();
			var dto = CreateMinimalDto();
			var docPane = (LayoutDocumentPaneDto)dto.RootPanel.Children[0];
			docPane.Children.Add(new LayoutDocumentDto
			{
				Title = "PositionedDoc",
				ContentId = "pd1",
				FloatingLeft = 100.5,
				FloatingTop = 200.5,
				FloatingWidth = 800.0,
				FloatingHeight = 600.0,
				IsMaximized = true
			});

			var xml = SerializeToXml(dto, manager);
			var result = DeserializeFromXml(xml, manager);

			var docPaneResult = result.RootPanel.Children
				.OfType<LayoutDocumentPaneDto>().First();
			var doc = docPaneResult.Children.OfType<LayoutDocumentDto>()
				.First(d => d.ContentId == "pd1");

			Assert.That(doc.FloatingLeft, Is.EqualTo(100.5));
			Assert.That(doc.FloatingTop, Is.EqualTo(200.5));
			Assert.That(doc.FloatingWidth, Is.EqualTo(800.0));
			Assert.That(doc.FloatingHeight, Is.EqualTo(600.0));
			Assert.That(doc.IsMaximized, Is.True);
		}

		[Test]
		public void RoundTrip_PreviousContainerInfo_Preserved()
		{
			var manager = new FakeDockingManager();
			var dto = CreateMinimalDto();
			var docPane = (LayoutDocumentPaneDto)dto.RootPanel.Children[0];
			docPane.Children.Add(new LayoutAnchorableDto
			{
				Title = "Docked",
				ContentId = "docked1",
				PreviousContainerId = "anchorPane1",
				PreviousContainerIndex = 2
			});

			var xml = SerializeToXml(dto, manager);
			var result = DeserializeFromXml(xml, manager);

			var docPaneResult = result.RootPanel.Children
				.OfType<LayoutDocumentPaneDto>().First();
			var anchor = docPaneResult.Children.OfType<LayoutAnchorableDto>()
				.First(a => a.ContentId == "docked1");

			Assert.That(anchor.PreviousContainerId, Is.EqualTo("anchorPane1"));
			Assert.That(anchor.PreviousContainerIndex, Is.EqualTo(2));
		}

		#endregion

		#region Edge cases / error handling

		[Test]
		public void Deserialize_InvalidXml_ThrowsInvalidOperationException()
		{
			var manager = new FakeDockingManager();
			var serializer = new XmlLayoutSerializer(manager);
			var bytes = Encoding.UTF8.GetBytes("<NotALayoutRoot/>");

			using var stream = new MemoryStream(bytes);
			Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(stream));
		}

		[Test]
		public void Deserialize_EmptyStream_ThrowsInvalidOperationException()
		{
			var manager = new FakeDockingManager();
			var serializer = new XmlLayoutSerializer(manager);

			using var stream = new MemoryStream();
			Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(stream));
		}

		[Test]
		public void Deserialize_BindingsResumedEvenOnError()
		{
			var manager = new FakeDockingManager();
			var serializer = new XmlLayoutSerializer(manager);
			var bytes = Encoding.UTF8.GetBytes("<InvalidXml/>");

			using var stream = new MemoryStream(bytes);
			try { serializer.Deserialize(stream); } catch { }

			Assert.That(manager.SuspendDocumentsSourceBinding, Is.False,
				"Document source binding should be resumed after a failed deserialization.");
			Assert.That(manager.SuspendAnchorablesSourceBinding, Is.False,
				"Anchorable source binding should be resumed after a failed deserialization.");
		}

		[Test]
		public void Serialize_ToStream_CanBeDeserializedBack()
		{
			var manager = new FakeDockingManager();
			manager.Layout = new FakeLayoutRoot(CreateRichDto());
			var serializer = new XmlLayoutSerializer(manager);

			using var stream = new MemoryStream();
			serializer.Serialize(stream);

			stream.Position = 0;
			serializer.Deserialize(stream);

			Assert.That(manager.Layout, Is.Not.Null);
			var resultDto = ((FakeDtoMapper)manager.DtoMapper).LastFromDtoInput;
			Assert.That(resultDto.RootPanel, Is.Not.Null);
		}

		#endregion

		#region Extension method tests (TextWriter/TextReader)

		[Test]
		public void Serialize_ToTextWriter_ProducesValidXml()
		{
			var manager = new FakeDockingManager();
			manager.Layout = new FakeLayoutRoot(CreateRichDto());
			var serializer = new XmlLayoutSerializer(manager);

			using var writer = new StringWriter();
			serializer.Serialize(writer);
			var xml = writer.ToString();

			Assert.That(xml, Does.Contain("<LayoutRoot"));
			Assert.That(xml, Does.Contain("Document1"));
		}

		[Test]
		public void Deserialize_FromTextReader_Works()
		{
			var manager = new FakeDockingManager();
			var xml = SerializeToXml(CreateRichDto(), manager);

			using var reader = new StringReader(xml);
			var serializer = new XmlLayoutSerializer(manager);
			serializer.Deserialize(reader);

			Assert.That(manager.Layout, Is.Not.Null);
			var resultDto = ((FakeDtoMapper)manager.DtoMapper).LastFromDtoInput;
			Assert.That(resultDto.RootPanel.Children.Count, Is.EqualTo(2));
		}

		#endregion

		#region LayoutSerializationCallback tests

		[Test]
		public void LayoutSerializationCallback_EventCanBeSubscribed()
		{
			var manager = new FakeDockingManager();
			var serializer = new XmlLayoutSerializer(manager);
			var callbackInvoked = false;

			serializer.LayoutSerializationCallback += (s, e) =>
			{
				callbackInvoked = true;
			};

			// Callback is only triggered during fixup when layout has content-less
			// items. With our fake (empty Descendents), the event won't fire,
			// but we verify we can subscribe without error.
			Assert.That(callbackInvoked, Is.False);
		}

		#endregion
	}
}
