using copyutil.Models;
using System.IO;

namespace copyutil.Services;

public sealed class PathDetectService
{
    public IReadOnlyList<PathItem> Detect(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<PathItem>();
        }

        var items = new List<PathItem>();
        var lines = text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            var path = Normalize(line);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (File.Exists(path))
            {
                items.Add(new PathItem(path, PathItemType.File));
            }
            else if (Directory.Exists(path))
            {
                items.Add(new PathItem(path, PathItemType.Directory));
            }
        }

        return items;
    }

    private static string Normalize(string value)
    {
        var path = value.Trim()
            .Trim('"', '\'', '“', '”', '‘', '’')
            .Trim();

        if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                path = uri.LocalPath;
            }
        }

        return path.Replace('/', Path.DirectorySeparatorChar);
    }
}
