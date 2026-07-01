using System;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;

namespace AvalonDockTest.FlaUITests
{
	/// <summary>
	/// UI automation tests verifying that the keyboard shortcuts declared via
	/// <see cref="AvalonDock.Core.IToolbox.Shortcut"/> toggle their toolbox, exactly
	/// like clicking the corresponding sidebar button.
	/// </summary>
	[TestFixture]
	[Category("FlaUI")]
	[Category("ToggleDock")]
	public class ToggleDockShortcutTests : ToggleDockFlaUITestBase
	{
		/// <summary>
		/// Pressing the Explorer shortcut (Ctrl+Shift+E) docks it; pressing it again hides it.
		/// </summary>
		[Test]
		public void Shortcut_TogglesExplorer_OnAndOff()
		{
			// Explorer starts auto-hidden (only Terminal is open by default).
			Assert.That(
				WaitForToggleState("Explorer", ToggleState.Off),
				Is.True,
				"Explorer should start unchecked.");

			SendChord(VirtualKeyShort.KEY_E, VirtualKeyShort.CONTROL, VirtualKeyShort.SHIFT);
			Assert.That(
				WaitForToggleState("Explorer", ToggleState.On),
				Is.True,
				"Explorer should be docked after pressing Ctrl+Shift+E.");

			SendChord(VirtualKeyShort.KEY_E, VirtualKeyShort.CONTROL, VirtualKeyShort.SHIFT);
			Assert.That(
				WaitForToggleState("Explorer", ToggleState.Off),
				Is.True,
				"Explorer should be hidden after pressing Ctrl+Shift+E a second time.");
		}

		/// <summary>
		/// A shortcut only affects its own toolbox: toggling Search (Ctrl+Shift+F) does not
		/// dock Explorer.
		/// </summary>
		[Test]
		public void Shortcut_TogglesOnlyTargetToolbox()
		{
			Assert.That(WaitForToggleState("Search", ToggleState.Off), Is.True,
				"Search should start unchecked.");

			SendChord(VirtualKeyShort.KEY_F, VirtualKeyShort.CONTROL, VirtualKeyShort.SHIFT);

			Assert.That(WaitForToggleState("Search", ToggleState.On), Is.True,
				"Search should be docked after pressing Ctrl+Shift+F.");
			Assert.That(GetToggleState(FindToggleButton("Explorer")), Is.EqualTo(ToggleState.Off),
				"Explorer should remain hidden when only the Search shortcut is pressed.");

			// Clean up: hide Search again.
			SendChord(VirtualKeyShort.KEY_F, VirtualKeyShort.CONTROL, VirtualKeyShort.SHIFT);
			WaitForToggleState("Search", ToggleState.Off);
		}

		/// <summary>
		/// Sends a keyboard chord (modifiers held while the main key is typed).
		/// </summary>
		/// <param name="key">The main key.</param>
		/// <param name="modifiers">The modifier keys held down.</param>
		private void SendChord(VirtualKeyShort key, params VirtualKeyShort[] modifiers)
		{
			MainWindow.SetForeground();
			Wait.UntilInputIsProcessed();

			foreach (var modifier in modifiers)
			{
				Keyboard.Press(modifier);
			}

			Keyboard.Type(key);

			for (int i = modifiers.Length - 1; i >= 0; i--)
			{
				Keyboard.Release(modifiers[i]);
			}

			Wait.UntilInputIsProcessed();
		}

		/// <summary>
		/// Polls the toggle state of a sidebar button until it reaches the expected state.
		/// </summary>
		/// <param name="name">The toolbox/button name.</param>
		/// <param name="expected">The expected toggle state.</param>
		/// <returns><c>true</c> if the state was reached within the timeout.</returns>
		private bool WaitForToggleState(string name, ToggleState expected)
		{
			return Retry.WhileFalse(
				() => GetToggleState(FindToggleButton(name)) == expected,
				timeout: TimeSpan.FromSeconds(5),
				interval: TimeSpan.FromMilliseconds(250)).Result;
		}
	}
}
