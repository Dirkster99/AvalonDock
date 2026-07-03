using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using AvalonDock;
using AvalonDock.Layout;

namespace AvalonDockTest
{
	/// <summary>
	/// Regression coverage for the <see cref="DockingManager.ContentDocking"/> and
	/// <see cref="DockingManager.ContentDocked"/> events.
	/// Covers issue:
	///   #599 - ContentDocking / ContentDocked never fire when content is re-docked
	///
	/// ContentDocking (cancellable) is raised at the operation entry points before any layout mutation.
	/// ContentDocked is raised where the mutation completes: at the end of
	/// <see cref="AvalonDock.Layout.LayoutContent.Dock"/> and
	/// <see cref="AvalonDock.Layout.LayoutContent.DockAsDocument"/> (covering the commands and direct
	/// calls to the public API alike, guarded by a floating-to-docked transition check), and in the
	/// drag-and-drop <c>DropTarget.Drop</c> for contents that actually left their floating window.
	///
	/// These tests need neither a shown window nor the dispatcher message loop: an unshown
	/// <see cref="DockingManager"/> has <see cref="System.Windows.FrameworkElement.IsLoaded"/> == false,
	/// so assigning a layout performs model bookkeeping only and never realizes floating-window controls.
	/// </summary>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class DockingEventsTests
	{
		/// <summary>
		/// Docking a floating anchorable through the Dock command must raise ContentDocking and then
		/// ContentDocked, and must actually move the anchorable back into the docked layout.
		/// </summary>
		[Test]
		public void ExecuteDockCommand_FiresContentDockingThenDocked_Issue599()
		{
			var manager = new DockingManager();
			var dockedPane = new LayoutAnchorablePane();
			// A "keeper" keeps the target pane alive across CollectGarbage so the floated anchorable
			// has somewhere to return to.
			dockedPane.Children.Add(new LayoutAnchorable { Title = "Keeper", ContentId = "keeper" });

			var floating = new LayoutAnchorable { Title = "Tool", ContentId = "tool" };
			var root = BuildRootWithFloatingAnchorable(dockedPane, floating);
			manager.Layout = root;

			var dockingRaised = new List<LayoutContent>();
			var dockedRaised = new List<LayoutContent>();
			manager.ContentDocking += (s, e) => dockingRaised.Add(e.Content);
			manager.ContentDocked += (s, e) => dockedRaised.Add(e.Content);

			Assert.That(floating.IsFloating, Is.True, "Precondition: the anchorable should start floating.");

			InvokeManagerMethod(manager, "ExecuteDockCommand", floating);

			Assert.That(dockingRaised.Count, Is.EqualTo(1), "ContentDocking should fire exactly once.");
			Assert.That(dockingRaised[0], Is.SameAs(floating), "ContentDocking should report the docked content.");
			Assert.That(dockedRaised.Count, Is.EqualTo(1), "ContentDocked should fire exactly once (Issue #599).");
			Assert.That(dockedRaised[0], Is.SameAs(floating), "ContentDocked should report the docked content (Issue #599).");
			Assert.That(floating.IsFloating, Is.False, "The anchorable should no longer be floating after docking.");
			Assert.That(floating.Parent, Is.SameAs(dockedPane), "The anchorable should reattach to its previous docked pane.");
		}

		/// <summary>
		/// Cancelling the ContentDocking event must abort the operation: ContentDocked must not fire and
		/// the anchorable must stay floating.
		/// </summary>
		[Test]
		public void ExecuteDockCommand_CancelledDocking_LeavesContentFloating_Issue599()
		{
			var manager = new DockingManager();
			var dockedPane = new LayoutAnchorablePane();
			dockedPane.Children.Add(new LayoutAnchorable { Title = "Keeper", ContentId = "keeper" });

			var floating = new LayoutAnchorable { Title = "Tool", ContentId = "tool" };
			var root = BuildRootWithFloatingAnchorable(dockedPane, floating);
			var floatingPane = (LayoutAnchorablePane)floating.Parent;
			manager.Layout = root;

			var dockingRaised = new List<LayoutContent>();
			var dockedRaised = new List<LayoutContent>();
			manager.ContentDocking += (s, e) =>
			{
				e.Cancel = true;
				dockingRaised.Add(e.Content);
			};
			manager.ContentDocked += (s, e) => dockedRaised.Add(e.Content);

			InvokeManagerMethod(manager, "ExecuteDockCommand", floating);

			Assert.That(dockingRaised.Count, Is.EqualTo(1), "ContentDocking should still fire so handlers can cancel.");
			Assert.That(dockingRaised[0], Is.SameAs(floating), "ContentDocking should report the content being docked.");
			Assert.That(dockedRaised, Is.Empty, "ContentDocked must not fire when docking is cancelled.");
			Assert.That(floating.IsFloating, Is.True, "The anchorable should remain floating after a cancelled dock.");
			Assert.That(floating.Parent, Is.SameAs(floatingPane), "The anchorable should stay in its floating pane.");
		}

		/// <summary>
		/// Docking a floating document through the Dock-as-Document command must raise ContentDocking and
		/// then ContentDocked, and move the document back into a docked document pane.
		/// </summary>
		[Test]
		public void ExecuteDockAsDocumentCommand_FiresContentDockingThenDocked_Issue599()
		{
			var manager = new DockingManager();
			var dockedDocumentPane = new LayoutDocumentPane();
			dockedDocumentPane.Children.Add(new LayoutDocument { Title = "Keeper", ContentId = "keeperDoc" });

			var floating = new LayoutDocument { Title = "Doc", ContentId = "doc" };
			var root = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(dockedDocumentPane)) };

			var floatingPane = new LayoutDocumentPane();
			floatingPane.Children.Add(floating);
			root.FloatingWindows.Add(new LayoutDocumentFloatingWindow
			{
				RootPanel = new LayoutDocumentPaneGroup(floatingPane)
			});
			manager.Layout = root;

			var dockingRaised = new List<LayoutContent>();
			var dockedRaised = new List<LayoutContent>();
			manager.ContentDocking += (s, e) => dockingRaised.Add(e.Content);
			manager.ContentDocked += (s, e) => dockedRaised.Add(e.Content);

			Assert.That(floating.IsFloating, Is.True, "Precondition: the document should start floating.");

			InvokeManagerMethod(manager, "ExecuteDockAsDocumentCommand", floating);

			Assert.That(dockingRaised.Count, Is.EqualTo(1), "ContentDocking should fire exactly once for the document.");
			Assert.That(dockingRaised[0], Is.SameAs(floating), "ContentDocking should report the docked document.");
			Assert.That(dockedRaised.Count, Is.EqualTo(1), "ContentDocked should fire exactly once for the document (Issue #599).");
			Assert.That(dockedRaised[0], Is.SameAs(floating), "ContentDocked should report the docked document (Issue #599).");
			Assert.That(floating.IsFloating, Is.False, "The document should no longer be floating after docking.");
			Assert.That(floating.Parent, Is.SameAs(dockedDocumentPane),
				"The document should reattach to the remaining docked document pane.");
		}

		/// <summary>
		/// The strongly-typed <see cref="AvalonDock.Core.IDockingManager.ContentDocked"/> interface event
		/// shares the same code path and must fire alongside the public event.
		/// </summary>
		[Test]
		public void ExecuteDockCommand_RaisesCoreContentDockedEvent_Issue599()
		{
			var manager = new DockingManager();
			var core = (AvalonDock.Core.IDockingManager)manager;
			var dockedPane = new LayoutAnchorablePane();
			dockedPane.Children.Add(new LayoutAnchorable { Title = "Keeper", ContentId = "keeper" });

			var floating = new LayoutAnchorable { Title = "Tool", ContentId = "tool" };
			var root = BuildRootWithFloatingAnchorable(dockedPane, floating);
			manager.Layout = root;

			var coreDockingRaised = new List<AvalonDock.Core.Serialization.ISerializableLayoutContent>();
			var coreDockedRaised = new List<AvalonDock.Core.Serialization.ISerializableLayoutContent>();
			core.ContentDocking += (s, e) => coreDockingRaised.Add(e.Content);
			core.ContentDocked += (s, e) => coreDockedRaised.Add(e.Content);

			InvokeManagerMethod(manager, "ExecuteDockCommand", floating);

			Assert.That(coreDockingRaised.Count, Is.EqualTo(1), "Core IDockingManager.ContentDocking should fire.");
			Assert.That(coreDockingRaised[0], Is.SameAs(floating), "Core ContentDocking should report the docked content.");
			Assert.That(coreDockedRaised.Count, Is.EqualTo(1), "Core IDockingManager.ContentDocked should fire (Issue #599).");
			Assert.That(coreDockedRaised[0], Is.SameAs(floating), "Core ContentDocked should report the docked content (Issue #599).");
		}

		/// <summary>
		/// Docking through the public <see cref="LayoutContent.Dock"/> API (not the command) must also raise
		/// ContentDocked, since <see cref="LayoutContent.Dock"/> itself raises it on a floating-to-docked
		/// transition. ContentDocking is not raised here: the app invoked the operation directly, so there
		/// is nothing to cancel.
		/// </summary>
		[Test]
		public void PublicDockApi_FiresContentDocked_Issue599()
		{
			var manager = new DockingManager();
			var dockedPane = new LayoutAnchorablePane();
			dockedPane.Children.Add(new LayoutAnchorable { Title = "Keeper", ContentId = "keeper" });

			var floating = new LayoutAnchorable { Title = "Tool", ContentId = "tool" };
			var root = BuildRootWithFloatingAnchorable(dockedPane, floating);
			manager.Layout = root;

			var dockingRaised = new List<LayoutContent>();
			var dockedRaised = new List<LayoutContent>();
			manager.ContentDocking += (s, e) => dockingRaised.Add(e.Content);
			manager.ContentDocked += (s, e) => dockedRaised.Add(e.Content);

			floating.Dock();

			Assert.That(dockingRaised, Is.Empty,
				"ContentDocking is an entry-point event; a direct Dock() call has nothing to cancel.");
			Assert.That(dockedRaised.Count, Is.EqualTo(1), "ContentDocked should fire for a direct Dock() call (Issue #599).");
			Assert.That(dockedRaised[0], Is.SameAs(floating), "ContentDocked should report the docked content.");
			Assert.That(floating.Parent, Is.SameAs(dockedPane), "The anchorable should reattach to its previous docked pane.");
		}

		/// <summary>
		/// Builds a layout root with <paramref name="dockedPane"/> hosted in the main layout and
		/// <paramref name="floating"/> hosted in a floating window whose previous container is
		/// <paramref name="dockedPane"/>, so that docking reattaches it there.
		/// </summary>
		private static LayoutRoot BuildRootWithFloatingAnchorable(LayoutAnchorablePane dockedPane, LayoutAnchorable floating)
		{
			var rootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()));
			rootPanel.Children.Add(dockedPane);
			var root = new LayoutRoot { RootPanel = rootPanel };

			((ILayoutPreviousContainer)floating).PreviousContainer = dockedPane;
			floating.PreviousContainerIndex = dockedPane.Children.Count;
			root.FloatingWindows.Add(new LayoutAnchorableFloatingWindow
			{
				RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(floating))
			});

			return root;
		}

		/// <summary>
		/// Invokes an internal <see cref="DockingManager"/> command by name; these are the entry points
		/// the context-menu Dock commands use and are not part of the public surface.
		/// </summary>
		private static void InvokeManagerMethod(DockingManager manager, string methodName, LayoutContent content)
		{
			var method = typeof(DockingManager).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.That(method, Is.Not.Null, $"Expected internal method {methodName} on DockingManager.");
			method.Invoke(manager, new object[] { content });
		}
	}
}
