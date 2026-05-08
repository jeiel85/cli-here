using System.Windows;
using CliHere.App.Services;
using CliHere.App.ViewModels;

namespace CliHere.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Length >= 3 && string.Equals(e.Args[0], "run", StringComparison.OrdinalIgnoreCase))
        {
            int exitCode = RunLauncherMode(e.Args[1], e.Args[2]);
            Shutdown(exitCode);
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
            MessageBox.Show(ex.Message, "CLI Here", MessageBoxButton.OK, MessageBoxImage.Error);
            return 1;
        }
    }
}
