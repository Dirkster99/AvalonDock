using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
	/// <summary>
	/// UI automation tests for restoring a stored layout into a <c>ToggleDockingManager</c>.
	/// </summary>
	/// <remarks>
	/// The sidebar buttons are built from the layout, so replacing the layout has to rebuild them.
	/// Without that the buttons keep pointing at the anchorables of the replaced tree: they survive
	/// as ghost toolboxes that cannot be toggled, and a tool window the stored layout no longer
	/// contains keeps its button. These tests drive the real serializer through the application menu,
	/// which is the path an application takes rather than the model level the unit tests cover.
	/// </remarks>
	[TestFixture]
	[Category("FlaUI")]
	[Category("ToggleDock")]
	public class ToggleDockSerializationTests : ToggleDockFlaUITestBase
	{
		/// <summary>Title of the tool window carried only by the "unknown toolbox" layout.</summary>
		private const string UnknownToolboxTitle = "Ghost Toolbox";

		/// <summary>A toolbox of the sample application, used to prove the buttons still work.</summary>
		private const string KnownToolbox = "Explorer";

		/// <summary>Reads the titles of every sidebar button currently shown.</summary>
		/// <returns>The button titles.</returns>
		private string[] SidebarButtonTitles() =>
			FindAllToggleButtonNames()
				.OrderBy(n => n, StringComparer.Ordinal)
				.ToArray();

		/// <summary>Saves the current layout through the application menu.</summary>
		private void SaveLayout()
		{
			ClickMenuPath("Layout", "Save Layout");
			System.Threading.Thread.Sleep(500);
		}

		/// <summary>Loads the stored layout through the application menu.</summary>
		private void LoadLayout()
		{
			ClickMenuPath("Layout", "Load Layout");
			WaitForLayoutSettled();
		}

		/// <summary>Waits for the sidebar to be rebuilt after a layout change.</summary>
		private void WaitForLayoutSettled()
		{
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(1500);
		}

		/// <summary>
		/// Restoring a layout must leave the sidebar showing exactly the tool windows it contains -
		/// no button lost, and no button duplicated by a second set built on top of the first.
		/// </summary>
		[Test]
		[Order(1)]
		public void SaveAndLoadLayout_KeepsExactlyTheSameSidebarButtons()
		{
			var before = SidebarButtonTitles();
			Assert.That(before, Is.Not.Empty, "The sample application should show sidebar buttons.");

			SaveLayout();
			LoadLayout();

			var after = SidebarButtonTitles();

			Assert.That(after, Is.EqualTo(before),
				"Restoring the layout it was saved from has to reproduce the same sidebar.");
		}

		/// <summary>
		/// The buttons that survive a restore have to point into the layout that was just applied.
		/// A button left over from the replaced tree still looks right but no longer toggles
		/// anything, so the check is behavioural rather than visual.
		/// </summary>
		[Test]
		[Order(2)]
		public void AfterLoadingLayout_TheSidebarButtonsStillToggleTheirToolWindow()
		{
			SaveLayout();
			LoadLayout();

			var button = FindToggleButton(KnownToolbox);
			Assert.That(button, Is.Not.Null,
				$"'{KnownToolbox}' should still have a sidebar button after the restore.");

			var before = GetToggleState(button);

			button.Click();
			Wait.UntilInputIsProcessed();
			System.Threading.Thread.Sleep(800);

			var toggled = Retry.WhileFalse(
				() =>
				{
					var current = FindToggleButton(KnownToolbox);
					return current != null && GetToggleState(current) != before;
				},
				timeout: TimeSpan.FromSeconds(5),
				interval: TimeSpan.FromMilliseconds(250));

			Assert.That(toggled.Result, Is.True,
				$"Clicking the '{KnownToolbox}' button after a layout restore has to toggle it, which "
				+ "it cannot do while it still references the replaced layout.");
		}

		/// <summary>
		/// A stored layout can name a tool window this version of the application does not offer any
		/// more. It must not appear in the sidebar, because its button could only ever open an empty
		/// pane.
		/// </summary>
		[Test]
		[Order(3)]
		public void LoadingLayoutWithUnknownToolbox_AddsNoGhostButton()
		{
			SaveLayout();

			var expected = SidebarButtonTitles();

			ClickMenuPath("Layout", "Load Layout With Unknown Toolbox");
			WaitForLayoutSettled();

			var after = SidebarButtonTitles();

			Assert.That(after, Does.Not.Contain(UnknownToolboxTitle),
				"A stored tool window without content must not reach the sidebar.");
			Assert.That(after, Is.EqualTo(expected),
				"The tool windows the application does offer have to survive that restore unchanged.");
			Assert.That(App.HasExited, Is.False,
				"Restoring a layout with an unknown tool window must not bring the application down.");
		}

		/// <summary>
		/// Restoring repeatedly must be stable: each restore replaces the sidebar rather than adding
		/// to it, so the buttons cannot accumulate across cycles.
		/// </summary>
		[Test]
		[Order(4)]
		public void LoadingLayoutRepeatedly_DoesNotAccumulateSidebarButtons()
		{
			SaveLayout();

			LoadLayout();
			var afterFirst = SidebarButtonTitles();

			LoadLayout();
			LoadLayout();
			var afterThird = SidebarButtonTitles();

			Assert.That(afterThird, Is.EqualTo(afterFirst),
				"Three restores have to leave the same sidebar as one.");
		}
	}
}
