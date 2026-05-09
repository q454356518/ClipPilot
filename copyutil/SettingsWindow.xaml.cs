using copyutil.Models;
using System.Windows;

namespace copyutil;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        ClipboardMonitorCheckBox.IsChecked = settings.EnableClipboardMonitor;
        DragCopyCheckBox.IsChecked = settings.EnableDragCopy;
        SuccessTipCheckBox.IsChecked = settings.ShowSuccessTip;
    }

    public AppSettings Settings => _settings;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.EnableClipboardMonitor = ClipboardMonitorCheckBox.IsChecked == true;
        _settings.EnableDragCopy = DragCopyCheckBox.IsChecked == true;
        _settings.ShowSuccessTip = SuccessTipCheckBox.IsChecked == true;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
