namespace AvalonDockTest
{
	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;

	using NUnit.Framework;

	using AvalonDock;
	using AvalonDock.Controls;
	using AvalonDock.Themes;
	using AvalonDock.Themes.VS;

	/// <summary>
	/// Regression tests for #683: every VS theme must provide the <see cref="ToggleDockingManager"/>
	/// specific styles and must not depend on resources from other theme assemblies.
	/// </summary>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class VsThemeToggleDockingManagerTests
	{
		private static IEnumerable<TestCaseData> VsThemes()
		{
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2015BlueTheme())).SetName("VS2015Blue");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2015DarkTheme())).SetName("VS2015Dark");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2015LightTheme())).SetName("VS2015Light");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2022BlueTheme())).SetName("VS2022Blue");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2022DarkTheme())).SetName("VS2022Dark");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2022LightTheme())).SetName("VS2022Light");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2026BlueTheme())).SetName("VS2026Blue");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2026DarkTheme())).SetName("VS2026Dark");
			yield return new TestCaseData((Func<DictionaryTheme>)(() => new VS2026LightTheme())).SetName("VS2026Light");
		}

		[TestCaseSource(nameof(VsThemes))]
		public void Theme_ProvidesToggleDockingManagerStyles(Func<DictionaryTheme> createTheme)
		{
			var dict = createTheme().ThemeResourceDictionary;

			Assert.That(dict["ToggleAnchorablePaneControlStyle"], Is.InstanceOf<Style>());
			Assert.That(dict[typeof(ToggleAnchorablePaneTitle)], Is.InstanceOf<Style>());
			Assert.That(dict[typeof(ToggleDockButton)], Is.InstanceOf<Style>());
			Assert.That(dict[typeof(ToggleDockingManager)], Is.InstanceOf<Style>());
		}

		[TestCaseSource(nameof(VsThemes))]
		public void ToggleAnchorablePaneTitle_TemplateResolvesAllResources(Func<DictionaryTheme> createTheme)
		{
			var host = new Border();
			host.Resources.MergedDictionaries.Add(createTheme().ThemeResourceDictionary);
			var title = new ToggleAnchorablePaneTitle();
			host.Child = title;

			Assert.DoesNotThrow(() => title.ApplyTemplate());
			Assert.That(title.Template, Is.Not.Null);
			Assert.That(title.Template.FindName("PART_MinimizeButton", title), Is.InstanceOf<Button>());
		}
	}
}
