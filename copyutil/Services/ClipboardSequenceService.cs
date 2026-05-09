using System.Runtime.InteropServices;

namespace copyutil.Services;

public sealed class ClipboardSequenceService
{
    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    public uint GetCurrent() => GetClipboardSequenceNumber();
}
