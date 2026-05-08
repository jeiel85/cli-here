using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class CliDefinitionService
{
    private static readonly IReadOnlyList<CliDefinition> BuiltInCliDefinitions =
    [
        new() { Id = "gemini", DisplayName = "Gemini CLI", ExecutableName = "gemini", InstallUrl = "https://github.com/google-gemini/gemini-cli", DocsUrl = "https://github.com/google-gemini/gemini-cli" },
        new() { Id = "opencode", DisplayName = "OpenCode", ExecutableName = "opencode", InstallUrl = "https://opencode.ai", DocsUrl = "https://opencode.ai/docs" },
        new() { Id = "claude", DisplayName = "Claude Code", ExecutableName = "claude", InstallUrl = "https://docs.anthropic.com/en/docs/claude-code", DocsUrl = "https://docs.anthropic.com/en/docs/claude-code" },
        new() { Id = "codex", DisplayName = "OpenAI Codex CLI", ExecutableName = "codex", InstallUrl = "https://platform.openai.com/docs/codex", DocsUrl = "https://platform.openai.com/docs/codex" },
    ];

    public IReadOnlyList<CliDefinition> GetAll() => BuiltInCliDefinitions;

    public CliDefinition? GetById(string cliId) => BuiltInCliDefinitions.FirstOrDefault(cli => string.Equals(cli.Id, cliId, StringComparison.OrdinalIgnoreCase));
}
