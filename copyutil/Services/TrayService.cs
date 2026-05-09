using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace copyutil.Services;

public sealed class TrayService : IDisposable
{
    private readonly Window _window;
    private readonly Icon _icon;
    private readonly Forms.NotifyIcon _notifyIcon;

    public TrayService(Window window)
    {
        _window = window;
        using var stream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Images/clip_pilot_icon.ico"))?.Stream;
        _icon = stream is null ? SystemIcons.Application : new Icon(stream);
        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "ClipPilot",
            Icon = _icon,
            Visible = true,
            ContextMenuStrip = CreateMenu()
        };
        _notifyIcon.DoubleClick += (_, _) => ToggleWindow();
    }

    private Forms.ContextMenuStrip CreateMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("显示悬浮窗", null, (_, _) => ShowWindow());
        menu.Items.Add("隐藏悬浮窗", null, (_, _) => _window.Hide());
        menu.Items.Add("设置", null, (_, _) => ShowSettings());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => Exit());
        return menu;
    }

    private void ToggleWindow()
    {
        if (_window.IsVisible)
        {
            _window.Hide();
        }
        else
        {
            ShowWindow();
        }
    }

    private void ShowWindow()
    {
        _window.Show();
        _window.Activate();
    }

    private void ShowSettings()
    {
        ShowWindow();
        if (_window is MainWindow mainWindow)
        {
            mainWindow.ShowSettings();
        }
    }

    private void Exit()
    {
        if (_window is MainWindow mainWindow)
        {
            mainWindow.ExitApplication();
            return;
        }

        Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon.Dispose();
    }
}
