namespace copyutil.Models;

public sealed class PathItem
{
    public PathItem(string path, PathItemType type, bool isGeneratedFromClipboard = false)
    {
        Path = path;
        Type = type;
        IsGeneratedFromClipboard = isGeneratedFromClipboard;
    }

    public string Path { get; }
    public PathItemType Type { get; }
    public bool IsGeneratedFromClipboard { get; }
    public string Name => System.IO.Path.GetFileName(Path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
}
