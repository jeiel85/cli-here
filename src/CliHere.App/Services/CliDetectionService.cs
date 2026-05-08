using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class CliDetectionService
{
    public CliDetectionResult Detect(CliDefinition definition)
    {
        string? resolvedPath = ResolveExecutablePath(definition.ExecutableName);
        return new CliDetectionResult
        {
            CliId = definition.Id,
            IsInstalled = !string.IsNullOrWhiteSpace(resolvedPath),
            ResolvedPath = resolvedPath,
        };
    }

    private static string? ResolveExecutablePath(string executableName)
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        string[] pathExtensions = (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string pathSegment in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (File.Exists(Path.Combine(pathSegment, executableName)))
            {
                return Path.Combine(pathSegment, executableName);
            }

            foreach (string extension in pathExtensions)
            {
                string ext = extension.StartsWith('.') ? extension : $".{extension}";
                string candidate = Path.Combine(pathSegment, executableName + ext.ToLowerInvariant());
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }
}
