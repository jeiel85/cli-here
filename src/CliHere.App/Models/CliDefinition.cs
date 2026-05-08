namespace CliHere.App.Models;

public sealed class CliDefinition
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string ExecutableName { get; init; }
    public required string InstallUrl { get; init; }
    public required string DocsUrl { get; init; }
}
