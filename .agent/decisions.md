# Decisions

## 0001. Use WPF and C#

Decision: Use C#/.NET with WPF for the MVP.

Reason: The app is Windows-only and needs tight integration with Explorer, registry, PATH detection, process launching, and UAC elevation.

## 0002. Use HKCU registry keys only

Decision: Write context menu entries under HKCU only.

Reason: This avoids administrator requirements for menu registration and limits system-wide impact.

## 0003. Do not auto-install CLIs in MVP

Decision: The app will only detect installed CLIs and open official install pages.

Reason: Automatic installation increases security, trust, permissions, and maintenance risks.

## 0004. English README first

Decision: Root README.md is English-first, with Korean documentation in README.ko.md.

Reason: The app will be distributed publicly on GitHub and Reddit.

## 0005. Korean and English localization from day one

Decision: All user-facing strings must support Korean and English from the start.

Reason: Retrofitting localization later is error-prone and would weaken public distribution quality.

## 0006. Administrator mode is launch-time only

Decision: Administrator mode will elevate the terminal process at CLI launch time, not the app installation or context-menu registration process.

Reason: Normal usage should stay non-admin, while users who need elevated CLI sessions can opt in through settings.

## 0007. Expected asset is CliHere-win-x64.zip

Decision: The release workflow must produce `CliHere-win-x64.zip`.

Reason: Public users need an actual downloadable Windows artifact, not only source code.

## 0008. Keep launcher and UI in a single executable for MVP

Decision: `CliHere.exe` handles both settings UI and launcher mode (`run`) through startup argument parsing.

Reason: This matches architecture spec while keeping deployment and registry command composition simple.
