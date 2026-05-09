namespace copyutil.Models;

public sealed class AppSettings
{
    public double WindowLeft { get; set; } = 120;
    public double WindowTop { get; set; } = 120;
    public bool EnableClipboardMonitor { get; set; } = true;
    public bool EnableDragCopy { get; set; } = true;
    public bool ShowSuccessTip { get; set; } = true;
}
