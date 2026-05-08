using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class ContextMenuRegistryServiceTests
{
    [Fact]
    public void BuildOwnedKeyName_UsesCliHerePrefix()
    {
        string key = ContextMenuRegistryService.BuildOwnedKeyName("codex");
        Assert.Equal("CliHere_codex", key);
    }

    [Fact]
    public void IsOwnedKey_ReturnsTrueOnlyForOwnedPrefix()
    {
        Assert.True(ContextMenuRegistryService.IsOwnedKey("CliHere_gemini"));
        Assert.True(ContextMenuRegistryService.IsOwnedKey(ContextMenuRegistryService.ParentGroupKey));
        Assert.False(ContextMenuRegistryService.IsOwnedKey("OtherApp_gemini"));
    }

    [Fact]
    public void BuildLauncherCommand_FormatsExpectedRunSyntax()
    {
        string command = ContextMenuRegistryService.BuildLauncherCommand(
            @"C:\Program Files\CliHere\CliHere.exe",
            "claude",
            "%V");

        Assert.Equal("\"C:\\Program Files\\CliHere\\CliHere.exe\" run claude \"%V\"", command);
    }
}
