using System.Windows;
using CliHere.App.Models;
using CliHere.App.Services;
using CliHere.App.ViewModels;
using CliHere.App.Views;

namespace CliHere.App;

public partial class App : Application
{
    public const string ApplyContextArg = "apply-context";
    public const string RemoveContextArg = "remove-context";

    protected override void OnStartup(StartupEventArgs e)
    {
        if (!ValidateDeployment())
        {
            Shutdown(1);
            return;
        }

        base.OnStartup(e);

        if (e.Args.Length >= 3 && string.Equals(e.Args[0], "run", StringComparison.OrdinalIgnoreCase))
        {
            int exitCode = RunLauncherMode(e.Args[1], e.Args[2]);
            Shutdown(exitCode);
            return;
        }

        if (e.Args.Length >= 1 && string.Equals(e.Args[0], ApplyContextArg, StringComparison.OrdinalIgnoreCase))
        {
            Shutdown(RunApplyContextMode());
            return;
        }

        if (e.Args.Length >= 1 && string.Equals(e.Args[0], RemoveContextArg, StringComparison.OrdinalIgnoreCase))
        {
            Shutdown(RunRemoveContextMode());
            return;
        }

        SettingsService settingsService = new();
        LocalizationService localizationService = new();
        CliDefinitionService cliDefinitionService = new();
        ContextMenuRegistryService contextMenuRegistryService = new();
        CliDetectionService cliDetectionService = new();

        MainWindow mainWindow = new()
        {
            DataContext = new MainViewModel(settingsService, localizationService, cliDefinitionService, contextMenuRegistryService, cliDetectionService),
        };
        mainWindow.Show();
    }

    /// <summary>
    /// Elevated child mode: re-reads settings, deletes any existing CliHere shell entries, then
    /// re-registers menu items for every enabled CLI. The parent process saves settings to disk
    /// before invoking this so the elevated child sees the latest selections.
    /// </summary>
    private static int RunApplyContextMode()
    {
        try
        {
            string appPath = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot resolve executable path.");
            SettingsService settingsService = new();
            LocalizationService localizationService = new();
            CliDefinitionService cliDefinitionService = new();
            ContextMenuRegistryService contextMenuRegistryService = new();

            AppSettings settings = settingsService.Load();
            IReadOnlyList<CliDefinition> definitions = cliDefinitionService.GetAll(settings);
            string parentLabel = localizationService.Translate("App.Title", settings.Language);

            contextMenuRegistryService.RemoveAll();

            HashSet<string> enabledIds = settings.EnabledCliIds.Count == 0
                ? new HashSet<string>(definitions.Select(d => d.Id), StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(settings.EnabledCliIds, StringComparer.OrdinalIgnoreCase);

            foreach (CliDefinition definition in definitions.Where(d => enabledIds.Contains(d.Id)))
            {
                string label = localizationService.Translate("ContextMenu.OpenWith", settings.Language, definition.DisplayName);
                contextMenuRegistryService.RegisterCli(definition, parentLabel, label, appPath);
            }

            return 0;
        }
        catch
        {
            return 1;
        }
    }

    private static int RunRemoveContextMode()
    {
        try
        {
            new ContextMenuRegistryService().RemoveAll();
            return 0;
        }
        catch
        {
            return 1;
        }
    }

    private static int RunLauncherMode(string cliId, string folderPath)
    {
        try
        {
            LauncherService launcherService = new(new CliDefinitionService(), new SettingsService(), new TerminalLauncher());
            launcherService.RunCli(cliId, folderPath);
            return 0;
        }
        catch (Exception ex)
        {
            ThemedMessageBox.Show(ex.Message, "CLI Here", MessageBoxButton.OK, MessageBoxImage.Error);
            return 1;
        }
    }

    private static bool ValidateDeployment()
    {
        string baseDir = AppContext.BaseDirectory;
        string[] requiredFiles =
        [
            "CliHere.dll",
            "Resources\\Languages\\en.json",
            "Resources\\Languages\\ko.json",
        ];

        foreach (string relativePath in requiredFiles)
        {
            string fullPath = Path.Combine(baseDir, relativePath);
            if (File.Exists(fullPath))
            {
                continue;
            }

            ThemedMessageBox.Show(
                "Installation looks incomplete. Please extract the full ZIP package before running.\n\n"
                + $"Missing file: {relativePath}\n\n"
                + "설치 파일이 불완전합니다. ZIP 전체를 압축 해제한 뒤 실행해 주세요.\n"
                + $"누락 파일: {relativePath}",
                "CLI Here",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        return true;
    }
}
