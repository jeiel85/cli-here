# CLI 여기서 열기

**CLI Here / CLI 여기서 열기**는 Windows 탐색기 우클릭 메뉴에서 Gemini CLI, OpenCode, Claude Code, OpenAI Codex CLI 같은 AI 코딩 CLI를 현재 폴더 기준으로 바로 실행하는 가벼운 Windows 유틸리티입니다.

---

## 상태

현재 MVP 구현 베이스가 완료되었고 릴리즈 품질 보강을 진행 중입니다.

---

## 무엇을 하나요?

Windows 탐색기에 다음과 같은 우클릭 메뉴를 추가합니다.

- Gemini 여기서 열기
- OpenCode 여기서 열기
- Claude Code 여기서 열기
- Codex 여기서 열기

메뉴를 선택하면 해당 폴더를 작업 디렉터리로 하여 PowerShell 또는 Windows Terminal에서 선택한 CLI를 실행합니다.

---

## 지원 CLI

| CLI | 명령어 | 설치 감지 | 설치/문서 |
|---|---|---|---|
| Gemini CLI | `gemini` | PATH | https://google-gemini.github.io/gemini-cli/docs/get-started/ |
| OpenCode | `opencode` | PATH | https://opencode.ai/docs/cli/ |
| Claude Code | `claude` | PATH | https://docs.claude.com/en/docs/claude-code/setup |
| OpenAI Codex CLI | `codex` | PATH | https://help.openai.com/en/articles/11096431-openai-codex-ci-getting-started |

이 앱은 CLI를 자동 설치하지 않습니다. 설치 여부를 PATH 기준으로 감지하고, 설치가 필요하면 공식 설치 문서 링크만 제공합니다.

---

## MVP 구현 상태

- [x] WPF 설정 UI
- [x] 한국어/영어 UI
- [x] System/Korean/English 언어 모드
- [x] PATH 기반 CLI 설치 감지
- [x] 우클릭 메뉴 추가/해제
- [x] 폴더 빈 공간 우클릭 지원
- [x] 폴더 자체 우클릭 지원
- [x] PowerShell 실행
- [x] Windows Terminal 실행
- [x] 선택적 관리자 권한 실행
- [x] `%APPDATA%\\CliHere\\settings.json` 설정 저장
- [x] GitHub Actions 빌드/테스트
- [x] ZIP 릴리즈 산출물

---

## 관리자 권한 실행 옵션

CLI Here는 선택적으로 터미널을 관리자 권한으로 실행할 수 있습니다.

일반적인 설정과 우클릭 메뉴 등록에는 관리자 권한이 필요하지 않습니다. 우클릭 메뉴는 사용자 단위 레지스트리 영역인 아래 경로에 등록하는 것을 기본으로 합니다.

```text
HKCU\Software\Classes
```

관리자 권한 실행 옵션을 켜면, 우클릭 메뉴에서 CLI를 실행할 때 Windows UAC 권한 상승 창이 표시됩니다.

---

## Windows 11 우클릭 메뉴 안내

MVP는 표준 레지스트리 기반 탐색기 우클릭 메뉴 방식을 사용합니다.

Windows 11에서는 메뉴가 아래 위치에 표시될 수 있습니다.

```text
더 많은 옵션 표시
```

Windows 11 최신 우클릭 메뉴 최상단 네이티브 통합은 후속 기능으로 검토합니다.

---

## 보안 원칙

- 텔레메트리 없음
- 분석 도구 없음
- 백그라운드 네트워크 요청 없음
- CLI 자동 설치 없음
- 프로젝트 파일 스캔 없음
- 프로젝트 파일 업로드 없음
- 사용자 단위 레지스트리만 사용
- 앱이 만든 레지스트리 키만 삭제

---

## 개발

권장 기술 스택:

- C#
- WPF
- .NET 10 LTS
- xUnit
- GitHub Actions

기본 명령:

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

---

## 라이선스

MIT

