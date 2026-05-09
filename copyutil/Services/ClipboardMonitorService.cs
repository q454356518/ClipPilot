using copyutil.Models;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace copyutil.Services;

public sealed class ClipboardMonitorService
{
    private const int WmClipboardUpdate = 0x031D;

    private readonly PathDetectService _pathDetectService;
    private readonly ClipboardWriteService _clipboardWriteService;
    private readonly ClipboardImageService _clipboardImageService;
    private readonly ClipboardSequenceService _clipboardSequenceService;
    private HwndSource? _source;
    private IntPtr _windowHandle;
    private uint _lastSequenceNumber;

    public ClipboardMonitorService(
        PathDetectService pathDetectService,
        ClipboardWriteService clipboardWriteService,
        ClipboardImageService clipboardImageService,
        ClipboardSequenceService clipboardSequenceService)
    {
        _pathDetectService = pathDetectService;
        _clipboardWriteService = clipboardWriteService;
        _clipboardImageService = clipboardImageService;
        _clipboardSequenceService = clipboardSequenceService;
        _lastSequenceNumber = _clipboardSequenceService.GetCurrent();
    }

    public event Action<IReadOnlyList<PathItem>>? PathsDetected;

    public void Start(Window window)
    {
        _windowHandle = new WindowInteropHelper(window).Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(WndProc);
        AddClipboardFormatListener(_windowHandle);
        _lastSequenceNumber = _clipboardSequenceService.GetCurrent();
    }

    public void Stop()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            RemoveClipboardFormatListener(_windowHandle);
        }

        _source?.RemoveHook(WndProc);
        _source = null;
        _windowHandle = IntPtr.Zero;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmClipboardUpdate)
        {
            handled = false;
            ProcessClipboardUpdate();
        }

        return IntPtr.Zero;
    }

    private void ProcessClipboardUpdate()
    {
        var currentSequenceNumber = _clipboardSequenceService.GetCurrent();
        if (currentSequenceNumber == _lastSequenceNumber)
        {
            return;
        }

        _lastSequenceNumber = currentSequenceNumber;
        if (_clipboardWriteService.IsInternalWrite)
        {
            return;
        }

        try
        {
            var fileItems = GetFileDropItems();
            if (fileItems.Count > 0)
            {
                PathsDetected?.Invoke(fileItems);
                return;
            }

            if (System.Windows.Clipboard.ContainsImage())
            {
                var imageItem = _clipboardImageService.SaveClipboardImage();
                if (imageItem is not null)
                {
                    PathsDetected?.Invoke(new[] { imageItem });
                    return;
                }
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                var items = _pathDetectService.Detect(System.Windows.Clipboard.GetText());
                if (items.Count > 0)
                {
                    PathsDetected?.Invoke(items);
                }
            }
        }
        catch
        {
        }
    }

    private static IReadOnlyList<PathItem> GetFileDropItems()
    {
        if (!System.Windows.Clipboard.ContainsFileDropList())
        {
            return Array.Empty<PathItem>();
        }

        var files = System.Windows.Clipboard.GetFileDropList();
        var items = new List<PathItem>();
        foreach (var path in files.Cast<string>())
        {
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
}
