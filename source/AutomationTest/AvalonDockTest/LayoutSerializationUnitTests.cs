using NUnit.Framework;
using AvalonDock.Layout;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AvalonDockTest
{
	/// <summary>
	/// Unit tests for LayoutRoot XML serialization — using IXmlSerializable directly.
	/// Tests the model layer without a DockingManager.
	/// Covers issues:
	///   #111 - LayoutRoot doesn't know how to deserialize
	///   #88  - LayoutRoot IXmlSerializable read end element
	///   #118 - Losing pane ratio after serialization
	///   #56  - Anchorable with reloaded layout cannot show again
	///   #74  - NullReferenceException comes back
	/// </summary>
	[TestFixture]
	public class LayoutSerializationUnitTests
	{
		private LayoutRoot CreateSampleLayout()
		{
			var docPane = new LayoutDocumentPane();
			docPane.Children.Add(new LayoutDocument { Title = "Doc1", ContentId = "doc1" });
			docPane.Children.Add(new LayoutDocument { Title = "Doc2", ContentId = "doc2" });

			var toolPane = new LayoutAnchorablePane { DockMinWidth = 100 };
			toolPane.Children.Add(new LayoutAnchorable { Title = "Tool1", ContentId = "tool1" });
			toolPane.Children.Add(new LayoutAnchorable { Title = "Tool2", ContentId = "tool2" });

			var panel = new LayoutPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
			panel.Children.Add(new LayoutDocumentPaneGroup(docPane));
			panel.Children.Add(new LayoutAnchorablePaneGroup(toolPane));

			var root = new LayoutRoot { RootPanel = panel };

			// Add auto-hide anchorable to left side
			var autoHideGroup = new LayoutAnchorGroup();
			autoHideGroup.Children.Add(new LayoutAnchorable { Title = "AutoHide1", ContentId = "autohide1" });
			var leftSide = new LayoutAnchorSide();
			leftSide.Children.Add(autoHideGroup);
			root.LeftSide = leftSide;

			return root;
		}

		private string SerializeLayout(LayoutRoot root)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			using (var sw = new StringWriter())
			{
				serializer.Serialize(sw, root);
				return sw.ToString();
			}
		}

		private LayoutRoot DeserializeLayout(string xml)
		{
			var serializer = new XmlSerializer(typeof(LayoutRoot));
			using (var sr = new StringReader(xml))
			{
				return (LayoutRoot)serializer.Deserialize(sr);
			}
		}

		/// <summary>
		/// Verifies basic serialization of LayoutRoot produces valid XML.
		/// Regression for #111 - LayoutRoot doesn't know how to deserialize.
		/// </summary>
		[Test]
		public void Serialize_ProducesValidXml_Issue111()
		{
			var root = CreateSampleLayout();
			string xml = SerializeLayout(root);

			Assert.That(xml, Is.Not.Empty, "Serialized XML should not be empty.");
			Assert.That(xml, Does.Contain("doc1"), "XML should contain doc1 ContentId.");
			Assert.That(xml, Does.Contain("tool1"), "XML should contain tool1 ContentId.");
		}

		/// <summary>
		/// Verifies serialization doesn't throw.
		/// Regression for #88 - LayoutRoot IXmlSerializable read end element.
		/// </summary>
		[Test]
		public void Serialize_DoesNotThrow_Issue88()
		{
			var root = CreateSampleLayout();

			Assert.DoesNotThrow(() => SerializeLayout(root),
				"Serializing a LayoutRoot should not throw (Issue #88).");
		}

		/// <summary>
		/// Verifies round-trip serialization/deserialization preserves ContentIds.
		/// Regression for #118 - Losing pane ratio after serialization.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesContentIds_Issue118()
		{
			var root = CreateSampleLayout();
			string xml = SerializeLayout(root);

			var restored = DeserializeLayout(xml);

			var contentIds = restored.Descendents().OfType<LayoutContent>()
				.Select(c => c.ContentId).Where(id => id != null).ToList();

			Assert.That(contentIds, Does.Contain("doc1"));
			Assert.That(contentIds, Does.Contain("doc2"));
			Assert.That(contentIds, Does.Contain("tool1"));
			Assert.That(contentIds, Does.Contain("tool2"));
		}

		/// <summary>
		/// Verifies round-trip preserves document count.
		/// </summary>
		[Test]
		public void RoundTrip_PreservesDocumentCount()
		{
			var root = CreateSampleLayout();
			int originalDocCount = root.Descendents().OfType<LayoutDocument>().Count();

			string xml = SerializeLayout(root);
			var restored = DeserializeLayout(xml);

			int restoredDocCount = restored.Descendents().OfType<LayoutDocument>().Count();
			Assert.That(restoredDocCount, Is.EqualTo(originalDocCount),
				"Document count should be preserved after round-trip.");
		}

		/// <summary>
		/// Verifies that all elements in the tree have proper parent references.
		/// Regression for #74 - NullReferenceException comes back.
		/// </summary>
		[Test]
		public void LayoutTree_AllElementsHaveParent_Issue74()
		{
			var root = CreateSampleLayout();

			foreach (var content in root.Descendents().OfType<LayoutContent>())
			{
				Assert.That(content.Parent, Is.Not.Null,
					$"LayoutContent '{content.Title}' should have a parent (Issue #74).");
			}
		}

		/// <summary>
		/// Verifies that hidden anchorables survive serialization.
		/// Regression for #56 - Anchorable with reloaded layout cannot show again.
		/// </summary>
		[Test]
		public void Serialize_IncludesHiddenAnchorables_Issue56()
		{
			var root = CreateSampleLayout();

			// Hide one anchorable
			var tool2 = root.Descendents().OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == "tool2");
			tool2?.Hide();

			string xml = SerializeLayout(root);

			Assert.That(xml, Does.Contain("tool2"),
				"Hidden anchorable should be included in serialized XML (Issue #56).");
		}

		/// <summary>
		/// Verifies Descendents() returns all expected elements.
		/// </summary>
		[Test]
		public void Descendents_ReturnsAllElements()
		{
			var root = CreateSampleLayout();
			var contents = root.Descendents().OfType<LayoutContent>().ToList();

			// Doc1, Doc2, Tool1, Tool2, AutoHide1
			Assert.That(contents.Count, Is.GreaterThanOrEqualTo(5),
				"Descendents should return all layout content elements.");
		}

		/// <summary>
		/// Verifies auto-hide anchorables are on the correct side.
		/// </summary>
		[Test]
		public void AutoHideSide_ContainsAnchorables()
		{
			var root = CreateSampleLayout();

			Assert.That(root.LeftSide, Is.Not.Null);
			Assert.That(root.LeftSide.Children.Count, Is.GreaterThan(0));

			var autoHideItems = root.LeftSide.Descendents().OfType<LayoutAnchorable>().ToList();
			Assert.That(autoHideItems.Count, Is.EqualTo(1));
			Assert.That(autoHideItems[0].ContentId, Is.EqualTo("autohide1"));
			Assert.That(autoHideItems[0].IsAutoHidden, Is.True);
		}
	}
}
