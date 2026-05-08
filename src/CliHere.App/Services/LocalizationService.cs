using System.Globalization;
using System.Text.Json;
using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class LocalizationService
{
    private readonly Dictionary<string, string> _english;
    private readonly Dictionary<string, string> _korean;

    public LocalizationService()
    {
        _english = LoadLanguage("en");
        _korean = LoadLanguage("ko");
    }

    public string Translate(string key, LanguageMode languageMode, params object[] args)
    {
        string value = GetLanguageDictionary(languageMode).GetValueOrDefault(key)
            ?? _english.GetValueOrDefault(key)
            ?? key;

        return args.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, args);
    }

    private Dictionary<string, string> GetLanguageDictionary(LanguageMode languageMode)
    {
        return languageMode switch
        {
            LanguageMode.Korean => _korean,
            LanguageMode.English => _english,
            _ => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase) ? _korean : _english,
        };
    }

    private static Dictionary<string, string> LoadLanguage(string languageCode)
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Languages", $"{languageCode}.json");
        if (!File.Exists(filePath))
        {
            return [];
        }

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }
}
