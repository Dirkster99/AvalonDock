using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Media;
using AvalonDock.Themes.VS;
using NUnit.Framework;

namespace AvalonDockTest
{
	[TestFixture]
	public class VsThemeParserTests
	{
		private const string MinimalDarkTheme = @"<Themes>
  <Theme Name=""Dark"" GUID=""{1ded0138-47ce-435e-84ef-9ec1f439b749}"">
    <Category Name=""Environment"" GUID=""{624ed9c3-bdfd-41fa-96c3-7c824ea32e3d}"">
      <Color Name=""EnvironmentBackground"">
        <Background Type=""CT_RAW"" Source=""FF2D2D30"" />
      </Color>
      <Color Name=""FileTabSelectedBorder"">
        <Background Type=""CT_RAW"" Source=""FF007ACC"" />
      </Color>
      <Color Name=""TitleBarActiveText"">
        <Background Type=""CT_RAW"" Source=""FFFFFFFF"" />
        <Foreground Type=""CT_RAW"" Source=""FF000000"" />
      </Color>
      <Color Name=""AutoHideTabText"">
        <Background Type=""CT_AUTOMATIC"" Source=""000000FF"" />
      </Color>
    </Category>
  </Theme>
</Themes>";

		[Test]
		public void Parse_ReadsEnvironmentBackground()
		{
			var palette = ParseString(MinimalDarkTheme);

			var bg = palette.GetBackground("EnvironmentBackground");

			Assert.That(bg, Is.Not.Null);
			Assert.That(bg.Value, Is.EqualTo(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x30)));
		}

		[Test]
		public void Parse_ReadsAccentColor()
		{
			var palette = ParseString(MinimalDarkTheme);

			var accent = palette.GetBackground("FileTabSelectedBorder");

			Assert.That(accent, Is.Not.Null);
			Assert.That(accent.Value, Is.EqualTo(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC)));
		}

		[Test]
		public void Parse_ReadsForegroundSeparately()
		{
			var palette = ParseString(MinimalDarkTheme);

			var bg = palette.GetBackground("TitleBarActiveText");
			var fg = palette.GetForeground("TitleBarActiveText");

			Assert.That(bg, Is.EqualTo(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)));
			Assert.That(fg, Is.EqualTo(Color.FromArgb(0xFF, 0x00, 0x00, 0x00)));
		}

		[Test]
		public void Parse_IgnoresCtAutomatic()
		{
			var palette = ParseString(MinimalDarkTheme);

			var bg = palette.GetBackground("AutoHideTabText");

			Assert.That(bg, Is.Null);
		}

		[Test]
		public void Parse_MissingColorReturnsNull()
		{
			var palette = ParseString(MinimalDarkTheme);

			Assert.That(palette.GetBackground("NonExistent"), Is.Null);
		}

		[Test]
		public void Parse_EmptyThemeReturnsEmptyPalette()
		{
			var palette = ParseString("<Themes><Theme Name=\"Empty\"><Category Name=\"Other\"></Category></Theme></Themes>");

			Assert.That(palette.Count, Is.EqualTo(0));
		}

		[Test]
		public void GetBackgroundOrDefault_ReturnsFallbackWhenMissing()
		{
			var palette = ParseString(MinimalDarkTheme);
			var fallback = Color.FromRgb(0xAA, 0xBB, 0xCC);

			var result = palette.GetBackgroundOrDefault("NonExistent", fallback);

			Assert.That(result, Is.EqualTo(fallback));
		}

		[Test]
		public void Contains_ReturnsTrueForExistingToken()
		{
			var palette = ParseString(MinimalDarkTheme);

			Assert.That(palette.Contains("EnvironmentBackground"), Is.True);
			Assert.That(palette.Contains("NonExistent"), Is.False);
		}

		[Test]
		public void ParseGZip_ProducesSamePaletteAsPlainText()
		{
			var plain = ParseString(MinimalDarkTheme);
			var gz = VsThemeParser.ParseGZip(GZip(MinimalDarkTheme));

			Assert.That(gz.Count, Is.EqualTo(plain.Count));
			Assert.That(gz.GetBackground("EnvironmentBackground"), Is.EqualTo(plain.GetBackground("EnvironmentBackground")));
			Assert.That(gz.GetBackground("FileTabSelectedBorder"), Is.EqualTo(plain.GetBackground("FileTabSelectedBorder")));
			Assert.That(gz.GetForeground("TitleBarActiveText"), Is.EqualTo(plain.GetForeground("TitleBarActiveText")));
		}

		[Test]
		public void Parse_AutoDetectsGZipStream()
		{
			using var stream = new MemoryStream(GZip(MinimalDarkTheme));

			var palette = VsThemeParser.Parse(stream);

			Assert.That(palette.GetBackground("EnvironmentBackground"), Is.EqualTo(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x30)));
		}

		private static byte[] GZip(string xml)
		{
			using var ms = new MemoryStream();
			using (var gz = new GZipStream(ms, CompressionMode.Compress))
			{
				var bytes = Encoding.UTF8.GetBytes(xml);
				gz.Write(bytes, 0, bytes.Length);
			}

			return ms.ToArray();
		}

		private static VsThemeColorPalette ParseString(string xml)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
			return VsThemeParser.Parse(stream);
		}
	}
}