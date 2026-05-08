using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CliHere.App.Models;
using CliHere.App.Services;
using CliHere.App.Views;

namespace CliHere.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    private readonly LocalizationService _localizationService;
    private readonly ContextMenuRegistryService _contextMenuRegistryService;
    private readonly CliDetectionService _cliDetectionService;
    private readonly CliDefinitionService _cliDefinitionService;
    private readonly UpdateService _updateService;
    private readonly System.Timers.Timer _updateTimer;
    private AppSettings _settings;
    private bool _isUpdating;
    private string _newCliDisplayName = string.Empty;
    private string _newCliExecutableName = string.Empty;
    private string _newCliInstallUrl = string.Empty;
    private string _newCliDocsUrl = string.Empty;

    public MainViewModel(
        SettingsService settingsService,
        LocalizationService localizationService,
        CliDefinitionService cliDefinitionService,
        ContextMenuRegistryService contextMenuRegistryService,
        CliDetectionService cliDetectionService)
    {
        _settingsService = settingsService;
        _localizationService = localizationService;
        _contextMenuRegistryService = contextMenuRegistryService;
        _cliDetectionService = cliDetectionService;
        _cliDefinitionService = cliDefinitionService;
        _settings = _settingsService.Load();
        _updateService = new UpdateService();
        _updateTimer = new System.Timers.Timer(TimeSpan.FromHours(24).TotalMilliseconds)
        {
            AutoReset = true,
        };
        _updateTimer.Elapsed += async (_, _) => await BackgroundCheckAsync();

        CliItems = new ObservableCollection<CliItemViewModel>(
            _cliDefinitionService.GetAll(_settings).Select(cli => new CliItemViewModel
            {
                Definition = cli,
                DetectionResult = _cliDetectionService.Detect(cli),
                IsEnabled = _settings.EnabledCliIds.Count == 0 || _settings.EnabledCliIds.Contains(cli.Id, StringComparer.OrdinalIgnoreCase),
            }));

        ApplyCommand = new RelayCommand(_ => Apply());
        RemoveAllCommand = new RelayCommand(_ => RemoveAll());
        RepairCommand = new RelayCommand(_ => Repair());
        RefreshCommand = new RelayCommand(_ => RefreshDetectionStatus());
        OpenInstallLinkCommand = new RelayCommand(param => OpenLink(param, isInstallLink: true));
        OpenDocsLinkCommand = new RelayCommand(param => OpenLink(param, isInstallLink: false));
        CheckUpdateCommand = new RelayCommand(_ => _ = ManualCheckForUpdateAsync());
        ResetSkippedUpdateCommand = new RelayCommand(_ => ResetSkippedUpdate());
        AddCustomCliCommand = new RelayCommand(_ => AddCustomCli());
        RemoveCustomCliCommand = new RelayCommand(param => RemoveCustomCli(param));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<CliItemViewModel> CliItems { get; }
    public ICommand ApplyCommand { get; }
    public ICommand RemoveAllCommand { get; }
    public ICommand RepairCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand OpenInstallLinkCommand { get; }
    public ICommand OpenDocsLinkCommand { get; }
    public ICommand CheckUpdateCommand { get; }
    public ICommand ResetSkippedUpdateCommand { get; }
    public ICommand AddCustomCliCommand { get; }
    public ICommand RemoveCustomCliCommand { get; }
    public string AppTitle => _localizationService.Translate("App.Title", Language);
    public string LanguageLabel => T("Settings.Language");
    public string TerminalLabel => T("Settings.Terminal");
    public string RunAsAdministratorLabel => T("Settings.RunAsAdministrator");
    public string SupportedCliLabel => T("Section.SupportedCli");
    public string CliColumnIdLabel => T("Cli.Column.Id");
    public string CliColumnNameLabel => T("Cli.Column.Name");
    public string CliColumnCommandLabel => T("Cli.Column.Command");
    public string CliColumnStatusLabel => T("Cli.Column.Status");
    public string CliColumnEnabledLabel => T("Cli.Column.Enabled");
    public string CliColumnActionsLabel => T("Cli.Column.Actions");
    public string LanguageSystemLabel => T("Language.System");
    public string LanguageKoreanLabel => T("Language.Korean");
    public string LanguageEnglishLabel => T("Language.English");
    public string TerminalWindowsTerminalLabel => T("Terminal.WindowsTerminal");
    public string TerminalPowerShellLabel => T("Terminal.PowerShell");
    public string TerminalPowerShell7Label => T("Terminal.PowerShell7");
    public string RefreshLabel => T("Action.Refresh");
    public string RemoveAllLabel => T("Action.RemoveAll");
    public string RepairLabel => T("Action.Repair");
    public string ApplyLabel => T("Action.Apply");
    public string CheckUpdateLabel => T("Action.CheckUpdate");
    public string ResetSkippedUpdateLabel => T("Action.ResetSkippedUpdate");
    public string CurrentVersionLabel => $"{T("Update.CurrentVersionPrefix")} v{UpdateService.CurrentVersion.ToString(3)}";
    public string UpdateCheckLabel { get; private set; } = string.Empty;
    public string SkippedUpdateLabel => string.IsNullOrWhiteSpace(_settings.SkippedUpdateVersion)
        ? string.Empty
        : string.Format(T("Update.SkippedVersion"), _settings.SkippedUpdateVersion);
    public string InstallLinkLabel => T("Cli.Action.OpenInstallPage");
    public string DocsLinkLabel => T("Cli.Action.OpenDocsPage");
    public string RemoveCustomCliLabel => T("Cli.Action.RemoveCustom");
    public string NewCliSectionLabel => T("CustomCli.Section");
    public string GeneralSectionLabel => T("Section.General");
    public string NewCliDisplayNameLabel => T("CustomCli.DisplayName");
    public string NewCliExecutableNameLabel => T("CustomCli.ExecutableName");
    public string NewCliInstallUrlLabel => T("CustomCli.InstallUrl");
    public string NewCliDocsUrlLabel => T("CustomCli.DocsUrl");
    public string AddCustomCliLabel => T("CustomCli.Add");
    public string InstalledStatusLabel => T("Cli.Status.Installed");
    public string NotInstalledStatusLabel => T("Cli.Status.NotInstalled");

    public LocalizationService LocalizationService => _localizationService;

    public LanguageMode Language
    {
        get => _settings.Language;
        set
        {
            if (_settings.Language == value) return;
            _settings.Language = value;
            OnPropertyChanged();
            NotifyLocalizedPropertiesChanged();
        }
    }

    public TerminalMode Terminal
    {
        get => _settings.Terminal;
        set
        {
            if (_settings.Terminal == value) return;
            _settings.Terminal = value;
            OnPropertyChanged();
        }
    }

    public bool RunAsAdministrator
    {
        get => _settings.RunAsAdministrator;
        set
        {
            if (_settings.RunAsAdministrator == value) return;
            _settings.RunAsAdministrator = value;
            OnPropertyChanged();
        }
    }

    public string NewCliDisplayName
    {
        get => _newCliDisplayName;
        set
        {
            if (_newCliDisplayName == value) return;
            _newCliDisplayName = value;
            OnPropertyChanged();
        }
    }

    public string NewCliExecutableName
    {
        get => _newCliExecutableName;
        set
        {
            if (_newCliExecutableName == value) return;
            _newCliExecutableName = value;
            OnPropertyChanged();
        }
    }

    public string NewCliInstallUrl
    {
        get => _newCliInstallUrl;
        set
        {
            if (_newCliInstallUrl == value) return;
            _newCliInstallUrl = value;
            OnPropertyChanged();
        }
    }

    public string NewCliDocsUrl
    {
        get => _newCliDocsUrl;
        set
        {
            if (_newCliDocsUrl == value) return;
            _newCliDocsUrl = value;
            OnPropertyChanged();
        }
    }

    public async Task OnAppStartedAsync()
    {
        _updateTimer.Start();
        await BackgroundCheckAsync();
    }

    private void Apply()
    {
        _settings.EnabledCliIds = CliItems.Where(x => x.IsEnabled).Select(x => x.Id).ToList();
        _settings.CustomCliDefinitions = CliItems
            .Where(x => IsCustomCliId(x.Id))
            .Select(x => new CustomCliDefinition
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                ExecutableName = x.ExecutableName,
                InstallUrl = x.Definition.InstallUrl,
                DocsUrl = x.Definition.DocsUrl,
            })
            .ToList();
        // Persist settings before elevation so the admin child reads the latest selections.
        _settingsService.Save(_settings);

        if (!RunContextMenuAction(App.ApplyContextArg))
        {
            return;
        }

        ThemedMessageBox.Show(_localizationService.Translate("Message.Applied", Language), AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Repair()
    {
        _settingsService.Save(_settings);

        if (!RunContextMenuAction(App.ApplyContextArg))
        {
            return;
        }

        ThemedMessageBox.Show(_localizationService.Translate("Message.Repaired", Language), AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RemoveAll()
    {
        if (!RunContextMenuAction(App.RemoveContextArg))
        {
            return;
        }

        ThemedMessageBox.Show(_localizationService.Translate("Message.Removed", Language), AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Runs the requested registry action either inline (when the current process is already
    /// elevated) or by relaunching this executable elevated. Win11 25H2 stopped honoring HKCU
    /// shell entries, so the actual write must happen under HKLM, which requires admin rights.
    /// Shows an error dialog and returns false on failure or UAC decline.
    /// </summary>
    private bool RunContextMenuAction(string mode)
    {
        try
        {
            if (ElevationHelper.IsCurrentProcessElevated())
            {
                if (string.Equals(mode, App.ApplyContextArg, StringComparison.OrdinalIgnoreCase))
                {
                    ApplyContextMenuRegistrationsInline();
                }
                else
                {
                    _contextMenuRegistryService.RemoveAll();
                }
                return true;
            }

            int exitCode = ElevationHelper.RunElevatedAndWait(mode);
            if (exitCode == 0)
            {
                return true;
            }

            string detail = exitCode == -1
                ? _localizationService.Translate("ContextMenu.Error.Cancelled", Language)
                : _localizationService.Translate("ContextMenu.Error.Failed", Language);
            ThemedMessageBox.Show(detail, AppTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        catch (Exception ex)
        {
            ThemedMessageBox.Show(ex.Message, AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private void ApplyContextMenuRegistrationsInline()
    {
        string appPath = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot resolve executable path.");
        _contextMenuRegistryService.RemoveAll();
        foreach (CliItemViewModel cliItem in CliItems.Where(x => x.IsEnabled))
        {
            string parentMenuLabel = AppTitle;
            string label = _localizationService.Translate("ContextMenu.OpenWith", Language, cliItem.DisplayName);
            _contextMenuRegistryService.RegisterCli(cliItem.Definition, parentMenuLabel, label, appPath);
        }
    }

    private void RefreshDetectionStatus()
    {
        for (int i = 0; i < CliItems.Count; i++)
        {
            bool isEnabled = CliItems[i].IsEnabled;
            CliDefinition definition = CliItems[i].Definition;
            CliItems[i] = new CliItemViewModel
            {
                Definition = definition,
                DetectionResult = _cliDetectionService.Detect(definition),
                IsEnabled = isEnabled,
            };
        }
    }

    private void AddCustomCli()
    {
        if (string.IsNullOrWhiteSpace(NewCliDisplayName) || string.IsNullOrWhiteSpace(NewCliExecutableName))
        {
            ThemedMessageBox.Show(T("CustomCli.Error.Required"), AppTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string baseId = Slugify(NewCliDisplayName);
        string id = EnsureUniqueCustomId(baseId);
        CliDefinition definition = new()
        {
            Id = id,
            DisplayName = NewCliDisplayName.Trim(),
            ExecutableName = NewCliExecutableName.Trim(),
            InstallUrl = string.IsNullOrWhiteSpace(NewCliInstallUrl) ? "https://example.com" : NewCliInstallUrl.Trim(),
            DocsUrl = string.IsNullOrWhiteSpace(NewCliDocsUrl) ? "https://example.com" : NewCliDocsUrl.Trim(),
        };

        CliItems.Add(new CliItemViewModel
        {
            Definition = definition,
            DetectionResult = _cliDetectionService.Detect(definition),
            IsEnabled = true,
        });

        NewCliDisplayName = string.Empty;
        NewCliExecutableName = string.Empty;
        NewCliInstallUrl = string.Empty;
        NewCliDocsUrl = string.Empty;
    }

    private void RemoveCustomCli(object? parameter)
    {
        if (parameter is not CliItemViewModel item || !IsCustomCliId(item.Id))
        {
            return;
        }

        CliItems.Remove(item);
    }

    private string EnsureUniqueCustomId(string baseId)
    {
        string id = baseId;
        int suffix = 1;
        while (CliItems.Any(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)))
        {
            id = $"{baseId}-{suffix}";
            suffix++;
        }

        return id;
    }

    private static string Slugify(string input)
    {
        string slug = new(input.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray());
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        slug = slug.Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? $"custom-{Guid.NewGuid():N}"[..12] : $"custom-{slug}";
    }

    private static bool IsCustomCliId(string id) => id.StartsWith("custom-", StringComparison.OrdinalIgnoreCase);

    private async Task ManualCheckForUpdateAsync()
    {
        if (_isUpdating)
        {
            return;
        }

        UpdateCheckLabel = T("Update.Checking");
        OnPropertyChanged(nameof(UpdateCheckLabel));

        UpdateService.UpdateInfo? result;
        try
        {
            result = await _updateService.CheckForUpdateAsync();
        }
        catch (UpdateCheckException ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                string message = ex.Kind switch
                {
                    UpdateCheckErrorKind.Network => T("Update.Error.Network"),
                    UpdateCheckErrorKind.Timeout => T("Update.Error.Timeout"),
                    UpdateCheckErrorKind.RateLimit => string.Format(T("Update.Error.RateLimit"), ex.RetryAtLocal ?? "?"),
                    UpdateCheckErrorKind.ApiError => string.Format(T("Update.Error.Api"), ex.StatusCode ?? 0),
                    _ => T("Update.Error.Unknown"),
                };
                ThemedMessageBox.Show(message, T("Update.DialogTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            });

            UpdateCheckLabel = string.Empty;
            OnPropertyChanged(nameof(UpdateCheckLabel));
            return;
        }
        catch
        {
            UpdateCheckLabel = string.Empty;
            OnPropertyChanged(nameof(UpdateCheckLabel));
            return;
        }

        if (result is null)
        {
            UpdateCheckLabel = string.Format(T("Update.UpToDate"), UpdateService.CurrentVersion.ToString(3));
            OnPropertyChanged(nameof(UpdateCheckLabel));
            return;
        }

        UpdateCheckLabel = string.Empty;
        OnPropertyChanged(nameof(UpdateCheckLabel));
        await Application.Current.Dispatcher.InvokeAsync(() => OfferUpdate(result));
    }

    private async Task BackgroundCheckAsync()
    {
        if (_isUpdating)
        {
            return;
        }

        try
        {
            UpdateService.UpdateInfo? result = await _updateService.CheckForUpdateAsync();
            if (result is null) return;
            await Application.Current.Dispatcher.InvokeAsync(() => OfferUpdate(result));
        }
        catch
        {
            // Background update check should fail silently.
        }
    }

    private void OfferUpdate(UpdateService.UpdateInfo info)
    {
        string version = info.Version.ToString(3);
        if (IsVersionSkipped(version))
        {
            return;
        }

        UpdateDialog dialog = new(
            info.ReleaseNotes,
            onSkip: () => SkipVersion(version),
            title: T("Update.DialogTitle"),
            skipLabel: T("Update.Skip"),
            updateLabel: T("Update.UpdateNow"),
            errorTitle: T("Update.ErrorTitle"));

        dialog.OnUpdateRequested += () =>
        {
            _isUpdating = true;
            _ = Task.Run(async () =>
            {
                try
                {
                    string preparedPath = await _updateService.DownloadAndPrepareUpdateAsync(
                        info.DownloadUrl,
                        info.Sha256Url,
                        (pc, status) =>
                        {
                            string localizedStatus = status switch
                            {
                                "Downloading..." => T("Update.Status.Downloading"),
                                "Verifying..." => T("Update.Status.Verifying"),
                                _ => status,
                            };
                            dialog.UpdateProgress(pc, localizedStatus);
                        });

                    dialog.UpdateProgress(100, T("Update.Restarting"));
                    await Task.Delay(500);
                    _updateService.ApplyPreparedUpdate(preparedPath);
                }
                catch (Exception ex)
                {
                    _isUpdating = false;
                    dialog.ShowError(ex.Message);
                }
            });
        };

        dialog.Show();
    }

    private bool IsVersionSkipped(string version)
        => string.Equals(_settings.SkippedUpdateVersion, version, StringComparison.OrdinalIgnoreCase);

    private void SkipVersion(string version)
    {
        _settings.SkippedUpdateVersion = version;
        _settingsService.Save(_settings);
        OnPropertyChanged(nameof(SkippedUpdateLabel));
    }

    private void ResetSkippedUpdate()
    {
        if (string.IsNullOrWhiteSpace(_settings.SkippedUpdateVersion))
        {
            return;
        }

        _settings.SkippedUpdateVersion = null;
        _settingsService.Save(_settings);
        OnPropertyChanged(nameof(SkippedUpdateLabel));
    }

    private void OpenLink(object? parameter, bool isInstallLink)
    {
        if (parameter is not CliItemViewModel cliItem)
        {
            return;
        }

        string url = isInstallLink ? cliItem.Definition.InstallUrl : cliItem.Definition.DocsUrl;
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private string T(string key) => _localizationService.Translate(key, Language);

    private void NotifyLocalizedPropertiesChanged()
    {
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(TerminalLabel));
        OnPropertyChanged(nameof(RunAsAdministratorLabel));
        OnPropertyChanged(nameof(SupportedCliLabel));
        OnPropertyChanged(nameof(CliColumnIdLabel));
        OnPropertyChanged(nameof(CliColumnNameLabel));
        OnPropertyChanged(nameof(CliColumnCommandLabel));
        OnPropertyChanged(nameof(CliColumnStatusLabel));
        OnPropertyChanged(nameof(CliColumnEnabledLabel));
        OnPropertyChanged(nameof(CliColumnActionsLabel));
        OnPropertyChanged(nameof(LanguageSystemLabel));
        OnPropertyChanged(nameof(LanguageKoreanLabel));
        OnPropertyChanged(nameof(LanguageEnglishLabel));
        OnPropertyChanged(nameof(TerminalWindowsTerminalLabel));
        OnPropertyChanged(nameof(TerminalPowerShellLabel));
        OnPropertyChanged(nameof(TerminalPowerShell7Label));
        OnPropertyChanged(nameof(RefreshLabel));
        OnPropertyChanged(nameof(RemoveAllLabel));
        OnPropertyChanged(nameof(RepairLabel));
        OnPropertyChanged(nameof(ApplyLabel));
        OnPropertyChanged(nameof(CheckUpdateLabel));
        OnPropertyChanged(nameof(ResetSkippedUpdateLabel));
        OnPropertyChanged(nameof(CurrentVersionLabel));
        OnPropertyChanged(nameof(UpdateCheckLabel));
        OnPropertyChanged(nameof(SkippedUpdateLabel));
        OnPropertyChanged(nameof(InstallLinkLabel));
        OnPropertyChanged(nameof(DocsLinkLabel));
        OnPropertyChanged(nameof(RemoveCustomCliLabel));
        OnPropertyChanged(nameof(NewCliSectionLabel));
        OnPropertyChanged(nameof(GeneralSectionLabel));
        OnPropertyChanged(nameof(NewCliDisplayNameLabel));
        OnPropertyChanged(nameof(NewCliExecutableNameLabel));
        OnPropertyChanged(nameof(NewCliInstallUrlLabel));
        OnPropertyChanged(nameof(NewCliDocsUrlLabel));
        OnPropertyChanged(nameof(AddCustomCliLabel));
        OnPropertyChanged(nameof(InstalledStatusLabel));
        OnPropertyChanged(nameof(NotInstalledStatusLabel));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
