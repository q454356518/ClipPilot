using System.Collections.Specialized;
using System.Windows;

namespace copyutil.Services;

public sealed class ClipboardWriteService
{
    public bool IsInternalWrite { get; private set; }

    public bool SetText(string text)
    {
        return WriteClipboard(() => System.Windows.Clipboard.SetText(text));
    }

    public bool SetFiles(IEnumerable<string> paths)
    {
        var files = new StringCollection();
        foreach (var path in paths)
        {
            files.Add(path);
        }

        return files.Count > 0 && WriteClipboard(() => System.Windows.Clipboard.SetFileDropList(files));
    }

    private bool WriteClipboard(Action action)
    {
        try
        {
            IsInternalWrite = true;
            action();
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _ = Task.Delay(300).ContinueWith(_ => IsInternalWrite = false, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
