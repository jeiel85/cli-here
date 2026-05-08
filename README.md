# CLI Here

**CLI Here** is a lightweight Windows utility that lets you launch AI coding CLIs directly from File Explorer's right-click menu.

Korean name: **CLI 여기서 열기**

> Open Gemini, OpenCode, Claude Code, or OpenAI Codex CLI in the current folder without opening PowerShell and typing `cd` manually.

---

## Status

This project has an MVP implementation baseline and is actively being hardened for release.

---

## What it does

CLI Here adds optional context menu entries to Windows File Explorer:

- Open Gemini here
- Open OpenCode here
- Open Claude Code here
- Open Codex here

When selected, CLI Here launches the configured terminal in the selected folder and starts the chosen CLI.

---

## Supported CLIs

| CLI | Command | Detection | Install/docs |
|---|---|---|---|
| Gemini CLI | `gemini` | PATH | https://google-gemini.github.io/gemini-cli/docs/get-started/ |
| OpenCode | `opencode` | PATH | https://opencode.ai/docs/cli/ |
| Claude Code | `claude` | PATH | https://docs.claude.com/en/docs/claude-code/setup |
| OpenAI Codex CLI | `codex` | PATH | https://help.openai.com/en/articles/11096431-openai-codex-ci-getting-started |

CLI Here does not install these CLIs automatically. It only detects whether they are available from PATH and links to the official install pages.

---

## MVP implementation status

- [x] WPF settings UI
- [x] Korean and English UI
- [x] System/Korean/English language mode
- [x] CLI installation detection from PATH
- [x] Context menu add/remove
- [x] Folder background right-click support
- [x] Folder right-click support
- [x] PowerShell launch mode
- [x] Windows Terminal launch mode
- [x] Optional administrator launch mode
- [x] JSON settings under `%APPDATA%\\CliHere\\settings.json`
- [x] GitHub Actions build/test workflow
- [x] ZIP release asset

---

## Administrator mode

CLI Here can optionally launch the selected terminal as administrator.

This does not mean CLI Here needs admin rights for normal setup. Context menu registration is planned to use user-level registry keys under:

```text
HKCU\Software\Classes
```

When administrator mode is enabled, Windows will show a UAC prompt when launching a CLI from the context menu.

---

## Windows 11 context menu note

The MVP uses standard registry-based File Explorer context menu integration.

On Windows 11, these entries may appear under:

```text
Show more options
```

Native top-level Windows 11 context menu integration is a future enhancement.

---

## Security principles

- No telemetry
- No analytics
- No background network requests
- No automatic CLI installation
- No project file scanning
- No project file upload
- User-level registry keys only
- Only registry keys created by this app are removed

---

## Localization

CLI Here supports:

- English
- Korean

The root README is English-first for public GitHub and Reddit distribution. Korean documentation is available in `README.ko.md`.

---

## Development

Recommended stack:

- C#
- WPF
- .NET 10 LTS
- xUnit
- GitHub Actions

Common commands:

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

---

## License

MIT

