# CLI Here / CLI 여기서 열기

<p align="center">
  <b>Windows 탐색기에서 AI 코딩 CLI를 한 번에 실행하세요.</b><br/>
  <sub>한국어 기본 문서 · English docs available below</sub>
</p>

<p align="center">
  <a href="./README.md"><b>한국어</b></a> ·
  <a href="./README.en.md"><b>English</b></a>
</p>

<p align="center">
  <a href="https://jeiel85.github.io/cli-here/"><b>GitHub Pages</b></a>
</p>

<p align="center">
  <a href="https://github.com/jeiel85/cli-here/actions/workflows/build.yml"><img alt="Build" src="https://img.shields.io/github/actions/workflow/status/jeiel85/cli-here/build.yml?branch=main&label=build"></a>
  <a href="https://github.com/jeiel85/cli-here/releases"><img alt="Release" src="https://img.shields.io/github/v/release/jeiel85/cli-here"></a>
  <a href="https://github.com/jeiel85/cli-here/blob/main/LICENSE"><img alt="License" src="https://img.shields.io/github/license/jeiel85/cli-here"></a>
  <a href="https://github.com/jeiel85/cli-here/stargazers"><img alt="Stars" src="https://img.shields.io/github/stars/jeiel85/cli-here?style=social"></a>
</p>

<p align="center">
  <img alt="CLI Here 메인 화면" src="docs/images/app-main.png" width="900">
</p>

---

## 왜 CLI Here인가요?

보통 AI CLI를 실행하려면:

1. 터미널 열기
2. 원하는 폴더로 `cd`
3. `gemini`, `codex`, `claude` 같은 명령 실행

CLI Here는 이 과정을 탐색기 우클릭으로 줄여줍니다.

---

## 주요 기능

- 탐색기 우클릭 통합:
  - 폴더 우클릭
  - 폴더 빈 공간 우클릭
  - 바탕화면 우클릭
- 기본 지원 CLI:
  - Gemini CLI
  - OpenCode
  - Claude Code
  - OpenAI Codex CLI
- 사용자 정의 CLI 추가/삭제
- 터미널 모드:
  - Windows Terminal
  - PowerShell
  - PowerShell 7 (`pwsh` 감지/fallback)
- 선택적 관리자 권한 실행(UAC)
- `CLI Here` 상위 메뉴 그룹화 (cascade)
- 레지스트리 복구(Repair) / 전체 제거 기능
- GitHub Releases 기반 자동업데이트 베이스라인
- 견고한 CLI 감지: process PATH + 레지스트리 PATH(HKCU/HKLM) + npm/`.local/bin`/`.cargo/bin`/`.dotnet/tools` 등 잘 알려진 폴더 fallback
- 블루 톤 다크 테마 + 자체 테마 메시지 박스

---

## 런타임 요구사항

기본 릴리즈는 용량을 줄이기 위해 framework-dependent(`self-contained=false`) 방식입니다.

대상 Windows PC에 아래가 필요합니다.

- **.NET 9 Desktop Runtime (x64)**

---

## 설치(릴리즈)

1. [Releases](https://github.com/jeiel85/cli-here/releases/latest)에서 최신 자산 다운로드
2. 보통은 `CliHere-win-x64.zip` 사용
3. 압축 해제 후 `CliHere.exe` 실행
4. 설정 후 `Apply` 클릭

자동업데이트 검증/디버깅용 자산:
- `CliHere.exe`
- `CliHere.exe.sha256`

---

## 지원 CLI

| CLI | 명령어 | 감지 방식 | 설치/문서 |
|---|---|---|---|
| Gemini CLI | `gemini` | PATH | https://google-gemini.github.io/gemini-cli/docs/get-started/ |
| OpenCode | `opencode` | PATH | https://opencode.ai/docs/cli/ |
| Claude Code | `claude` | PATH | https://docs.claude.com/en/docs/claude-code/setup |
| OpenAI Codex CLI | `codex` | PATH | https://help.openai.com/en/articles/11096431-openai-codex-ci-getting-started |

CLI Here는 서드파티 CLI를 자동 설치하지 않고, 감지 및 공식 링크만 제공합니다.

---

## 보안 원칙

- 텔레메트리 없음
- 분석 SDK 없음
- 프로젝트 파일 백그라운드 스캔 없음
- 프로젝트 파일 업로드 없음
- 레지스트리는 `HKLM\SOFTWARE\Classes` 범위만 사용
- `CliHere_` 소유 키만 정리/수정 (다른 앱의 컨텍스트 메뉴 항목은 절대 건드리지 않음)

---

## 적용 시 관리자 권한 (UAC)

`적용 / 복구 / 전체 제거`를 누르면 **UAC 프롬프트가 1회 표시**됩니다.

- Windows 11 25H2부터 셸이 사용자 영역(HKCU)의 컨텍스트 메뉴 항목을 더 이상 인식하지 않습니다 (Microsoft의 셸 보안 강화). 따라서 시스템 영역(HKLM)에 등록해야 우클릭 메뉴에 정상적으로 표시됩니다.
- 앱은 자체적으로 자식 프로세스를 관리자 권한으로 재실행해 등록만 처리하고 즉시 종료하므로, 메인 앱은 일반 권한으로 계속 실행됩니다.
- UAC를 거부해도 앱은 정상 동작하며, 안내 메시지가 표시됩니다.

---

## Windows 11 우클릭 메뉴에서 보는 방법

Windows 11의 **새(기본) 우클릭 메뉴**는 시스템 정책상 클래식 레지스트리 기반 항목을 모두 숨깁니다. 다음 두 방법 중 하나로 클래식 메뉴를 펼쳐주세요.

1. `Shift` + 우클릭 → 클래식 메뉴 즉시 표시
2. 일반 우클릭 → 메뉴 맨 아래 `더 많은 옵션 표시 (Show more options)` 클릭

클래식 메뉴에서 `CLI 여기서 열기` 항목 위에 마우스를 올리면 4개 CLI cascade가 펼쳐집니다.

> Windows 10 클래식 메뉴 사용자는 추가 단계 없이 바로 표시됩니다.

---

## 개발

### 스택

- C# / .NET 9 / WPF
- xUnit
- GitHub Actions

### 명령

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

---

## 로드맵 요약

- [x] MVP 기반 + 컨텍스트 메뉴 + 런처
- [x] 사용자 CLI 정의
- [x] Repair / 전체 제거 액션
- [x] PowerShell 7 모드
- [x] 블루 다크 테마 + 자체 테마 메시지 박스
- [x] HKLM + UAC 1회 elevation 등록 (Win11 25H2 호환)
- [x] 견고한 CLI 감지 (registry PATH + well-known 폴더 fallback)
- [x] 바탕화면 우클릭 컨텍스트 지원
- [~] 자동업데이트 고도화/검증
- [ ] Windows 11 새 우클릭 메뉴 노출 (`IExplorerCommand` COM DLL / MSIX Sparse Package — 별도 작업)

---

## 라이선스

MIT
