namespace CliHere.App.Models;

public sealed class CliDetectionResult
{
    public required string CliId { get; init; }
    public required bool IsInstalled { get; init; }
    public string? ResolvedPath { get; init; }
}
