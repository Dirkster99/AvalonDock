using System.Collections.Generic;
using System.Windows.Media;
using AvalonDock.Themes.VS;
using NUnit.Framework;

namespace AvalonDockTest
{
	[TestFixture]
	public class VsJsonThemeParserTests
	{
		// The override sample from the VS blog "Make Visual Studio look the way you want".
		private const string CoolBreezeJson = @"[
  { ""Name"": ""EnvironmentHeader"", ""Category"": ""5af241b7-5627-4d12-bfb1-2b67d11127d7"", ""Background"": ""FFF5CC84"" },
  { ""Name"": ""EnvironmentTab"", ""Category"": ""5af241b7-5627-4d12-bfb1-2b67d11127d7"", ""Background"": ""FFF5CC84"" },
  { ""Name"": ""EnvironmentBody"", ""Category"": ""5af241b7-5627-4d12-bfb1-2b67d11127d7"", ""Background"": ""FF5D6B99"" },
  { ""Name"": ""EnvironmentBodyText"", ""Category"": ""5af241b7-5627-4d12-bfb1-2b67d11127d7"", ""Background"": ""E4FFFFFF"" },
  { ""Name"": ""EnvironmentBackground"", ""Category"": ""5af241b7-5627-4d12-bfb1-2b67d11127d7"", ""Background"": ""FFCCD5F0"" },
  { ""Name"": ""StatusBarBackgroundFillRest"", ""Category"": ""5af241b7-5627-4d12-bfb1-2b67d11127d7"", ""Background"": ""FF40508D"" }
]";

		[Test]
		public void Parse_ReadsBackgroundColors()
		{
			var palette = VsJsonThemeParser.Parse(CoolBreezeJson);

			Assert.That(palette.GetBackground("EnvironmentHeader"), Is.EqualTo(Color.FromArgb(0xFF, 0xF5, 0xCC, 0x84)));
			Assert.That(palette.GetBackground("EnvironmentBackground"), Is.EqualTo(Color.FromArgb(0xFF, 0xCC, 0xD5, 0xF0)));
		}

		[Test]
		public void Parse_HonorsAlphaChannel()
		{
			var palette = VsJsonThemeParser.Parse(CoolBreezeJson);

			Assert.That(palette.GetBackground("EnvironmentBodyText"), Is.EqualTo(Color.FromArgb(0xE4, 0xFF, 0xFF, 0xFF)));
		}

		[Test]
		public void Parse_MissingTokenReturnsNull()
		{
			var palette = VsJsonThemeParser.Parse(CoolBreezeJson);

			Assert.That(palette.GetBackground("NonExistent"), Is.Null);
		}

		[Test]
		public void Parse_EntryWithoutBackgroundHasNullBackground()
		{
			var palette = VsJsonThemeParser.Parse(@"[ { ""Name"": ""Foo"", ""Category"": ""x"" } ]");

			Assert.That(palette.Contains("Foo"), Is.True);
			Assert.That(palette.GetBackground("Foo"), Is.Null);
		}

		[Test]
		public void Parse_IsCaseInsensitiveOnPropertyNames()
		{
			var palette = VsJsonThemeParser.Parse(@"[ { ""name"": ""Foo"", ""background"": ""FF112233"" } ]");

			Assert.That(palette.GetBackground("Foo"), Is.EqualTo(Color.FromArgb(0xFF, 0x11, 0x22, 0x33)));
		}

		[Test]
		public void Merge_OverridesReplaceBaseEntries()
		{
			var basePalette = MakePalette(
				("EnvironmentBackground", Color.FromArgb(0xFF, 0x1E, 0x1E, 0x1E)),
				("FileTabSelectedBorder", Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC)));

			var overrides = VsJsonThemeParser.Parse(CoolBreezeJson);
			var merged = basePalette.Merge(overrides);

			// Overridden token takes the new value.
			Assert.That(merged.GetBackground("EnvironmentBackground"), Is.EqualTo(Color.FromArgb(0xFF, 0xCC, 0xD5, 0xF0)));
			// Untouched base token is preserved.
			Assert.That(merged.GetBackground("FileTabSelectedBorder"), Is.EqualTo(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC)));
			// New token introduced by the override is added.
			Assert.That(merged.GetBackground("EnvironmentHeader"), Is.EqualTo(Color.FromArgb(0xFF, 0xF5, 0xCC, 0x84)));
		}

		[Test]
		public void Merge_PreservesBaseForegroundWhenOverrideOnlySetsBackground()
		{
			var baseColors = new Dictionary<string, VsThemeColorEntry>
			{
				["TitleBarActiveText"] = new VsThemeColorEntry(
					Color.FromArgb(0xFF, 0x00, 0x00, 0x00),
					Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),
			};
			var basePalette = new VsThemeColorPalette(baseColors);

			var overrides = VsJsonThemeParser.Parse(@"[ { ""Name"": ""TitleBarActiveText"", ""Background"": ""FF112233"" } ]");
			var merged = basePalette.Merge(overrides);

			Assert.That(merged.GetBackground("TitleBarActiveText"), Is.EqualTo(Color.FromArgb(0xFF, 0x11, 0x22, 0x33)));
			Assert.That(merged.GetForeground("TitleBarActiveText"), Is.EqualTo(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)));
		}

		[Test]
		public void Merge_DoesNotModifyInputs()
		{
			var basePalette = MakePalette(("EnvironmentBackground", Color.FromArgb(0xFF, 0x1E, 0x1E, 0x1E)));
			var overrides = MakePalette(("EnvironmentBackground", Color.FromArgb(0xFF, 0xCC, 0xD5, 0xF0)));

			basePalette.Merge(overrides);

			Assert.That(basePalette.GetBackground("EnvironmentBackground"), Is.EqualTo(Color.FromArgb(0xFF, 0x1E, 0x1E, 0x1E)));
			Assert.That(basePalette.Count, Is.EqualTo(1));
		}

		private static VsThemeColorPalette MakePalette(params (string Name, Color Background)[] entries)
		{
			var colors = new Dictionary<string, VsThemeColorEntry>();
			foreach (var (name, background) in entries)
			{
				colors[name] = new VsThemeColorEntry(background, null);
			}

			return new VsThemeColorPalette(colors);
		}
	}
}
