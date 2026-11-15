namespace AvalonDockTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;

	using NUnit.Framework;

	using AvalonDock.Controls;
	using AvalonDock.Themes;
	using AvalonDock.Themes.VS;

	/// <summary>
	/// Regression tests for #684: the implicit <see cref="MenuItemEx"/> style must set
	/// <see cref="Control.HorizontalContentAlignment"/> and <see cref="Control.VerticalContentAlignment"/>
	/// explicitly. Otherwise the default <see cref="MenuItem"/> style binds them to an
	/// <see cref="ItemsControl"/> ancestor that generated document list containers do not have yet,
	/// which produces binding errors when the document overflow menu is opened.
	/// </summary>
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class MenuItemExContentAlignmentTests
	{
		private static IEnumerable<TestCaseData> Themes()
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

		[TestCaseSource(nameof(Themes))]
		public void MenuItemExStyle_SetsContentAlignmentExplicitly(Func<DictionaryTheme> createTheme)
		{
			var style = createTheme().ThemeResourceDictionary[typeof(MenuItemEx)] as Style;
			Assert.That(style, Is.Not.Null);

			var setters = style.Setters.OfType<Setter>().ToList();
			Assert.That(setters.SingleOrDefault(s => s.Property == Control.HorizontalContentAlignmentProperty)?.Value,
				Is.EqualTo(HorizontalAlignment.Left));
			Assert.That(setters.SingleOrDefault(s => s.Property == Control.VerticalContentAlignmentProperty)?.Value,
				Is.EqualTo(VerticalAlignment.Center));
		}
	}
}
