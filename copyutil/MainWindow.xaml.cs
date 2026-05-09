using copyutil.Models;
using copyutil.Services;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace copyutil;

public partial class MainWindow : Window
{
    private readonly PathDetectService _pathDetectService = new();
    private readonly ClipboardWriteService _clipboardWriteService = new();
    private readonly FileActionService _fileActionService = new();
    private readonly ClipboardImageService _clipboardImageService = new();
    private readonly ClipboardSequenceService _clipboardSequenceService = new();
    private readonly WindowStateService _windowStateService = new();
    private readonly ClipboardMonitorService _clipboardMonitorService;
    private readonly TrayService _trayService;
    private AppSettings _settings;
    private bool _clipboardMonitorStarted;
    private readonly DispatcherTimer _statusTimer = new() { Interval = TimeSpan.FromMilliseconds(1500) };
    private IReadOnlyList<PathItem> _currentItems = Array.Empty<PathItem>();
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();

        _settings = _windowStateService.Load();
        _windowStateService.Apply(this);
        _clipboardMonitorService = new ClipboardMonitorService(_pathDetectService, _clipboardWriteService, _clipboardImageService, _clipboardSequenceService);
        _clipboardMonitorService.PathsDetected += ShowDetectedPaths;
        Loaded += (_, _) => ApplyClipboardMonitorSetting();
        _statusTimer.Tick += (_, _) => HideStatusTip();
        _trayService = new TrayService(this);
    }

    private PathItem? CurrentItem => _currentItems.Count > 0 ? _currentItems[0] : null;

    public void ShowSettings()
    {
        var settingsWindow = new SettingsWindow(_settings) { Owner = this };
        if (settingsWindow.ShowDialog() == true)
        {
            _settings = settingsWindow.Settings;
            _windowStateService.Save(_settings);
            ApplyClipboardMonitorSetting();
            ShowStatus("设置已保存", true);
        }
    }

    private void ApplyClipboardMonitorSetting()
    {
        if (_settings.EnableClipboardMonitor && !_clipboardMonitorStarted)
        {
            _clipboardMonitorService.Start(this);
            _clipboardMonitorStarted = true;
        }
        else if (!_settings.EnableClipboardMonitor && _clipboardMonitorStarted)
        {
            _clipboardMonitorService.Stop();
            _clipboardMonitorStarted = false;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
            _windowStateService.Save(this);
        }
    }

    private void MascotImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && _currentItems.Count > 0)
        {
            e.Handled = true;
            BubblePanel.Visibility = BubblePanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            return;
        }

        Window_MouseLeftButtonDown(sender, e);
    }

    private void Window_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        if (!_settings.EnableDragCopy || !e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            e.Effects = System.Windows.DragDropEffects.None;
            return;
        }

        DragTip.Visibility = Visibility.Visible;
        StatusText.Text = "松开复制路径";
        e.Effects = System.Windows.DragDropEffects.Copy;
    }

    private void Window_DragLeave(object sender, System.Windows.DragEventArgs e)
    {
        ResetDragState();
    }

    private void Window_Drop(object sender, System.Windows.DragEventArgs e)
    {
        ResetDragState();
        if (!_settings.EnableDragCopy || !e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            return;
        }

        var paths = (string[]?)e.Data.GetData(System.Windows.DataFormats.FileDrop);
        if (paths is null || paths.Length == 0)
        {
            return;
        }

        var text = string.Join(Environment.NewLine, paths);
        ShowStatus(_clipboardWriteService.SetText(text) ? "已复制路径" : "复制失败");
        var items = paths
            .Where(path => File.Exists(path) || Directory.Exists(path))
            .Select(path => new PathItem(path, File.Exists(path) ? PathItemType.File : PathItemType.Directory))
            .ToList();
        if (items.Count > 0)
        {
            ShowDetectedPaths(items);
        }
    }

    private void CopyFileButton_Click(object sender, RoutedEventArgs e)
    {
        var files = _currentItems.Where(item => item.Type == PathItemType.File).Select(item => item.Path).ToList();
        if (files.Count == 0)
        {
            ShowStatus("没有可复制的文件");
            return;
        }

        ShowStatus(_clipboardWriteService.SetFiles(files) ? "已复制文件" : "复制文件失败");
    }

    private void CopyPathButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentItems.Count == 0)
        {
            return;
        }

        var text = string.Join(Environment.NewLine, _currentItems.Select(item => item.Path));
        ShowStatus(_clipboardWriteService.SetText(text) ? "已复制路径" : "复制失败");
    }

    private void CopyNameButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentItems.Count == 0)
        {
            return;
        }

        var text = string.Join(Environment.NewLine, _currentItems.Select(item => item.Name));
        ShowStatus(_clipboardWriteService.SetText(text) ? "已复制名称" : "复制失败");
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var item = CurrentItem;
        if (item is null || item.Type != PathItemType.File)
        {
            return;
        }

        ShowStatus(_fileActionService.Open(item) ? "已打开文件" : "打开失败");
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        var item = CurrentItem;
        if (item is null)
        {
            return;
        }

        ShowStatus(_fileActionService.OpenDirectory(item) ? "已打开目录" : "打开失败");
    }

    private void ShowDetectedPaths(IReadOnlyList<PathItem> items)
    {
        _currentItems = items;
        var first = items[0];
        DragTip.Visibility = Visibility.Collapsed;
        BubblePanel.Visibility = Visibility.Visible;

        DetectedTypeText.Text = first.IsGeneratedFromClipboard
            ? "截图已存好"
            : first.Type == PathItemType.File ? "发现文件" : "发现文件夹";
        DetectedNameText.Text = first.IsGeneratedFromClipboard
            ? "可以直接复制截图路径"
            : items.Count == 1 ? first.Name : $"共 {items.Count} 个项目";
        UpdateActionButtons(items);

        Show();
        Activate();
    }

    private void UpdateActionButtons(IReadOnlyList<PathItem> items)
    {
        var first = items[0];
        var hasFiles = items.Any(item => item.Type == PathItemType.File);
        var hasDirectories = items.Any(item => item.Type == PathItemType.Directory);
        var isSingle = items.Count == 1;
        var isScreenshot = first.IsGeneratedFromClipboard;

        CopyFileButton.Visibility = hasFiles ? Visibility.Visible : Visibility.Collapsed;
        CopyFileButton.Content = isScreenshot ? "复制截图" : isSingle ? "复制文件" : "复制文件";
        CopyPathButton.Content = isScreenshot ? "复制图路径" : "复制路径";
        CopyNameButton.Visibility = isScreenshot ? Visibility.Collapsed : Visibility.Visible;
        CopyNameButton.Content = hasDirectories && !hasFiles ? "复制夹名" : "复制名称";
        OpenFileButton.Visibility = isSingle && first.Type == PathItemType.File ? Visibility.Visible : Visibility.Collapsed;
        OpenFileButton.Content = isScreenshot ? "打开截图" : "打开";
        OpenButton.Visibility = isSingle || hasDirectories ? Visibility.Visible : Visibility.Collapsed;
        OpenButton.Content = first.Type == PathItemType.File ? "打开目录" : "打开文件夹";
    }

    private void ShowStatus(string message, bool force = false)
    {
        if (!force && !_settings.ShowSuccessTip && message.StartsWith("已", StringComparison.Ordinal))
        {
            return;
        }

        DragTip.Visibility = Visibility.Visible;
        StatusText.Text = message;
        _statusTimer.Stop();
        _statusTimer.Start();
    }

    private void HideStatusTip()
    {
        _statusTimer.Stop();
        DragTip.Visibility = Visibility.Collapsed;
    }

    private void ResetDragState()
    {
        DragTip.Visibility = Visibility.Collapsed;
        StatusText.Text = "松开复制路径";
    }

    public void ExitApplication()
    {
        _isExiting = true;
        Close();
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _windowStateService.Save(this);
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        _clipboardMonitorService.Stop();
        _trayService.Dispose();
    }
}
