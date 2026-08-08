using System.Linq;
using System.Threading;
using AvalonDock.Layout;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Covers <see cref="LayoutAnchorable.Show"/> for the case where the pane the anchorable was
	/// hidden from is no longer part of the layout.
	/// </summary>
	/// <remarks>
	/// A hidden tool window finds its way back through PreviousContainer, and CollectGarbage keeps an
	/// emptied pane alive as long as a hidden anchorable still points at it. That leaves two ways for
	/// the reference to go bad: the pane gets detached from the tree - a layout restored from file
	/// replaces the docked area wholesale - or CollectGarbage nulls the reference once the pane is
	/// disconnected. Show used to walk past both of its branches in that state and return having done
	/// nothing: no move, no exception, and a tool window that stayed invisible. A host loading tool
	/// windows from plugins meets this every time content turns up after the layout was restored.
	/// </remarks>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class AnchorableShowFallbackTests
	{
		/// <summary>Builds a layout with a document pane and one tool window in its own pane.</summary>
		/// <param name="anchorable">The tool window of the layout.</param>
		/// <param name="toolPaneGroup">The group holding the pane of that tool window.</param>
		/// <returns>The layout root.</returns>
		private static LayoutRoot BuildLayout(
			out LayoutAnchorable anchorable,
			out LayoutAnchorablePaneGroup toolPaneGroup)
		{
			anchorable = new LayoutAnchorable { Title = "Explorer", ContentId = "explorer" };
			toolPaneGroup = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(anchorable));

			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(toolPaneGroup);

			return new LayoutRoot { RootPanel = panel };
		}

		/// <summary>The pane is still there, so the anchorable goes back exactly where it came from.</summary>
		[Test]
		public void Show_ReturnsToItsOwnPaneAtItsOwnIndex()
		{
			var layout = BuildLayout(out var anchorable, out _);
			var pane = (LayoutAnchorablePane)anchorable.Parent;
			pane.Children.Add(new LayoutAnchorable { Title = "Output", ContentId = "output" });
			pane.Children.Add(new LayoutAnchorable { Title = "Errors", ContentId = "errors" });

			// The one in the middle, so a restore that appends shows up as a wrong index.
			var middle = pane.Children[1];
			middle.Hide();
			layout.CollectGarbage();

			Assert.That(middle.IsHidden, Is.True, "Hide did not park the anchorable.");

			middle.Show();

			Assert.Multiple(() =>
			{
				Assert.That(middle.IsVisible, Is.True, "Show left the anchorable hidden.");
				Assert.That(middle.Parent, Is.SameAs(pane), "Show did not use the original pane.");
				Assert.That(pane.Children.IndexOf(middle), Is.EqualTo(1), "Show lost the position.");
				Assert.That(layout.Hidden, Does.Not.Contain(middle));
			});
		}

		/// <summary>
		/// The remembered pane was taken out of the tree while the anchorable was hidden. Show has to
		/// place it in the docked area instead of doing nothing.
		/// </summary>
		[Test]
		public void Show_FallsBackToAnotherPane_WhenItsOwnPaneWasDetached()
		{
			var layout = BuildLayout(out var anchorable, out var toolPaneGroup);

			var survivingPane = new LayoutAnchorablePane(
				new LayoutAnchorable { Title = "Output", ContentId = "output" });
			layout.RootPanel.Children.Add(survivingPane);

			anchorable.Hide();

			// What a layout restore does to the docked area: the old branch is dropped, and the pane
			// the hidden anchorable remembers goes with it.
			layout.RootPanel.Children.Remove(toolPaneGroup);

			var remembered = ((ILayoutPreviousContainer)anchorable).PreviousContainer;
			Assert.Multiple(() =>
			{
				Assert.That(remembered, Is.Not.Null, "The test needs a stale reference.");
				Assert.That(
					((LayoutAnchorablePane)remembered).Root,
					Is.Null,
					"The remembered pane was expected to be off the tree.");
			});

			anchorable.Show();

			Assert.Multiple(() =>
			{
				Assert.That(anchorable.IsVisible, Is.True, "Show left the anchorable hidden.");
				Assert.That(anchorable.Parent, Is.SameAs(survivingPane));
				Assert.That(layout.Hidden, Does.Not.Contain(anchorable));
			});
		}

		/// <summary>
		/// Nothing remembers where the anchorable belongs - the state a restored layout leaves a tool
		/// window in whose content only turns up later. Show still has to make it visible.
		/// </summary>
		[Test]
		public void Show_FallsBackToAnotherPane_WhenNothingIsRemembered()
		{
			var anchorable = new LayoutAnchorable { Title = "Plugin Tool", ContentId = "plugin" };
			var pane = new LayoutAnchorablePane(
				new LayoutAnchorable { Title = "Output", ContentId = "output" });
			var panel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			panel.Children.Add(new LayoutAnchorablePaneGroup(pane));

			var layout = new LayoutRoot { RootPanel = panel };
			layout.Hidden.Add(anchorable);

			Assert.Multiple(() =>
			{
				Assert.That(anchorable.IsHidden, Is.True);
				Assert.That(
					((ILayoutPreviousContainer)anchorable).PreviousContainer,
					Is.Null,
					"The test needs an unremembered pane.");
			});

			anchorable.Show();

			Assert.Multiple(() =>
			{
				Assert.That(anchorable.IsVisible, Is.True, "Show left the anchorable hidden.");
				Assert.That(anchorable.Parent, Is.SameAs(pane));
				Assert.That(layout.Hidden, Does.Not.Contain(anchorable));
			});
		}

		/// <summary>A layout without a single tool window pane gets one rather than swallowing the call.</summary>
		[Test]
		public void Show_CreatesAPane_WhenTheLayoutHasNone()
		{
			var anchorable = new LayoutAnchorable { Title = "Plugin Tool", ContentId = "plugin" };
			var layout = new LayoutRoot
			{
				RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())),
			};
			layout.Hidden.Add(anchorable);

			anchorable.Show();

			Assert.Multiple(() =>
			{
				Assert.That(anchorable.IsVisible, Is.True, "Show left the anchorable hidden.");
				Assert.That(anchorable.Parent, Is.InstanceOf<LayoutAnchorablePane>());
				Assert.That(
					layout.Descendents().OfType<LayoutAnchorable>(),
					Does.Contain(anchorable),
					"The anchorable was not attached to the layout.");
				Assert.That(layout.Hidden, Does.Not.Contain(anchorable));
			});
		}

		/// <summary>Showing something that is already visible stays the no-op it always was.</summary>
		[Test]
		public void Show_DoesNothing_WhenTheAnchorableIsAlreadyVisible()
		{
			var layout = BuildLayout(out var anchorable, out _);
			var pane = anchorable.Parent;

			anchorable.Show();

			Assert.Multiple(() =>
			{
				Assert.That(anchorable.Parent, Is.SameAs(pane), "Show moved a visible anchorable.");
				Assert.That(layout.Descendents().OfType<LayoutAnchorablePane>().Count(), Is.EqualTo(1));
			});
		}
	}
}
