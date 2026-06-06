using System;
using System.IO;
using System.Windows.Media;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToggleTestApp.ViewModels;

public partial class EditorTabViewModel : ObservableDocument
{
	[ObservableProperty] private string _toolTip = string.Empty;

	[ObservableProperty] private string _filePath = string.Empty;

	[ObservableProperty] private string _content = string.Empty;

	[ObservableProperty] private string _syntaxHighlighting = "Text";

	[ObservableProperty] private ImageSource? _iconSource;

	public string ContentId => Id;

	public void LoadFile(string path)
	{
		FilePath = path;
		Title = Path.GetFileName(path);
		Id = path;
		ToolTip = path;
		IconSource = FileIconHelper.GetIconForExtension(Path.GetExtension(path));
		SyntaxHighlighting = GetHighlightingForExtension(Path.GetExtension(path));

		try
		{
			if (IsBinaryFile(path))
			{
				Content = $"[Binary file — {new FileInfo(path).Length:N0} bytes]";
			}
			else
			{
				Content = File.ReadAllText(path);
			}

			IsModified = false;
		}
		catch (Exception ex)
		{
			Content = $"Error loading file: {ex.Message}";
		}
	}

	private static bool IsBinaryFile(string path)
	{
		try
		{
			var buffer = new byte[8192];
			using var stream = File.OpenRead(path);
			var bytesRead = stream.Read(buffer, 0, buffer.Length);
			for (int i = 0; i < bytesRead; i++)
			{
				if (buffer[i] == 0) return true;
			}

			return false;
		}
		catch
		{
			return false;
		}
	}

	private static string GetHighlightingForExtension(string ext)
	{
		return ext.ToLowerInvariant() switch
		{
			".cs" => "C#",
			".xml" or ".xaml" or ".csproj" or ".sln" or ".config" or ".props" => "XML",
			".js" => "JavaScript",
			".html" or ".htm" => "HTML",
			".css" => "CSS",
			".json" => "JSON",
			".py" => "Python",
			".md" => "MarkDown",
			".sql" => "TSQL",
			".cpp" or ".c" or ".h" => "C++",
			_ => "Text"
		};
	}
}

/// <summary>
/// Provides file extension icons as DrawingImage for MVVM binding to ImageSource.
/// </summary>
public static class FileIconHelper
{
	public static ImageSource? GetIconForExtension(string ext)
	{
		var color = GetColorForExtension(ext);
		var glyph = GetGlyphForExtension(ext);
		return CreateTextIcon(glyph, color);
	}

	private static ImageSource CreateTextIcon(string text, Color color)
	{
		var formatted = new FormattedText(
			text,
			System.Globalization.CultureInfo.InvariantCulture,
			System.Windows.FlowDirection.LeftToRight,
			new Typeface("Segoe MDL2 Assets"),
			12,
			new SolidColorBrush(color),
			1.0);

		var drawing = new GeometryDrawing(
			new SolidColorBrush(color),
			null,
			formatted.BuildGeometry(new System.Windows.Point(0, 0)));

		var image = new DrawingImage(drawing);
		image.Freeze();
		return image;
	}

	private static string GetGlyphForExtension(string ext)
	{
		return ext.ToLowerInvariant() switch
		{
			".cs" => "\uE943", // Code
			".xaml" or ".xml" => "\uE9D5", // FileExplorer
			".json" => "\uE9D5",
			".csproj" or ".sln" or ".props" => "\uE90F", // Settings
			".md" => "\uE8A5", // Document
			".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".ico" => "\uEB9F", // Photo
			_ => "\uE8A5" // Document
		};
	}

	private static Color GetColorForExtension(string ext)
	{
		return ext.ToLowerInvariant() switch
		{
			".cs" => Color.FromRgb(0x6A, 0x9F, 0x55), // Green for C#
			".xaml" => Color.FromRgb(0x56, 0x9C, 0xD6), // Blue for XAML
			".xml" or ".config" => Color.FromRgb(0xD4, 0x9C, 0x56), // Orange for XML
			".json" => Color.FromRgb(0xCE, 0x91, 0x78), // Salmon for JSON
			".csproj" or ".sln" or ".props" => Color.FromRgb(0xB8, 0x86, 0xDB), // Purple for project
			".md" => Color.FromRgb(0x56, 0x9C, 0xD6), // Blue for Markdown
			".js" => Color.FromRgb(0xDC, 0xDC, 0x8B), // Yellow for JS
			".css" => Color.FromRgb(0x56, 0x9C, 0xD6), // Blue for CSS
			".html" or ".htm" => Color.FromRgb(0xD4, 0x6B, 0x56), // Red-ish for HTML
			_ => Color.FromRgb(0xCC, 0xCC, 0xCC) // Gray default
		};
	}
}