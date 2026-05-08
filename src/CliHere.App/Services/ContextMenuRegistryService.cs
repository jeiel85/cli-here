using CliHere.App.Models;
using Microsoft.Win32;

namespace CliHere.App.Services;

public sealed class ContextMenuRegistryService
{
    internal const string Prefix = "CliHere_";
    internal const string ParentGroupKey = $"{Prefix}Group";
    // HKLM scope. Win11 25H2 stopped honoring HKCU\Software\Classes\Directory entries for the
    // shell context menu, so registration must happen system-wide. The app prompts for UAC once
    // when the user clicks Apply / Repair / Remove.
    internal const string BackgroundPath = @"SOFTWARE\Classes\Directory\Background\shell";
    internal const string FolderPath = @"SOFTWARE\Classes\Directory\shell";
    internal const string DesktopBackgroundPath = @"SOFTWARE\Classes\DesktopBackground\Shell";

    // Legacy HKCU store keys cleaned up so old per-user registrations don't linger.
    private const string LegacyHkcuBackgroundParentPath = @"Software\Classes\Directory\Background\shell";
    private const string LegacyHkcuFolderParentPath = @"Software\Classes\Directory\shell";
    private const string LegacyHkcuDesktopParentPath = @"Software\Classes\DesktopBackground\Shell";
    private const string LegacyHkcuBackgroundStorePath = @"Software\Classes\CliHere.CascadeMenu.Background";
    private const string LegacyHkcuFolderStorePath = @"Software\Classes\CliHere.CascadeMenu.Folder";

    public void RegisterCli(CliDefinition cliDefinition, string parentMenuLabel, string menuLabel, string appExecutablePath)
    {
        RegisterForContext(BackgroundPath, cliDefinition.Id, parentMenuLabel, menuLabel, appExecutablePath, "%V");
        RegisterForContext(FolderPath, cliDefinition.Id, parentMenuLabel, menuLabel, appExecutablePath, "%1");
        RegisterForContext(DesktopBackgroundPath, cliDefinition.Id, parentMenuLabel, menuLabel, appExecutablePath, "%V");
    }

    public void RemoveAll()
    {
        RemoveOwnedKeys(Registry.LocalMachine, BackgroundPath);
        RemoveOwnedKeys(Registry.LocalMachine, FolderPath);
        RemoveOwnedKeys(Registry.LocalMachine, DesktopBackgroundPath);

        // Best-effort cleanup of legacy HKCU registrations from prior versions of the app.
        RemoveOwnedKeys(Registry.CurrentUser, LegacyHkcuBackgroundParentPath);
        RemoveOwnedKeys(Registry.CurrentUser, LegacyHkcuFolderParentPath);
        RemoveOwnedKeys(Registry.CurrentUser, LegacyHkcuDesktopParentPath);
        RemoveSubKeyIfExists(Registry.CurrentUser, LegacyHkcuBackgroundStorePath);
        RemoveSubKeyIfExists(Registry.CurrentUser, LegacyHkcuFolderStorePath);
    }

    private static void RegisterForContext(
        string parentBasePath,
        string cliId,
        string parentMenuLabel,
        string menuLabel,
        string executablePath,
        string argumentToken)
    {
        // Inline-shell cascade pattern matched to Python IDLE's registry layout, which Win11 25H2
        // is confirmed to render correctly:
        //   parent
        //     MUIVerb     = display label
        //     Subcommands = ""        ← signals "this is a static cascade host"
        //     shell\<verb>
        //       MUIVerb   = child label
        //       command\(default) = "exe ..."
        // Note: extra values like a (default) display name or Icon on the child verb make some
        // Win11 builds collapse the cascade. We deliberately keep the child minimal.
        using RegistryKey baseKey = Registry.LocalMachine.CreateSubKey(parentBasePath) ?? throw new InvalidOperationException("Unable to create registry path.");
        using RegistryKey parentKey = baseKey.CreateSubKey(ParentGroupKey) ?? throw new InvalidOperationException("Unable to create parent command key.");
        parentKey.SetValue("MUIVerb", parentMenuLabel, RegistryValueKind.String);
        parentKey.SetValue("Subcommands", string.Empty, RegistryValueKind.String);
        parentKey.SetValue("Icon", executablePath, RegistryValueKind.String);

        using RegistryKey shellKey = parentKey.CreateSubKey("shell") ?? throw new InvalidOperationException("Unable to create shell key.");
        string keyName = BuildOwnedKeyName(cliId);
        using RegistryKey commandOwner = shellKey.CreateSubKey(keyName) ?? throw new InvalidOperationException("Unable to create command owner key.");
        commandOwner.SetValue("MUIVerb", menuLabel, RegistryValueKind.String);

        using RegistryKey commandKey = commandOwner.CreateSubKey("command") ?? throw new InvalidOperationException("Unable to create command key.");
        commandKey.SetValue(string.Empty, BuildLauncherCommand(executablePath, cliId, argumentToken), RegistryValueKind.String);
    }

    private static void RemoveOwnedKeys(RegistryKey hive, string basePath)
    {
        try
        {
            using RegistryKey? baseKey = hive.OpenSubKey(basePath, writable: true);
            if (baseKey is null)
            {
                return;
            }

            foreach (string subKey in baseKey.GetSubKeyNames())
            {
                if (IsOwnedKey(subKey))
                {
                    baseKey.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Caller is non-elevated and best-effort cleanup of legacy keys is allowed to fail.
        }
    }

    private static void RemoveSubKeyIfExists(RegistryKey hive, string fullPath)
    {
        int lastSeparator = fullPath.LastIndexOf('\\');
        if (lastSeparator < 0)
        {
            return;
        }

        string parentPath = fullPath[..lastSeparator];
        string leaf = fullPath[(lastSeparator + 1)..];

        try
        {
            using RegistryKey? parentKey = hive.OpenSubKey(parentPath, writable: true);
            parentKey?.DeleteSubKeyTree(leaf, throwOnMissingSubKey: false);
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort.
        }
    }

    internal static string BuildOwnedKeyName(string cliId) => Prefix + cliId;

    internal static bool IsOwnedKey(string subKeyName) => subKeyName.StartsWith(Prefix, StringComparison.Ordinal);

    internal static string BuildLauncherCommand(string executablePath, string cliId, string argumentToken)
        => $"\"{executablePath}\" run {cliId} \"{argumentToken}\"";
}
