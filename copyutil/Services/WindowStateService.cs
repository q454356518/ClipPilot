using copyutil.Models;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace copyutil.Services;

public sealed class WindowStateService
{
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ClipPilot",
        "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Apply(Window window)
    {
        var settings = Load();
        window.Left = settings.WindowLeft;
        window.Top = settings.WindowTop;
    }

    public void Save(Window window)
    {
        var settings = Load();
        settings.WindowLeft = window.Left;
        settings.WindowTop = window.Top;
        Save(settings);
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings));
        }
        catch
        {
        }
    }
}
