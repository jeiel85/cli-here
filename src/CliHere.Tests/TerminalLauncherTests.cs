using System.Diagnostics;
using CliHere.App.Models;
using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class TerminalLauncherTests
{
    [Fact]
    public void Quote_EscapesEmbeddedDoubleQuotes()
    {
        string raw = "C:\\Work Folder\\\"My Project\"";
        string quoted = TerminalLauncher.Quote(raw);

        Assert.StartsWith("\"", quoted, StringComparison.Ordinal);
        Assert.EndsWith("\"", quoted, StringComparison.Ordinal);
        Assert.Contains("\\\"", quoted, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateStartInfo_WhenRunAsAdministrator_SetsRunAsAndShellExecute()
    {
        ProcessStartInfo psi = TerminalLauncher.CreateStartInfo(
            TerminalMode.PowerShell,
            runAsAdministrator: true,
            workingDirectory: "C:\\Work Folder",
            command: "codex");

        Assert.Equal("powershell.exe", psi.FileName);
        Assert.True(psi.UseShellExecute);
        Assert.Equal("runas", psi.Verb);
        Assert.Contains("-NoExit -Command \"codex\"", psi.Arguments, StringComparison.Ordinal);
    }
}
