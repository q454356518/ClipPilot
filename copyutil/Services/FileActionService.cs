using copyutil.Models;
using System.Diagnostics;
using System.IO;

namespace copyutil.Services;

public sealed class FileActionService
{
    public bool Open(PathItem item)
    {
        if (item.Type == PathItemType.File && !File.Exists(item.Path))
        {
            return false;
        }

        if (item.Type == PathItemType.Directory && !Directory.Exists(item.Path))
        {
            return false;
        }

        return Start(new ProcessStartInfo(item.Path) { UseShellExecute = true });
    }

    public bool OpenDirectory(PathItem item)
    {
        if (item.Type == PathItemType.Directory)
        {
            return Directory.Exists(item.Path) && Start(new ProcessStartInfo(item.Path) { UseShellExecute = true });
        }

        if (!File.Exists(item.Path))
        {
            return false;
        }

        return Start(new ProcessStartInfo("explorer.exe", $"/select,\"{item.Path}\"") { UseShellExecute = true });
    }

    private static bool Start(ProcessStartInfo info)
    {
        try
        {
            Process.Start(info);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
