using CliHere.App.Models;

namespace CliHere.App.ViewModels;

public sealed class CliItemViewModel
{
    public required CliDefinition Definition { get; init; }

    public required CliDetectionResult DetectionResult { get; init; }

    public string Id => Definition.Id;

    public string DisplayName => Definition.DisplayName;

    public string ExecutableName => Definition.ExecutableName;

    public bool IsInstalled => DetectionResult.IsInstalled;

    public bool IsEnabled { get; set; }
}
