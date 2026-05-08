using CliHere.App.Models;
using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class SettingsServiceTests
{
    [Fact]
    public void Load_WhenFileMissing_ReturnsDefaults()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        SettingsService service = new(tempDir);
        AppSettings settings = service.Load();

        Assert.Equal(LanguageMode.System, settings.Language);
        Assert.Equal(TerminalMode.WindowsTerminal, settings.Terminal);
        Assert.False(settings.RunAsAdministrator);
    }
}
