# Progress

## Current status

Initial MVP implementation baseline completed.

## Completed

- Created `CliHere.sln` with WPF app (`src/CliHere.App`) and xUnit tests (`src/CliHere.Tests`)
- Added models: `AppSettings`, `CliDefinition`, `CliDetectionResult`
- Added services: `SettingsService`, `LocalizationService`, `CliDefinitionService`, `CliDetectionService`, `ContextMenuRegistryService`, `TerminalLauncher`, `LauncherService`
- Implemented launcher mode entry point: `CliHere.exe run <cliId> <folderPath>`
- Added initial settings UI and `MainViewModel`
- Added Korean/English localization JSON files
- Added initial unit tests for settings defaults and built-in CLI definition coverage

## Verification

- `dotnet restore` succeeded
- `dotnet build --configuration Release` succeeded
- `dotnet test --configuration Release` succeeded (2 tests passed)

## Next step

- Connect all UI labels to localization keys and refine UX copy
- Expand tests for terminal launch quoting/elevation and registry safety boundaries

- Added CLI list status refresh and install/docs link actions in settings UI
- Replaced hard-coded UI labels with localization-backed bindings

- Added TerminalLauncher quoting/start-info unit tests and refactored launcher for testability
- Executed built app once from Release output (CliHere.App.exe)

- Added registry safety unit tests for owned key prefix checks and launcher command formatting

- Added selectable CLI enable flags with persisted settings (EnabledCliIds)
- Verified local publish output and zip packaging (dist/CliHere-win-x64.zip)

- Aligned app output name to CliHere.exe (from project assembly output)
- Aligned GitHub Actions SDK setup to .NET 9.x for build/release workflows

- Added brand identity app icon asset (folder + terminal motif) and wired it to app/window icon settings

- Implemented parent context menu grouping so CLI entries appear under a single CLI Here submenu

- Switched release publish mode to portable (self-contained=false) to reduce distribution size
- Measured local zip size delta: self-contained 58,687,386 bytes vs portable 108,033 bytes

- Started auto-update implementation: GitHub release check, update dialog, skip-version setting, startup background check, and manual check action
- Release workflow now uploads CliHere.exe and CliHere.exe.sha256 assets for updater consumption

- Auto-update UX pass: added skipped-version label/reset action and concurrent update-check guard
- Expanded SettingsService tests for skipped update persistence and invalid JSON fallback

- Implemented custom CLI definitions (add/remove) with persistence and launcher integration
- Added PowerShell 7 terminal mode with pwsh detection fallback
- Added explicit context menu Repair action and completion message

- Prepared v0.1.1 release rehearsal (version bump + changelog + validation)

- Refreshed GitHub README.md with a top-repo style layout (badges, highlights, install path, roadmap snapshot)

- Switched root README default language to Korean and added cross-language links (README.md ↔ README.en.md)

