using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.Layout;
using AvalonDock.Serializer.Xml;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Regression tests for ghost tool windows: entries of a stored layout whose content cannot be
	/// resolved any more, because the application does not offer that toolbox in this version.
	/// They must never end up as an empty pane or an empty tab carrying nothing but a stored title.
	/// </summary>
	/// <remarks>
	/// These tests need neither a shown window nor a dispatcher message loop: an unshown
	/// <see cref="DockingManager"/> has <see cref="System.Windows.FrameworkElement.IsLoaded"/> == false,
	/// so assigning a layout performs model bookkeeping only.
	/// </remarks>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class GhostAnchorableTests
	{
		/// <summary>
		/// A layout stored while the application still offered "tool1" and "tool2".
		/// </summary>
		/// <returns>The serialized layout.</returns>
		private static string StoredLayout()
		{
			var manager = new DockingManager();

			var toolPane = new LayoutAnchorablePane();
			toolPane.Children.Add(new LayoutAnchorable { Title = "Tool 1", ContentId = "tool1" });
			toolPane.Children.Add(new LayoutAnchorable { Title = "Tool 2", ContentId = "tool2" });

			var documentPane = new LayoutDocumentPane();
			documentPane.Children.Add(new LayoutDocument { Title = "Doc 1", ContentId = "doc1" });

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(documentPane));
			panel.Children.Add(new LayoutAnchorablePaneGroup(toolPane));

			manager.Layout = new LayoutRoot { RootPanel = panel };

			using var stream = new MemoryStream();
			new XmlLayoutSerializer(manager).Serialize(stream);
			return Encoding.UTF8.GetString(stream.ToArray());
		}

		/// <summary>Restores the stored layout into a fresh manager.</summary>
		/// <param name="xml">The stored layout.</param>
		/// <param name="attachContent">
		/// Callback used as the <c>LayoutSerializationCallback</c> handler, or <see langword="null"/>
		/// to deserialize without one.
		/// </param>
		/// <returns>The manager holding the restored layout.</returns>
		private static DockingManager Restore(string xml, System.Action<LayoutSerializationCallbackEventArgs> attachContent)
		{
			var manager = new DockingManager();
			var serializer = new XmlLayoutSerializer(manager);

			if (attachContent != null)
				serializer.LayoutSerializationCallback += (s, e) => attachContent(e);

			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
			serializer.Deserialize(stream);
			return manager;
		}

		/// <summary>Finds an anchorable that is part of the visible layout.</summary>
		/// <param name="manager">The manager to search.</param>
		/// <param name="contentId">The content id to look for.</param>
		/// <returns>The anchorable, or <see langword="null"/>.</returns>
		private static LayoutAnchorable VisibleAnchorable(DockingManager manager, string contentId) =>
			manager.Layout.Descendents().OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId && !a.IsHidden);

		/// <summary>
		/// A callback that ignores an unknown content id - the common case for an application that
		/// simply does not know the toolbox any more - must not leave an empty pane behind. The
		/// anchorable is hidden instead, so it stays recoverable but is out of the visible layout.
		/// </summary>
		[Test]
		public void Deserialize_WithCallbackThatSuppliesNoContent_HidesTheAnchorable()
		{
			var manager = Restore(StoredLayout(), e => { /* the toolbox does not exist any more */ });

			Assert.That(VisibleAnchorable(manager, "tool1"), Is.Null,
				"An anchorable whose content could not be restored must not stay in the visible layout.");
			Assert.That(manager.Layout.Hidden.Any(a => a.ContentId == "tool1"), Is.True,
				"It should be hidden rather than dropped, so it can come back once its content exists again.");
		}

		/// <summary>
		/// The same for documents: an unresolved document is closed instead of being shown as an
		/// empty tab carrying nothing but its stored title.
		/// </summary>
		[Test]
		public void Deserialize_WithCallbackThatSuppliesNoContent_ClosesTheDocument()
		{
			var manager = Restore(StoredLayout(), e => { });

			Assert.That(manager.Layout.Descendents().OfType<LayoutDocument>().Any(d => d.ContentId == "doc1"),
				Is.False,
				"A document whose content could not be restored must not stay in the layout.");
		}

		/// <summary>
		/// A callback may attach the content to the model instead of to
		/// <see cref="LayoutSerializationCallbackEventArgs.Content"/>. That anchorable is fully
		/// restored and has to stay exactly where the stored layout put it.
		/// </summary>
		[Test]
		public void Deserialize_WithCallbackThatSetsContentOnTheModel_KeepsTheAnchorableVisible()
		{
			var content = new object();
			var manager = Restore(StoredLayout(), e =>
			{
				if (e.Model.ContentId == "tool1")
					e.Model.Content = content;
			});

			var restored = VisibleAnchorable(manager, "tool1");
			Assert.That(restored, Is.Not.Null,
				"An anchorable whose content the callback attached to the model must stay visible.");
			Assert.That(restored.Content, Is.SameAs(content));
		}

		/// <summary>
		/// The callback wins over the previous state: cancelling closes the anchorable outright.
		/// </summary>
		[Test]
		public void Deserialize_WithCancellingCallback_RemovesTheAnchorableCompletely()
		{
			var manager = Restore(StoredLayout(), e => e.Cancel = true);

			Assert.That(VisibleAnchorable(manager, "tool1"), Is.Null);
			Assert.That(manager.Layout.Hidden.Any(a => a.ContentId == "tool1"), Is.False,
				"A cancelled anchorable is closed, not hidden.");
		}

		/// <summary>
		/// Without a callback the anchorables of the stored layout cannot be matched against anything
		/// in a fresh manager, so none of them stays in the visible layout.
		/// </summary>
		[Test]
		public void Deserialize_WithoutCallback_HidesUnmatchedAnchorables()
		{
			var manager = Restore(StoredLayout(), null);

			Assert.That(VisibleAnchorable(manager, "tool1"), Is.Null);
			Assert.That(VisibleAnchorable(manager, "tool2"), Is.Null);
			Assert.That(manager.Layout.Hidden.Select(a => a.ContentId),
				Is.EquivalentTo(new[] { "tool1", "tool2" }));
		}
	}
}
