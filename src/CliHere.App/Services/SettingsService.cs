using System.Text.Json;
using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsDirectoryPath;

    public SettingsService(string? settingsDirectoryPath = null)
    {
        _settingsDirectoryPath = string.IsNullOrWhiteSpace(settingsDirectoryPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CliHere")
            : settingsDirectoryPath;
    }

    public string SettingsDirectoryPath => _settingsDirectoryPath;
    public string SettingsFilePath => Path.Combine(SettingsDirectoryPath, "settings.json");

    public AppSettings Load()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return new AppSettings();
        }

        try
        {
            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectoryPath);
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }
}
