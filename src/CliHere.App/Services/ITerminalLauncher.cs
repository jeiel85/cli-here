namespace CliHere.App.Services;

public interface ITerminalLauncher
{
    void Launch(Models.TerminalMode terminalMode, bool runAsAdministrator, string workingDirectory, string command);
}
