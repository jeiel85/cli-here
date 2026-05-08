namespace CliHere.App.Models;

public enum LanguageMode
{
    System,
    Korean,
    English,
}

public enum TerminalMode
{
    WindowsTerminal,
    PowerShell,
}

public sealed class AppSettings
{
    public LanguageMode Language { get; set; } = LanguageMode.System;
    public TerminalMode Terminal { get; set; } = TerminalMode.WindowsTerminal;
    public bool RunAsAdministrator { get; set; }
    public List<string> EnabledCliIds { get; set; } = [];
}
