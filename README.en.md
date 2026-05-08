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

---

## Why CLI Here?

Running AI CLIs usually means:

1. Open terminal
2. `cd` into your folder
3. Run `gemini`, `codex`, `claude`, etc.

CLI Here removes those steps by adding Explorer context menu actions that open your selected CLI directly in the target folder.

---

## Highlights

- Context menu integration for both:
  - Folder right-click
  - Folder background right-click
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
- Explorer menu grouping under a single `CLI Here` parent menu
- One-click registry repair action
- GitHub Releases-based auto-update baseline

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
- Registry scope limited to `HKCU\Software\Classes`
- Only `CliHere_` owned keys are removed/updated

---

## Windows 11 Note

This MVP uses classic registry-based context menu integration.

On Windows 11, entries may appear under:

- `Show more options`

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
- [x] Repair action
- [x] PowerShell 7 mode
- [~] Auto-update hardening and rollout validation
- [ ] Windows 11 native top-level context menu research

---

## License

MIT
