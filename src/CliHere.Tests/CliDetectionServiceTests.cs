using CliHere.App.Models;
using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class CliDetectionServiceTests
{
    [Fact]
    public void Detect_WhenPathSegmentIsQuoted_FindsExecutable()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string commandPath = Path.Combine(tempDir, "codex.cmd");
        File.WriteAllText(commandPath, "@echo off");

        string? originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", $"\"{tempDir}\"");
            CliDetectionService service = new();
            CliDefinition definition = new()
            {
                Id = "codex",
                DisplayName = "OpenAI Codex CLI",
                ExecutableName = "codex",
                InstallUrl = "https://example.com/install",
                DocsUrl = "https://example.com/docs",
            };

            CliDetectionResult result = service.Detect(definition);

            Assert.True(result.IsInstalled);
            Assert.Equal(commandPath, result.ResolvedPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }

    [Fact]
    public void Detect_WhenPathSegmentContainsEnvironmentVariable_FindsExecutable()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string commandPath = Path.Combine(tempDir, "claude.cmd");
        File.WriteAllText(commandPath, "@echo off");

        string variableName = $"CLI_HERE_TEST_PATH_{Guid.NewGuid():N}";
        string? originalPath = Environment.GetEnvironmentVariable("PATH");
        string? originalVariable = Environment.GetEnvironmentVariable(variableName);
        try
        {
            Environment.SetEnvironmentVariable(variableName, tempDir);
            Environment.SetEnvironmentVariable("PATH", $"%{variableName}%");

            CliDetectionService service = new();
            CliDefinition definition = new()
            {
                Id = "claude",
                DisplayName = "Claude Code",
                ExecutableName = "claude",
                InstallUrl = "https://example.com/install",
                DocsUrl = "https://example.com/docs",
            };

            CliDetectionResult result = service.Detect(definition);

            Assert.True(result.IsInstalled);
            Assert.Equal(commandPath, result.ResolvedPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, originalVariable);
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }
}
