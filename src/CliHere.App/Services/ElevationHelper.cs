using System.Diagnostics;
using System.Security.Principal;

namespace CliHere.App.Services;

public static class ElevationHelper
{
    public static bool IsCurrentProcessElevated()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Re-launches the current executable elevated (UAC prompt) with the given arguments and waits
    /// for the child process to exit. Returns the child's exit code, or -1 if the user declined the
    /// UAC prompt.
    /// </summary>
    public static int RunElevatedAndWait(string arguments)
    {
        string? exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
        {
            throw new InvalidOperationException("Cannot resolve current executable path for elevation.");
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = exePath,
            Arguments = arguments,
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
        };

        try
        {
            using Process? child = Process.Start(startInfo);
            if (child is null)
            {
                return -1;
            }
            child.WaitForExit();
            return child.ExitCode;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // User clicked "No" on the UAC prompt, or no permission to elevate.
            return -1;
        }
    }
}
