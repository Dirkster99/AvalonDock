using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToggleTestApp.ViewModels;

public partial class EditorTabViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Untitled";

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private string _syntaxHighlighting = "Text";

    public string ContentId => FilePath;

    public void LoadFile(string path)
    {
        try
        {
            FilePath = path;
            Title = Path.GetFileName(path);
            Content = File.ReadAllText(path);
            SyntaxHighlighting = GetHighlightingForExtension(Path.GetExtension(path));
            IsModified = false;
        }
        catch (Exception ex)
        {
            Content = $"Error loading file: {ex.Message}";
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
