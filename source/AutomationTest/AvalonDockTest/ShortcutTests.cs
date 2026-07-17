using System.ComponentModel;
using AvalonDock.Core;
using AvalonDock.Mvvm;
using AvalonDock.Mvvm.CommunityToolkit;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Unit tests for the <see cref="IToolbox.Shortcut"/> MVVM contract and its
	/// implementations on the supplied toolbox base classes.
	/// </summary>
	[TestFixture]
	public class ShortcutTests
	{
		private sealed class TestToolbox : ToolboxBase
		{
			public TestToolbox()
			{
				Id = "test";
				Title = "Test";
			}
		}

		private sealed class TestObservableToolbox : ObservableToolboxBase
		{
			public TestObservableToolbox()
			{
				Id = "test-observable";
				Title = "Test Observable";
			}
		}

		[Test]
		public void IToolbox_HasShortcutProperty()
		{
			var prop = typeof(IToolbox).GetProperty(nameof(IToolbox.Shortcut));
			Assert.That(prop, Is.Not.Null);
			Assert.That(prop!.PropertyType, Is.EqualTo(typeof(string)));
			Assert.That(prop.GetMethod, Is.Not.Null);
			Assert.That(prop.SetMethod, Is.Not.Null);
		}

		[Test]
		public void Shortcut_DefaultsToNull()
		{
			Assert.That(new TestToolbox().Shortcut, Is.Null);
			Assert.That(new TestObservableToolbox().Shortcut, Is.Null);
		}

		[Test]
		public void ToolboxBase_Shortcut_RoundTrips()
		{
			var toolbox = new TestToolbox { Shortcut = "Ctrl+Shift+E" };
			Assert.That(toolbox.Shortcut, Is.EqualTo("Ctrl+Shift+E"));
		}

		[Test]
		public void ObservableToolboxBase_Shortcut_RoundTrips()
		{
			var toolbox = new TestObservableToolbox { Shortcut = "Ctrl+OemTilde" };
			Assert.That(toolbox.Shortcut, Is.EqualTo("Ctrl+OemTilde"));
		}

		[Test]
		public void ToolboxBase_Shortcut_RaisesPropertyChanged()
		{
			var toolbox = new TestToolbox();
			string changed = null;
			((INotifyPropertyChanged)toolbox).PropertyChanged += (_, e) => changed = e.PropertyName;

			toolbox.Shortcut = "Ctrl+Shift+F";

			Assert.That(changed, Is.EqualTo(nameof(IToolbox.Shortcut)));
		}

		[Test]
		public void ObservableToolboxBase_Shortcut_RaisesPropertyChanged()
		{
			var toolbox = new TestObservableToolbox();
			string changed = null;
			((INotifyPropertyChanged)toolbox).PropertyChanged += (_, e) => changed = e.PropertyName;

			toolbox.Shortcut = "Ctrl+Shift+F";

			Assert.That(changed, Is.EqualTo(nameof(IToolbox.Shortcut)));
		}

		[Test]
		public void Shortcut_AssignableThroughInterface()
		{
			IToolbox toolbox = new TestToolbox();
			toolbox.Shortcut = "Ctrl+Shift+G";
			Assert.That(toolbox.Shortcut, Is.EqualTo("Ctrl+Shift+G"));
		}
	}
}
