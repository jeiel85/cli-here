# CLI Here

<p align="center">
  <b>Launch AI coding CLIs from Windows File Explorer in one click.</b><br/>
  <sub>English name: <b>CLI Here</b> · Korean name: <b>CLI 여기서 열기</b></sub>
</p>

<p align="center">
  <a href="https://github.com/jeiel85/cli-here/actions/workflows/build.yml"><img alt="Build" src="https://img.shields.io/github/actions/workflow/status/jeiel85/cli-here/build.yml?branch=main&label=build"></a>
  <a href="https://github.com/jeiel85/cli-here/releases"><img alt="Release" src="https://img.shields.io/github/v/release/jeiel85/cli-here"></a>
  <a href="https://github.com/jeiel85/cli-here/blob/main/LICENSE"><img alt="License" src="https://img.shields.io/github/license/jeiel85/cli-here"></a>
  <a href="https://github.com/jeiel85/cli-here/stargazers"><img alt="Stars" src="https://img.shields.io/github/stars/jeiel85/cli-here?style=social"></a>
</p>

<p align="center">
  <img alt="CLI Here main window" src="docs/images/app-main.png" width="900">
</p>

---

## Why CLI Here?

Running AI CLIs usually means:

1. Open terminal
2. `cd` into your folder
3. Run `gemini`, `codex`, `claude`, etc.

CLI Here removes those steps by adding Explorer context menu actions that open your selected CLI directly in the target folder.

---

## Highlights

- Context menu integration in three contexts:
  - Folder right-click
  - Folder background right-click
  - Desktop background right-click
- Built-in CLIs:
  - Gemini CLI
  - OpenCode
  - Claude Code
  - OpenAI Codex CLI
- Custom CLI definitions (add/remove)
- Terminal modes:
  - Windows Terminal
  - PowerShell
  - PowerShell 7 (`pwsh`, fallback-aware)
- Optional administrator launch (`runas` + UAC)
- Explorer menu grouping under a single `CLI Here` cascade parent
- One-click Repair / Remove-all registry actions
- GitHub Releases-based auto-update baseline
- Robust CLI detection: process PATH + registry PATH (HKCU/HKLM) + well-known fallback folders (npm, `.local/bin`, `.cargo/bin`, `.dotnet/tools`, etc.)
- Blue dark theme with a fully themed in-app message dialog

---

## Runtime Requirement

Default release packaging uses **framework-dependent publish** (`self-contained=false`) to keep downloads small.

You need:

- **.NET 9 Desktop Runtime (x64)** on the target Windows machine

---

## Installation (Release)

1. Download latest assets from [Releases](https://github.com/jeiel85/cli-here/releases/latest)
2. Use:
   - `CliHere-win-x64.zip` for normal app delivery
   - `CliHere.exe` + `CliHere.exe.sha256` for update flow verification/debugging
3. Extract and run `CliHere.exe`
4. Configure options and click `Apply`

---

## Supported CLI Table

| CLI | Command | Detection | Install / Docs |
|---|---|---|---|
| Gemini CLI | `gemini` | PATH | https://google-gemini.github.io/gemini-cli/docs/get-started/ |
| OpenCode | `opencode` | PATH | https://opencode.ai/docs/cli/ |
| Claude Code | `claude` | PATH | https://docs.claude.com/en/docs/claude-code/setup |
| OpenAI Codex CLI | `codex` | PATH | https://help.openai.com/en/articles/11096431-openai-codex-ci-getting-started |

CLI Here does **not** auto-install third-party CLIs. It only detects availability and provides official links.

---

## Security Principles

- No telemetry
- No analytics SDKs
- No background network scanning of projects
- No project file upload
- Registry scope limited to `HKLM\SOFTWARE\Classes`
- Only `CliHere_`-prefixed keys are touched (never modifies other apps' menu entries)

---

## Apply requires UAC

Clicking **Apply / Repair / Remove all** triggers a **one-time UAC prompt**.

- Starting with Windows 11 25H2, the shell stopped honoring per-user (HKCU) context menu entries (Microsoft tightened shell security). The integration must therefore live in the system hive (HKLM).
- The app spawns a short-lived elevated child process that performs only the registry update and exits — the main window keeps running unprivileged.
- Declining UAC is safe; the app shows a friendly message and continues.

---

## How to find the menu on Windows 11

The Windows 11 **new (default) right-click menu hides classic registry entries by design**. Use either of:

1. `Shift` + right-click → classic menu opens immediately
2. Right-click → click `Show more options` at the bottom

Hover `CLI Here` in the classic menu to expand the cascade with all four CLIs.

> Windows 10 classic menu users will see the entry without any extra step.

---

## Localization

- English
- Korean

Korean docs: [README.ko.md](./README.ko.md)

---

## Development

### Tech Stack

- C# / .NET 9 / WPF
- xUnit
- GitHub Actions

### Commands

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

---

## Roadmap Snapshot

- [x] MVP foundation + context menu + launcher modes
- [x] Custom CLI definitions
- [x] Repair / Remove-all actions
- [x] PowerShell 7 mode
- [x] Blue dark theme + themed in-app message dialog
- [x] HKLM registration with one-time UAC elevation (Win11 25H2 compatible)
- [x] Robust CLI detection (registry PATH + well-known folder fallbacks)
- [x] Desktop-background context support
- [~] Auto-update hardening and rollout validation
- [ ] Windows 11 *new* right-click menu (`IExplorerCommand` COM DLL / MSIX Sparse Package — separate effort)

---

## License

MIT
