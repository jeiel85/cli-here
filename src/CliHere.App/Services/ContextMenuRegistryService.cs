using CliHere.App.Models;
using Microsoft.Win32;

namespace CliHere.App.Services;

public sealed class ContextMenuRegistryService
{
    internal const string Prefix = "CliHere_";
    internal const string ParentGroupKey = $"{Prefix}Group";
    internal const string BackgroundPath = @"Software\Classes\Directory\Background\shell";
    internal const string FolderPath = @"Software\Classes\Directory\shell";

    public void RegisterCli(CliDefinition cliDefinition, string parentMenuLabel, string menuLabel, string appExecutablePath)
    {
        RegisterForPath(BackgroundPath, cliDefinition.Id, parentMenuLabel, menuLabel, appExecutablePath, "%V");
        RegisterForPath(FolderPath, cliDefinition.Id, parentMenuLabel, menuLabel, appExecutablePath, "%1");
    }

    public void RemoveAll()
    {
        RemoveOwnedKeys(BackgroundPath);
        RemoveOwnedKeys(FolderPath);
    }

    private static void RegisterForPath(string basePath, string cliId, string parentMenuLabel, string menuLabel, string executablePath, string argumentToken)
    {
        using RegistryKey baseKey = Registry.CurrentUser.CreateSubKey(basePath) ?? throw new InvalidOperationException("Unable to create registry path.");
        using RegistryKey parentKey = baseKey.CreateSubKey(ParentGroupKey) ?? throw new InvalidOperationException("Unable to create parent command key.");
        parentKey.SetValue("MUIVerb", parentMenuLabel, RegistryValueKind.String);

        using RegistryKey shellKey = parentKey.CreateSubKey("shell") ?? throw new InvalidOperationException("Unable to create shell key.");
        string keyName = BuildOwnedKeyName(cliId);
        using RegistryKey commandOwner = shellKey.CreateSubKey(keyName) ?? throw new InvalidOperationException("Unable to create command owner key.");
        commandOwner.SetValue(string.Empty, menuLabel, RegistryValueKind.String);

        using RegistryKey commandKey = commandOwner.CreateSubKey("command") ?? throw new InvalidOperationException("Unable to create command key.");
        string commandValue = BuildLauncherCommand(executablePath, cliId, argumentToken);
        commandKey.SetValue(string.Empty, commandValue, RegistryValueKind.String);
    }

    private static void RemoveOwnedKeys(string basePath)
    {
        using RegistryKey? baseKey = Registry.CurrentUser.OpenSubKey(basePath, writable: true);
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

    internal static string BuildOwnedKeyName(string cliId) => Prefix + cliId;

    internal static bool IsOwnedKey(string subKeyName) => subKeyName.StartsWith(Prefix, StringComparison.Ordinal);

    internal static string BuildLauncherCommand(string executablePath, string cliId, string argumentToken)
        => $"\"{executablePath}\" run {cliId} \"{argumentToken}\"";
}
