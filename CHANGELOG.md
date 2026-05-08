# Changelog

## v0.1.0 - 2026-05-08

### Added

- Initial planning bundle for `CLI Here / CLI 여기서 열기`.
- Project-specific `AGENTS.md` with latest automation-first agent rules.
- English-first `README.md` for GitHub and Reddit distribution.
- Korean `README.ko.md`.
- Architecture, roadmap, branding, localization, security, release, and agent prompt documents.
- `.agent/tasks.md`, `.agent/progress.md`, and `.agent/decisions.md`.
- MVP scope for File Explorer context menu integration.
- Optional administrator terminal launch mode.
- Expected release asset rule: `CliHere-win-x64.zip`.
- Initial .NET solution (`CliHere.sln`) with WPF app and xUnit test project.
- Initial MVP service skeletons: CLI definition/detection, settings, localization, registry context menu, terminal launcher.
- Launcher mode command path: `CliHere.exe run <cliId> <folderPath>`.
- Korean/English localization resource files under `src/CliHere.App/Resources/Languages`.
- Initial ViewModel + settings UI skeleton for language/terminal/admin options.
- Basic unit tests for default settings and built-in CLI definitions.

### Documentation

- Defined Korean and English localization requirements.
- Defined HKCU-only registry integration rules.
- Defined GitHub Actions-centered validation rules.
- Defined release artifact verification rules.
- Updated agent tracking docs to reflect implementation start and current status.

### Changed`n`n- Added per-CLI enable selection in settings and persisted selected IDs through `EnabledCliIds`.`n- Updated launcher mode with explicit input validation and error dialog on failure.`n- Updated release workflow to upload `CliHere-win-x64.zip` to GitHub Release assets.`n- Updated README status from planning to implementation baseline.`n`n`n- Settings UI labels now use localization keys instead of hard-coded text.`n- Added CLI status column (Installed/Not installed) with refresh support.`n- Added per-CLI Install and Docs link actions.`n`n### Fixed`n`n- Added registry safety unit tests for `CliHere_` ownership filtering and run-command formatting.`n`n`n- Refactored terminal launch start-info creation and quoting path for safer test coverage.`n- Added unit tests for admin launch flags and quoted command argument generation.`n`n### Verification`n`n- `dotnet publish src/CliHere.App/CliHere.App.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -o dist/CliHere` passed.`n- Local zip `dist/CliHere-win-x64.zip` generated (size > 0).`n- Published exe start-up verified locally.`n

- `dotnet restore` passed.
- `dotnet build --configuration Release` passed.
- `dotnet test --configuration Release` passed (2 tests).




