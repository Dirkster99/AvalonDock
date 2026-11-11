namespace AvalonDockTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Threading;

	using NUnit.Framework;

	using AvalonDock;
	using AvalonDock.Controls;
	using AvalonDock.Core;
	using AvalonDock.Layout;
	using AvalonDock.Mvvm;
	using AvalonDockTest.TestHelpers;

	/// <summary>
	/// Tests that hiding a toolbox and restoring it from the hidden panels menu of a
	/// <see cref="ToggleDockingManager"/> keeps exactly one sidebar button per anchorable (issue #689).
	/// </summary>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class ToggleDockHideRestoreTests : AutomationTestBase
	{
		/// <summary>A toolbox view model that can be placed in any zone.</summary>
		private sealed class TestToolbox : ToolboxBase
		{
			/// <summary>Initializes a new instance of the <see cref="TestToolbox"/> class.</summary>
			/// <param name="id">The content id, also used as the title.</param>
			/// <param name="zone">The zone to place the toolbox in.</param>
			public TestToolbox(string id, DockZone zone)
			{
				Id = id;
				Title = id;
				Zone = zone;
			}
		}

		/// <summary>
		/// Hiding runs through the anchorable's HideCommand - the path "View Mode > Hidden" and the pane
		/// header take - and has to drop the sidebar button, not only the sidebar button's own "Hide" item.
		/// </summary>
		[Test]
		public void HideCommand_RemovesTheSidebarButton()
		{
			var toolbox = new TestToolbox("explorer", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, toolbox);

			try
			{
				window.Show();
				DoEvents();

				var anchorable = FindAnchorable(manager, toolbox);
				Assert.That(CountButtons(manager, anchorable), Is.EqualTo(1));

				Hide(manager, anchorable);

				Assert.That(anchorable.IsHidden, Is.True);
				Assert.That(CountButtons(manager, anchorable), Is.EqualTo(0),
					"A hidden anchorable may not keep its sidebar button.");
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// The regression from issue #689: every hide/restore cycle used to add another sidebar button.
		/// </summary>
		[Test]
		public void RepeatedHideAndRestore_KeepsASingleSidebarButton()
		{
			var toolbox = new TestToolbox("explorer", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, toolbox);

			try
			{
				window.Show();
				DoEvents();

				var anchorable = FindAnchorable(manager, toolbox);

				for (int cycle = 1; cycle <= 3; cycle++)
				{
					Hide(manager, anchorable);
					Restore(manager, anchorable);

					Assert.That(anchorable.IsHidden, Is.False);
					Assert.That(CountButtons(manager, anchorable), Is.EqualTo(1),
						$"Exactly one sidebar button has to exist after hide/restore cycle {cycle}.");
				}
			}
			finally
			{
				window.Close();
			}
		}

		/// <summary>
		/// Restoring has to be safe even when a button for the anchorable is still present, for example
		/// because the anchorable was hidden by code that bypassed the manager.
		/// </summary>
		[Test]
		public void RestoreWithStaleButton_DoesNotDuplicateIt()
		{
			var toolbox = new TestToolbox("explorer", DockZone.LeftTop) { IsOpenByDefault = true };
			var window = CreateHostingWindow(out var manager, toolbox);

			try
			{
				window.Show();
				DoEvents();

				var anchorable = FindAnchorable(manager, toolbox);

				// Hide straight on the model, which the manager does not intercept.
				anchorable.HideAnchorable(true);
				DoEvents();
				Assert.That(anchorable.IsHidden, Is.True);

				Restore(manager, anchorable);

				Assert.That(CountButtons(manager, anchorable), Is.EqualTo(1));
			}
			finally
			{
				window.Close();
			}
		}

		private static void Hide(ToggleDockingManager manager, LayoutAnchorable anchorable)
		{
			var layoutItem = manager.GetLayoutItemFromModel(anchorable) as LayoutAnchorableItem;
			Assert.That(layoutItem, Is.Not.Null);
			layoutItem.HideCommand.Execute(null);
			DoEvents();
		}

		private static void Restore(ToggleDockingManager manager, LayoutAnchorable anchorable)
		{
			var method = typeof(ToggleDockingManager).GetMethod(
				"RestoreHiddenAnchorable", BindingFlags.Instance | BindingFlags.NonPublic);

			Assert.That(method, Is.Not.Null, "RestoreHiddenAnchorable backs the hidden panels menu.");
			method.Invoke(manager, new object[] { anchorable });
			DoEvents();
		}

		private static int CountButtons(ToggleDockingManager manager, LayoutAnchorable anchorable)
		{
			var barFields = typeof(ToggleDockingManager)
				.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
				.Where(f => f.FieldType == typeof(ToggleDockButtonBar));

			int count = 0;
			foreach (var field in barFields)
			{
				if (field.GetValue(manager) is ToggleDockButtonBar bar)
				{
					count += bar.Items.OfType<ToggleDockButton>().Count(b => b.Anchorable == anchorable);
				}
			}

			return count;
		}

		private static LayoutAnchorable FindAnchorable(ToggleDockingManager manager, TestToolbox toolbox)
		{
			return manager.Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.First(a => ReferenceEquals(a.Content, toolbox));
		}

		private static Window CreateHostingWindow(out ToggleDockingManager manager, params TestToolbox[] toolboxes)
		{
			manager = new ToggleDockingManager
			{
				Layout = BuildLayout(toolboxes),
			};

			return new Window
			{
				Width = 800,
				Height = 600,
				ShowInTaskbar = false,
				ShowActivated = false,
				WindowStartupLocation = WindowStartupLocation.Manual,
				Left = -10000,
				Top = -10000,
				Content = manager,
			};
		}

		private static LayoutRoot BuildLayout(IEnumerable<TestToolbox> toolboxes)
		{
			var layout = new LayoutRoot
			{
				RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())),
			};

			foreach (var toolbox in toolboxes)
			{
				var group = new LayoutAnchorGroup();
				group.Children.Add(new LayoutAnchorable
				{
					Title = toolbox.Title,
					ContentId = toolbox.Id,
					Content = toolbox,
				});

				layout.LeftSide.Children.Add(group);
			}

			return layout;
		}

		private static void DoEvents()
		{
			var frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(
				DispatcherPriority.Background,
				new Action(() => frame.Continue = false));
			Dispatcher.PushFrame(frame);
		}
	}
}
