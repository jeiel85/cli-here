# Changelog

## v0.1.0 - 2026-05-08

### Added

- Initial planning bundle for `CLI Here / CLI 여기서 열기`.
- Project-specific `AGENTS.md` with automation-first rules.
- English-first `README.md` and Korean `README.ko.md`.
- Architecture, roadmap, branding, localization, security, release, and agent prompt docs.
- `.agent/tasks.md`, `.agent/progress.md`, `.agent/decisions.md`.
- Initial .NET solution (`CliHere.sln`) with WPF app and xUnit test project.
- MVP service baseline: CLI definition/detection, settings, localization, context menu registry, terminal launcher.
- Launcher mode command path: `CliHere.exe run <cliId> <folderPath>`.
- Korean/English localization resources under `src/CliHere.App/Resources/Languages`.
- Unit tests for settings, CLI definitions, terminal launch argument safety, launcher validation, and registry ownership rules.

### Changed`n`n- Grouped context menu entries under a single `CLI Here` parent submenu for cleaner Explorer menus.`n`n`n- Added a brand identity app icon (folder + terminal motif) and applied it to the executable/window icon settings.`n`n`n- Aligned output executable naming to `CliHere.exe` by setting assembly output name.`n- Aligned build/release workflow SDK setup to `.NET 9.x` for target-framework consistency.`n

- Settings UI labels now bind to localization keys instead of hard-coded text.
- Added per-CLI enable selection and persisted selected IDs through `EnabledCliIds`.
- Added CLI status column (Installed/Not installed) with refresh support.
- Added per-CLI Install and Docs link actions.
- Updated launcher mode with explicit input validation and error dialog on failure.
- Updated release workflow to upload `CliHere-win-x64.zip` to GitHub Release assets.
- Updated README status from planning to implementation baseline.

### Fixed

- Refactored terminal launch start-info creation and quoting path for safer coverage.
- Added registry safety coverage for `CliHere_` ownership filtering and launcher command formatting.

### Verification

- `dotnet restore` passed.
- `dotnet build --configuration Release` passed.
- `dotnet test --configuration Release` passed (10 tests).
- `dotnet publish src/CliHere.App/CliHere.App.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -o dist/CliHere` passed.
- Local zip `dist/CliHere-win-x64.zip` generated (size > 0).
- Published exe startup verified locally.



