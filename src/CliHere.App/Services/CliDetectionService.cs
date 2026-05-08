using CliHere.App.Models;
using Microsoft.Win32;

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
        string[] pathExtensions = (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string segment in EnumerateSearchPaths())
        {
            if (File.Exists(Path.Combine(segment, executableName)))
            {
                return Path.Combine(segment, executableName);
            }

            foreach (string extension in pathExtensions)
            {
                string ext = extension.StartsWith('.') ? extension : $".{extension}";
                string candidate = Path.Combine(segment, executableName + ext.ToLowerInvariant());
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateSearchPaths()
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        foreach (string segment in EnumerateProcessPath()) if (seen.Add(segment)) yield return segment;
        foreach (string segment in EnumerateRegistryPath(RegistryHive.CurrentUser, "Environment")) if (seen.Add(segment)) yield return segment;
        foreach (string segment in EnumerateRegistryPath(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment")) if (seen.Add(segment)) yield return segment;
        foreach (string segment in EnumerateWellKnownPaths()) if (seen.Add(segment)) yield return segment;
    }

    private static IEnumerable<string> EnumerateProcessPath()
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue)) yield break;

        foreach (string raw in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string seg = NormalizePathSegment(raw);
            if (!string.IsNullOrWhiteSpace(seg)) yield return seg;
        }
    }

    private static IEnumerable<string> EnumerateRegistryPath(RegistryHive hive, string subKey)
    {
        if (!OperatingSystem.IsWindows()) yield break;

        string? raw = null;
        try
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using RegistryKey? key = baseKey.OpenSubKey(subKey);
            raw = key?.GetValue("Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
        }
        catch
        {
            // Registry access can fail under reduced privilege; treat as no contribution.
        }

        if (string.IsNullOrWhiteSpace(raw)) yield break;

        foreach (string segment in raw.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string seg = NormalizePathSegment(segment);
            if (!string.IsNullOrWhiteSpace(seg)) yield return seg;
        }
    }

    private static IEnumerable<string> EnumerateWellKnownPaths()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string[] candidates =
        [
            Path.Combine(roaming, "npm"),
            Path.Combine(userProfile, ".local", "bin"),
            Path.Combine(userProfile, ".cargo", "bin"),
            Path.Combine(userProfile, ".dotnet", "tools"),
            Path.Combine(userProfile, ".bun", "bin"),
            Path.Combine(localApp, "Programs", "opencode"),
        ];

        foreach (string candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && Directory.Exists(candidate))
            {
                yield return candidate;
            }
        }
    }

    private static string NormalizePathSegment(string rawPathSegment)
    {
        string cleaned = rawPathSegment.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return string.Empty;
        }

        return Environment.ExpandEnvironmentVariables(cleaned);
    }
}
