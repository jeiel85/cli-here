# Changelog

## v0.1.2 - 2026-05-08

### Changed

- Fixed oversized artifact path by aligning local `scripts/publish.ps1` with framework-dependent release mode.
- Added symbol-stripping publish flags in release/publish pipeline (`DebugType=None`, `DebugSymbols=false`) to keep artifacts lean.
- Refined README language flow and added GitHub Pages branding entrypoint.

### Verification

- `./scripts/publish.ps1` passed.
- `dist/CliHere/CliHere.exe` size verified: 244,878 bytes.
- `dist/CliHere-win-x64.zip` size verified: 111,531 bytes.
- `dotnet build --configuration Release` passed.
- `dotnet test --configuration Release` passed.
## v0.1.1 - 2026-05-08

### Added`n`n- Added a responsive GitHub Pages branding landing page (`docs/index.html`).`n

- Custom CLI definition support (add/remove in settings, persistence, launcher integration).
- Context menu Repair action for one-click registry rebuild.
- PowerShell 7 terminal mode option with `pwsh` detection fallback.
- GitHub Releases-based auto-update baseline with check/download/verify/apply flow.

### Changed`n`n- Fixed publish script/release flags to prevent accidental oversized self-contained artifacts.`n- Added no-symbol publish flags to keep release artifact size lean.`n`n`n- Switched README default language to Korean and added explicit Korean/English cross-links.`n`n`n- README redesign with a top-repo style structure (badges, highlights, install flow, roadmap snapshot).`n

- Grouped Explorer context menu entries under a single `CLI Here` parent submenu.
- Switched release packaging to framework-dependent mode (`self-contained=false`) for smaller default asset size.
- Updated release workflow to publish `CliHere.exe` and `CliHere.exe.sha256` updater assets.

### Fixed

- Improved update flow resilience (overlap guard, skipped-version reset, invalid settings JSON fallback).

### Verification

- `dotnet build --configuration Release` passed.
- `dotnet test --configuration Release` passed (15 tests).
- Build CI passed on `main` before tag release.
## v0.1.0 - 2026-05-08

### Added`n`n- Added a responsive GitHub Pages branding landing page (`docs/index.html`).`n`n`n- Added custom CLI definition support (add/remove in settings, persistence, and launcher execution).`n- Added explicit context menu Repair action for one-click registry rebuild.`n- Added PowerShell 7 terminal mode support with pwsh detection fallback.`n`n`n- Started auto-update implementation using GitHub Releases (check/download/verify/apply flow with update dialog).`n- Added update check UI action and startup background update check path.`n

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

### Changed`n`n- Fixed publish script/release flags to prevent accidental oversized self-contained artifacts.`n- Added no-symbol publish flags to keep release artifact size lean.`n`n`n- Switched README default language to Korean and added explicit Korean/English cross-links.`n`n`n- README redesign with a top-repo style structure (badges, highlights, install flow, roadmap snapshot).`n`n`n- Added skipped-version visibility/reset controls in settings UI for update flow recovery.`n- Added guard against overlapping update-check/apply flows.`n`n`n- Release workflow now publishes `CliHere.exe` and `CliHere.exe.sha256` assets for updater compatibility.`n- Removed portable naming wording and kept default framework-dependent release terminology.`n`n`n- Changed release publish mode to portable (`self-contained=false`) to dramatically reduce asset size.`n- Added runtime requirement note for .NET 9 Desktop Runtime (x64).`n`n`n- Grouped context menu entries under a single `CLI Here` parent submenu for cleaner Explorer menus.`n`n`n- Added a brand identity app icon (folder + terminal motif) and applied it to the executable/window icon settings.`n`n`n- Aligned output executable naming to `CliHere.exe` by setting assembly output name.`n- Aligned build/release workflow SDK setup to `.NET 9.x` for target-framework consistency.`n

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

### Verification`n`n- Added test coverage for settings invalid JSON fallback and skipped update persistence.`n

- `dotnet restore` passed.
- `dotnet build --configuration Release` passed.
- `dotnet test --configuration Release` passed (10 tests).
- `dotnet publish src/CliHere.App/CliHere.App.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -o dist/CliHere` passed.
- Local zip `dist/CliHere-win-x64.zip` generated (size > 0).
- Published exe startup verified locally.













