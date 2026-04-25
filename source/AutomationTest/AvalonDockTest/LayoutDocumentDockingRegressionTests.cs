using System.Linq;
using System.Reflection;
using NUnit.Framework;
using AvalonDock.Layout;

namespace AvalonDockTest
{
	/// <summary>
	/// Regression coverage for document float/dock transitions.
	/// Covers issues:
	///   #551 - Document stays floating after float -> dock -> float -> Dock as Tabbed Document
	///   #450 - Empty document panes should still be removed while referenced by floating content
	/// </summary>
	[TestFixture]
	public class LayoutDocumentDockingRegressionTests
	{
		private static readonly FieldInfo PreviousContainerField =
			typeof(LayoutContent).GetField("_previousContainer", BindingFlags.Instance | BindingFlags.NonPublic);

		[Test]
		public void CollectGarbage_RemovesEmptyMainDocumentPaneReferencedByFloatingDocument_Issue450()
		{
			var floatedDocument = new LayoutDocument { Title = "Floated", ContentId = "floated" };
			var remainingDocument = new LayoutDocument { Title = "Remaining", ContentId = "remaining" };
			var emptiedPane = new LayoutDocumentPane();
			var remainingPane = new LayoutDocumentPane();
			var paneGroup = new LayoutDocumentPaneGroup();
			var root = new LayoutRoot { RootPanel = new LayoutPanel(paneGroup) };

			emptiedPane.Children.Add(floatedDocument);
			remainingPane.Children.Add(remainingDocument);
			paneGroup.Children.Add(emptiedPane);
			paneGroup.Children.Add(remainingPane);

			MoveDocumentToFloatingWindow(root, floatedDocument, emptiedPane);

			root.CollectGarbage();

			Assert.That(emptiedPane.Parent, Is.Null,
				"The emptied main-window pane should be removed after the document is floated (Issue #450).");
			Assert.That(GetPreviousContainer(floatedDocument), Is.Null,
				"Floating content should not keep a stale PreviousContainer reference to a removed pane.");
			Assert.That(
				root.Descendents().OfType<LayoutDocumentPane>().Count(p => p.FindParent<LayoutDocumentFloatingWindow>() == null),
				Is.EqualTo(1),
				"Only the remaining docked document pane should stay in the main layout.");
		}

		[Test]
		public void DockAsDocument_UsesDockedPaneWhenLastFocusedDocumentIsFloating_Issue551()
		{
			var hostDocument = new LayoutDocument { Title = "Host", ContentId = "host" };
			var floatingDocument = new LayoutDocument { Title = "Floating", ContentId = "floating" };
			var hostPane = new LayoutDocumentPane();
			var transientPane = new LayoutDocumentPane();
			var paneGroup = new LayoutDocumentPaneGroup();
			var root = new LayoutRoot { RootPanel = new LayoutPanel(paneGroup) };

			hostPane.Children.Add(hostDocument);
			transientPane.Children.Add(floatingDocument);
			paneGroup.Children.Add(hostPane);
			paneGroup.Children.Add(transientPane);

			MoveDocumentToFloatingWindow(root, floatingDocument, transientPane);
			floatingDocument.IsActive = true;

			root.CollectGarbage();

			Assert.That(transientPane.Parent, Is.Null,
				"The temporary pane should be removed after the document is floated again.");
			Assert.That(GetPreviousContainer(floatingDocument), Is.Null,
				"This reproduces the post-#450 state where DockAsDocument must find another target.");

			floatingDocument.DockAsDocument();

			Assert.That(floatingDocument.FindParent<LayoutDocumentFloatingWindow>(), Is.Null,
				"Dock as Tabbed Document should move the document out of the floating window (Issue #551).");
			Assert.That(floatingDocument.Parent, Is.SameAs(hostPane),
				"The document should reattach to the remaining docked document pane.");
			Assert.That(hostPane.Children.Contains(floatingDocument), Is.True);
			Assert.That(hostPane.Children.Count, Is.EqualTo(2),
				"The host pane should contain both the original document and the re-docked document.");
		}

		private static void MoveDocumentToFloatingWindow(LayoutRoot root, LayoutDocument document, LayoutDocumentPane previousPane)
		{
			SetPreviousContainer(document, previousPane);
			document.PreviousContainerIndex = previousPane.IndexOf(document);
			previousPane.RemoveChild(document);

			var floatingPane = new LayoutDocumentPane();
			floatingPane.Children.Add(document);
			root.FloatingWindows.Add(new LayoutDocumentFloatingWindow
			{
				RootPanel = new LayoutDocumentPaneGroup(floatingPane)
			});
		}

		private static ILayoutContainer GetPreviousContainer(LayoutContent content)
			=> PreviousContainerField?.GetValue(content) as ILayoutContainer;

		private static void SetPreviousContainer(LayoutContent content, ILayoutContainer previousContainer)
			=> PreviousContainerField?.SetValue(content, previousContainer);
	}
}
