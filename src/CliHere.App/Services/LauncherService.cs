using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class LauncherService
{
    private readonly CliDefinitionService _cliDefinitionService;
    private readonly SettingsService _settingsService;
    private readonly ITerminalLauncher _terminalLauncher;

    public LauncherService(CliDefinitionService cliDefinitionService, SettingsService settingsService, ITerminalLauncher terminalLauncher)
    {
        _cliDefinitionService = cliDefinitionService;
        _settingsService = settingsService;
        _terminalLauncher = terminalLauncher;
    }

    public void RunCli(string cliId, string folderPath)
    {
        if (string.IsNullOrWhiteSpace(cliId))
        {
            throw new ArgumentException("CLI ID is required.", nameof(cliId));
        }

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            throw new ArgumentException("Folder path is invalid.", nameof(folderPath));
        }

        CliDefinition cli = _cliDefinitionService.GetById(cliId) ?? throw new ArgumentException("Unknown CLI ID.", nameof(cliId));
        AppSettings settings = _settingsService.Load();
        _terminalLauncher.Launch(settings.Terminal, settings.RunAsAdministrator, folderPath, cli.ExecutableName);
    }
}
