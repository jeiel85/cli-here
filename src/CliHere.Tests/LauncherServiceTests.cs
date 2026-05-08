using CliHere.App.Models;
using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class LauncherServiceTests
{
    [Fact]
    public void RunCli_WhenCliIdIsEmpty_ThrowsArgumentException()
    {
        LauncherService service = CreateService(out _);
        string folderPath = Path.GetTempPath();

        Assert.Throws<ArgumentException>(() => service.RunCli("", folderPath));
    }

    [Fact]
    public void RunCli_WhenFolderPathIsInvalid_ThrowsArgumentException()
    {
        LauncherService service = CreateService(out _);
        Assert.Throws<ArgumentException>(() => service.RunCli("codex", "Z:\\does-not-exist\\folder"));
    }

    [Fact]
    public void RunCli_WhenValid_CallsTerminalLauncher()
    {
        LauncherService service = CreateService(out FakeTerminalLauncher fakeLauncher);
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        service.RunCli("codex", tempDir);

        Assert.Equal("codex", fakeLauncher.LastCommand);
        Assert.Equal(tempDir, fakeLauncher.LastWorkingDirectory);
    }

    private static LauncherService CreateService(out FakeTerminalLauncher fakeLauncher)
    {
        fakeLauncher = new FakeTerminalLauncher();
        SettingsService settingsService = new(Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N")));
        settingsService.Save(new AppSettings
        {
            Terminal = TerminalMode.PowerShell,
            RunAsAdministrator = false,
        });

        return new LauncherService(new CliDefinitionService(), settingsService, fakeLauncher);
    }

    private sealed class FakeTerminalLauncher : ITerminalLauncher
    {
        public string? LastWorkingDirectory { get; private set; }
        public string? LastCommand { get; private set; }

        public void Launch(TerminalMode terminalMode, bool runAsAdministrator, string workingDirectory, string command)
        {
            LastWorkingDirectory = workingDirectory;
            LastCommand = command;
        }
    }
}
