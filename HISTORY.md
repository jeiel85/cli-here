# History

## 2026-05-08

- 작업: CLI Here / CLI 여기서 열기 초기 설계 묶음 작성
- 변경 파일:
  - AGENTS.md: 최신 범용 에이전트 지침과 CLI Here 프로젝트 전용 규칙 통합
  - README.md: 영어 우선 공개 배포용 소개 문서 작성
  - README.ko.md: 한국어 사용자 문서 작성
  - docs/: 제품 명세, 아키텍처, 로드맵, 브랜딩, 로컬라이징, 보안, 릴리즈, 에이전트 프롬프트 작성
  - .agent/: 작업 목록, 진행 기록, 결정 기록 작성
  - CHANGELOG.md: v0.1.0 계획 항목 작성
- 검증: 문서 묶음 생성 확인
- 결과: 성공
- 후속 작업: 실제 .NET WPF 프로젝트 초기화 및 GitHub 저장소 푸시

## 2026-05-08

- 작업: MVP 초기 구현 시작 (솔루션/앱/서비스/런처/테스트 골격)
- 변경 파일:
  - CliHere.sln
  - src/CliHere.App/: WPF 앱 기본 구조, 모델/서비스/뷰모델, 로컬라이징 리소스, run 런처 모드
  - src/CliHere.Tests/: 기본 단위 테스트 2건
  - .agent/tasks.md, .agent/progress.md, .agent/decisions.md
  - HISTORY.md, CHANGELOG.md
- 검증:
  - `dotnet restore`
  - `dotnet build --configuration Release`
  - `dotnet test --configuration Release`
- 결과: 성공 (테스트 2건 통과)
- 후속 작업:
  - UI 완전 로컬라이징 바인딩 적용
  - 레지스트리/런처 인자 quoting 및 관리자 실행 경로 테스트 확장

- 작업: UI 로컬라이징 바인딩 전환 및 CLI 상태/링크 액션 추가
- 변경 파일:
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/ViewModels/CliItemViewModel.cs
  - src/CliHere.App/App.xaml.cs
  - src/CliHere.App/Resources/Languages/en.json
  - src/CliHere.App/Resources/Languages/ko.json
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
- 결과: 성공


- 작업: TerminalLauncher quoting/관리자 실행 테스트 확장 및 실행 파일 기동 확인
- 변경 파일:
  - src/CliHere.App/Services/TerminalLauncher.cs
  - src/CliHere.App/CliHere.App.csproj
  - src/CliHere.Tests/TerminalLauncherTests.cs
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (4 tests)
  - Release exe 실행 확인: CliHere.App.exe (PID 기동 확인 후 종료)
- 결과: 성공


- 작업: ContextMenuRegistryService 소유 키 안전성 테스트 추가
- 변경 파일:
  - src/CliHere.App/Services/ContextMenuRegistryService.cs
  - src/CliHere.Tests/ContextMenuRegistryServiceTests.cs
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (7 tests)
- 결과: 성공


- 작업: 배포 준비 고도화 (선택 등록/런처 안정성/릴리즈 워크플로/퍼블리시 검증)
- 변경 파일:
  - src/CliHere.App/Services/ITerminalLauncher.cs
  - src/CliHere.App/Services/LauncherService.cs
  - src/CliHere.App/Services/SettingsService.cs
  - src/CliHere.App/Services/TerminalLauncher.cs
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/ViewModels/CliItemViewModel.cs
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/App.xaml.cs
  - src/CliHere.App/Resources/Languages/en.json
  - src/CliHere.App/Resources/Languages/ko.json
  - src/CliHere.Tests/LauncherServiceTests.cs
  - src/CliHere.Tests/ContextMenuRegistryServiceTests.cs
  - .github/workflows/release.yml
  - README.md, README.ko.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (10 tests)
  - dotnet publish ... win-x64 self-contained single-file
  - dist/CliHere-win-x64.zip 생성 및 크기 확인
  - Publish exe 실행 확인
- 결과: 성공


- 작업: 배포 정합성 보강 (출력 exe 명/CI SDK 버전 정렬)
- 변경 파일:
  - src/CliHere.App/CliHere.App.csproj
  - .github/workflows/build.yml
  - .github/workflows/release.yml
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (10 tests)
  - dotnet publish ... win-x64 self-contained single-file
  - dist/CliHere/CliHere.exe 실행 확인
- 결과: 성공

