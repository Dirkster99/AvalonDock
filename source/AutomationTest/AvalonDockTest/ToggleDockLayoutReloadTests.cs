using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDockTest.TestHelpers;
using NUnit.Framework;
using UnitTests;

namespace AvalonDockTest
{
	/// <summary>
	/// Regression tests for reloading a stored layout into a <see cref="ToggleDockingManager"/>.
	/// Replacing the layout swaps in a new set of <see cref="LayoutAnchorable"/> instances, so the
	/// sidebar buttons have to be rebuilt from it. Without that the buttons of the previous layout
	/// survive as ghost toolboxes - dead buttons for tool windows the new layout does not contain.
	/// </summary>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class ToggleDockLayoutReloadTests : AutomationTestBase
	{
		/// <summary>The sidebar bar fields of the manager, one per dock zone.</summary>
		private static readonly string[] BarFieldNames =
		{
			"_leftTopBar", "_leftBottomBar", "_rightTopBar",
			"_rightBottomBar", "_bottomLeftBar", "_bottomRightBar",
		};

		/// <summary>A window hosting nothing but a <see cref="ToggleDockingManager"/>.</summary>
		public class ToggleDockTestWindow : Window
		{
			/// <summary>Initializes a new instance of the <see cref="ToggleDockTestWindow"/> class.</summary>
			public ToggleDockTestWindow()
			{
				Width = 800;
				Height = 600;
				Manager = new ToggleDockingManager();
				Content = Manager;
			}

			/// <summary>Gets the hosted manager.</summary>
			public ToggleDockingManager Manager { get; }
		}

		/// <summary>
		/// Builds a layout whose anchorables all sit auto-hidden on the left side, which is the state
		/// the toggle docking manager keeps them in while their sidebar button is not toggled on.
		/// </summary>
		/// <param name="contentIds">The content ids of the anchorables to create.</param>
		/// <returns>The layout root.</returns>
		private static LayoutRoot BuildLayout(params string[] contentIds)
		{
			var layout = new LayoutRoot
			{
				RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())),
			};

			var group = new LayoutAnchorGroup();
			foreach (var contentId in contentIds)
			{
				group.Children.Add(new LayoutAnchorable
				{
					Title = contentId,
					ContentId = contentId,
					Content = new object(),
				});
			}

			layout.LeftSide.Children.Add(group);
			return layout;
		}

		/// <summary>
		/// Reads the sidebar buttons of the manager. The bars are internal fields; their items are
		/// filled directly, so no layout pass is needed to observe them.
		/// </summary>
		/// <param name="manager">The manager to inspect.</param>
		/// <returns>The sidebar buttons of all six zones.</returns>
		private static List<ToggleDockButton> SidebarButtons(ToggleDockingManager manager)
		{
			var buttons = new List<ToggleDockButton>();
			foreach (var fieldName in BarFieldNames)
			{
				var field = typeof(ToggleDockingManager).GetField(
					fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

				if (field?.GetValue(manager) is ToggleDockButtonBar bar)
					buttons.AddRange(bar.Items.OfType<ToggleDockButton>());
			}

			return buttons;
		}

		/// <summary>
		/// Loading a stored layout that no longer contains a tool window must not leave its sidebar
		/// button behind: the bars are rebuilt from the layout that was just applied.
		/// </summary>
		[Test]
		public void ReplacingTheLayout_RebuildsTheSidebarWithoutGhostButtons()
		{
			List<string> titlesAfterReload = null;
			var ghostButtons = 0;

			ThreadExecutor.RunCodeAsSTA(
				_are,
				() =>
				{
					var task = WindowHelpers.CreateInvisibleWindowAsync<ToggleDockTestWindow>();
					task.Wait();
					var manager = task.Result.Manager;

					// The layout the user saved, back when the application still offered "tool1".
					manager.Layout = BuildLayout("tool1", "tool2");
					var stale = manager.Layout.Descendents().OfType<LayoutAnchorable>().ToList();

					// The application of today: "tool1" does not exist any more.
					var reloaded = BuildLayout("tool2");
					manager.Layout = reloaded;

					var buttons = SidebarButtons(manager);
					titlesAfterReload = buttons.Select(b => b.Anchorable?.Title).ToList();
					ghostButtons = buttons.Count(b => stale.Contains(b.Anchorable));
				});

			_are.WaitOne();

			Assert.That(ghostButtons, Is.Zero,
				"No sidebar button may still reference an anchorable of the replaced layout.");
			Assert.That(titlesAfterReload, Is.EqualTo(new[] { "tool2" }),
				"The sidebar has to show exactly the tool windows of the layout that was loaded.");
		}

		/// <summary>
		/// A tool window that the reloaded layout does contain keeps its button, so reloading is not
		/// simply clearing the sidebar.
		/// </summary>
		[Test]
		public void ReplacingTheLayout_KeepsButtonsOfTheAnchorablesItContains()
		{
			List<string> titlesAfterReload = null;

			ThreadExecutor.RunCodeAsSTA(
				_are,
				() =>
				{
					var task = WindowHelpers.CreateInvisibleWindowAsync<ToggleDockTestWindow>();
					task.Wait();
					var manager = task.Result.Manager;

					manager.Layout = BuildLayout("tool1");
					manager.Layout = BuildLayout("tool1", "tool2", "tool3");

					titlesAfterReload = SidebarButtons(manager)
						.Select(b => b.Anchorable?.Title)
						.ToList();
				});

			_are.WaitOne();

			Assert.That(titlesAfterReload, Is.EquivalentTo(new[] { "tool1", "tool2", "tool3" }));
		}

		/// <summary>
		/// Every sidebar button has to point at an anchorable of the current layout, so that toggling
		/// it actually reaches the layout tree the manager is showing.
		/// </summary>
		[Test]
		public void ReplacingTheLayout_LeavesEveryButtonPointingIntoTheCurrentLayout()
		{
			var orphanedButtons = 0;

			ThreadExecutor.RunCodeAsSTA(
				_are,
				() =>
				{
					var task = WindowHelpers.CreateInvisibleWindowAsync<ToggleDockTestWindow>();
					task.Wait();
					var manager = task.Result.Manager;

					manager.Layout = BuildLayout("tool1", "tool2");
					manager.Layout = BuildLayout("tool3");

					var current = manager.Layout;
					orphanedButtons = SidebarButtons(manager)
						.Count(b => b.Anchorable == null || b.Anchorable.Root != current);
				});

			_are.WaitOne();

			Assert.That(orphanedButtons, Is.Zero);
		}
	}
}
