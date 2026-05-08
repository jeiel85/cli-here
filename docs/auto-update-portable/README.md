# Portable AutoUpdate (WPF + GitHub Releases)

`claude-usage-tray-windows` 에서 실제로 동작하는 자동업데이트 흐름을 그대로 떼어낸 휴대용 패키지다.
다른 WPF (.NET 9) 프로젝트에 복붙해서 동일한 사용자 경험을 그대로 재현할 수 있다.

---

## 1. 동작 원리 한 컷

```
[24h Timer / 시작 시 즉시]
        │
        ▼
UpdateService.CheckForUpdateAsync()
   ├─ GitHub Releases API 조회
   ├─ 현재 버전보다 새 버전이 있으면 release notes 합쳐서 반환
   └─ 실패는 UpdateCheckException(Kind=Network/Timeout/RateLimit/ApiError)
        │
        ▼ (있을 때)
UpdateDialog (Skip / Update 버튼 + 마크다운 렌더)
        │  사용자가 "Update" 누르면
        ▼
UpdateService.DownloadAndPrepareUpdateAsync()
   ├─ %TEMP%\{ProcessName}_new_{GUID}.exe 로 스트리밍 다운로드 (진행률 콜백)
   └─ SHA256 검증 (자산에 .sha256 / SHA256.txt 가 있으면)
        │
        ▼
UpdateService.ApplyPreparedUpdate()
   ├─ %TEMP% 에 PowerShell 자기-스왑 스크립트(ps1) 작성 (UTF-8 BOM)
   │   ① 호스트 종료 대기 (graceful 20초 → 강제 kill)
   │   ② 기존 EXE 삭제 → 새 EXE Move-Item (5회 재시도, 잠금 해소 대기)
   │   ③ 새 EXE Start-Process
   │   ④ ps1 자기 정리
   └─ Application.Current.Shutdown()  ← 호스트 종료, 이후는 ps1 가 처리
```

핵심 디자인 결정 두 가지:
- **HttpClient Timeout 15초** — 기본 100초는 사용자가 멈춘 줄 안다.
- **PowerShell 스크립트 UTF-8 BOM** — Windows PowerShell 5.1 이 한글 사용자 경로(예: `C:\Users\용은\...`)를 정확히 읽도록.

---

## 2. 패키지 구성

| 파일 | 역할 |
| --- | --- |
| `UpdateService.cs` | 업데이트 확인 / 다운로드+검증 / EXE 자기-스왑 코어 |
| `UpdateDialog.xaml` | 다크 테마 다이얼로그 (헤더 / 릴리스 노트 / Skip+Update / 진행률) |
| `UpdateDialog.xaml.cs` | 마크다운(`##`,`###`,`*`,`-`,`**bold**`,`` `code` ``) 자체 렌더 + 진행률 갱신 API |

> **외부 NuGet 의존성 0개.** `System.Text.Json`, `System.Net.Http`, `WPF` 만 사용.

---

## 3. 적용 순서 (5분)

### Step 1 — 파일 복사
세 파일을 호스트 프로젝트의 다음 경로로 복사:

```
YourApp/
  Services/UpdateService.cs
  Views/UpdateDialog.xaml
  Views/UpdateDialog.xaml.cs
```

### Step 2 — 네임스페이스 정리
파일 첫 줄 `namespace YourApp.Services;` / `namespace YourApp.Views;` 를 본인 프로젝트 네임스페이스로 변경. XAML 의 `x:Class="YourApp.Views.UpdateDialog"` 도 같이.

### Step 3 — `UpdateService.cs` 상단 const 4개 수정
```csharp
private const string Repo          = "owner/repo";        // GitHub 저장소
private const string AssetExeName  = "YourApp.exe";       // Release 자산 EXE 이름
private const string ProcessName   = "YourApp";           // 실행 중 프로세스 이름 (확장자 제외)
private const string UserAgent     = "YourApp-Updater";   // GitHub API User-Agent (필수)
```

### Step 4 — `csproj` 에 어셈블리 버전 명시
`UpdateService.CurrentVersion` 은 `Assembly.GetName().Version` 을 읽는다. Release 태그(`v1.2.3`) 와 일치시킬 것.

```xml
<PropertyGroup>
  <Version>1.2.3</Version>
  <AssemblyVersion>1.2.3.0</AssemblyVersion>
  <FileVersion>1.2.3.0</FileVersion>
</PropertyGroup>
```

### Step 5 — ViewModel / App 시작 시 호출 (스니펫은 §4)

### Step 6 — GitHub Release 자산 업로드 규약
릴리스 만들 때 두 파일 첨부:
- `YourApp.exe` (이름은 `AssetExeName` 과 정확히 일치, 대소문자 무시)
- `YourApp.exe.sha256` 또는 `SHA256.txt` (한 줄, 첫 토큰이 hex 64자 — `Get-FileHash` 출력 그대로 OK)

GitHub Actions 예시 한 줄:
```bash
sha256sum YourApp.exe > YourApp.exe.sha256
```

---

## 4. 호스트 통합 — ViewModel 스니펫

본 리포 `MainViewModel.cs` 에서 검증된 패턴을 그대로 옮겼다. CommunityToolkit.Mvvm 사용 가정.

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly UpdateService _updater = new();
    private readonly Timer _updateTimer;

    private string _updateDownloadUrl = "";
    private string _updateSha256Url = "";

    [ObservableProperty] private bool _isUpdating;
    [ObservableProperty] private bool _updateAvailable;
    [ObservableProperty] private string _updateLabel = "";
    [ObservableProperty] private string _updateCheckLabel = "";

    public string CurrentVersionLabel => $"v{UpdateService.CurrentVersion.ToString(3)}";

    public MainViewModel()
    {
        // 24시간마다 백그라운드 체크
        _updateTimer = new Timer(TimeSpan.FromHours(24).TotalMilliseconds);
        _updateTimer.Elapsed += async (_, _) => await BackgroundCheckAsync();
        _updateTimer.AutoReset = true;
    }

    public async Task OnAppStartedAsync()
    {
        _updateTimer.Start();
        _ = BackgroundCheckAsync();   // 시작 시 즉시 1회
    }

    /// <summary>메뉴/버튼에서 수동 호출 — 에러를 사용자에게 분류해서 보여준다.</summary>
    [RelayCommand]
    public async Task ManualCheckForUpdateAsync()
    {
        if (IsUpdating) return;
        UpdateCheckLabel = "업데이트 확인 중...";

        UpdateService.UpdateInfo? result;
        try { result = await _updater.CheckForUpdateAsync(); }
        catch (UpdateCheckException uex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var msg = uex.Kind switch
                {
                    UpdateCheckErrorKind.Network    => "인터넷 연결을 확인하세요.",
                    UpdateCheckErrorKind.Timeout    => "응답이 지연됩니다. 잠시 후 다시 시도하세요.",
                    UpdateCheckErrorKind.RateLimit  => $"GitHub API 호출 한도 초과. {uex.RetryAtLocal} 이후 다시 시도하세요.",
                    UpdateCheckErrorKind.ApiError   => $"GitHub API 오류 (HTTP {uex.StatusCode}).",
                    _                               => "업데이트 확인 실패."
                };
                MessageBox.Show(msg, "업데이트", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            UpdateCheckLabel = "";
            return;
        }
        catch { UpdateCheckLabel = ""; return; }

        if (result is null)
        {
            UpdateCheckLabel = $"최신 버전 ({CurrentVersionLabel})";
            return;
        }
        UpdateCheckLabel = "";
        OfferUpdate(result);
    }

    /// <summary>타이머 백그라운드 체크 — 조용히 실패하고, 새 버전이 있을 때만 다이얼로그.</summary>
    private async Task BackgroundCheckAsync()
    {
        if (IsUpdating) return;
        UpdateService.UpdateInfo? result;
        try { result = await _updater.CheckForUpdateAsync(); }
        catch { return; }
        if (result is null) return;
        await Application.Current.Dispatcher.InvokeAsync(() => OfferUpdate(result));
    }

    private void OfferUpdate(UpdateService.UpdateInfo info)
    {
        var versionStr = info.version.ToString(3);

        // 사용자가 "이 버전 건너뛰기" 누른 버전은 다시 띄우지 않음 (settings 에 저장)
        if (IsVersionSkipped(versionStr)) return;

        _updateDownloadUrl = info.downloadUrl;
        _updateSha256Url   = info.sha256Url;
        UpdateLabel        = $"새 버전 v{versionStr} 사용 가능";
        UpdateAvailable    = true;

        var dialog = new UpdateDialog(
            info.releaseNotes,
            onSkip: () => SkipVersion(versionStr));

        dialog.OnUpdateRequested += () =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    IsUpdating = true;
                    var tempPath = await _updater.DownloadAndPrepareUpdateAsync(
                        _updateDownloadUrl, _updateSha256Url,
                        (pc, status) => dialog.UpdateProgress(pc, status));

                    dialog.UpdateProgress(100, "Restarting...");
                    await Task.Delay(500);
                    _updater.ApplyPreparedUpdate(tempPath);   // ← 여기서 호스트가 종료됨
                }
                catch (Exception ex)
                {
                    IsUpdating = false;
                    dialog.ShowError(ex.Message);
                }
            });
        };

        dialog.Show();
    }

    // 아래 두 메서드는 본인의 settings 저장소에 맞게 구현
    private bool IsVersionSkipped(string version) { /* settings 조회 */ return false; }
    private void SkipVersion(string version)      { /* settings 에 저장 */ }
}
```

---

## 5. 커스터마이징 포인트

| 원하는 것 | 어디서 바꾸나 |
| --- | --- |
| API 타임아웃 변경 | `UpdateService.cs` `Http = new() { Timeout = ... }` |
| 사전 릴리스(prerelease)도 받기 | `CheckForUpdateAsync` 에서 `if (... pre.GetBoolean()) continue;` 제거 |
| SHA256 누락 시 강제 실패 | `DownloadAndPrepareUpdateAsync` 의 `catch (Exception ex) { Debug.WriteLine ... }` 를 `throw;` 로 |
| 종료 대기 시간 변경 | ps1 의 `$timeout = 20` |
| Move 재시도 횟수 | ps1 의 `$retry = 5` |
| 다이얼로그 라벨 한국어 → 영어 등 | `new UpdateDialog(..., title:"...", skipLabel:"...", updateLabel:"...", errorTitle:"...")` |
| 다크 테마 색상 | `UpdateDialog.xaml` 상단 `Window.Resources` 의 `SolidColorBrush` 들 |
| 마크다운 렌더 확장 | `UpdateDialog.xaml.cs` `RenderMarkdown` / `ParseInlines` |

---

## 6. 알려진 제약 / 트레이드오프

- **WPF 전용** — `Application.Current.Shutdown`, `Dispatcher`, FlowDocument 사용. WinForms / Avalonia 로 옮기려면 다이얼로그와 종료 호출만 갈아끼우면 됨 (코어 로직은 그대로).
- **PowerShell 의존** — `powershell.exe` 가 PATH 에 있어야 함 (Windows 기본 보장). `pwsh` 만 있는 환경이면 `ProcessStartInfo` 의 `"powershell.exe"` 를 `"pwsh"` 로.
- **GitHub API 무인증 60/h** — 사용자 한 명 기준이면 24시간 1회 + 수동 체크 정도라 충분. 회사 네트워크에서 NAT 공유로 한도 초과가 자주 나면 `Authorization: token ...` 헤더 추가 고려.
- **관리자 권한 / 보호된 경로** — `Program Files` 등에 설치된 EXE 라면 ps1 안에서 `Move-Item` 이 권한 부족으로 실패한다. 일반 사용자 경로(`%LocalAppData%`, `%UserProfile%\AppData\...`) 설치를 권장.
- **백신 false positive** — 릴리스 EXE 가 코드사인되지 않으면 일부 백신이 차단할 수 있다. SHA256 자산을 함께 올리는 이유 중 하나.

---

## 7. 디버깅

업데이트 도중 EXE 가 안 바뀌었다면 PowerShell 스크립트 로그를 보면 원인이 뜬다:

```
%TEMP%\{ProcessName}_update_debug.log
```

예시 출력:
```
Update started at 05/08/2026 10:23:11
10:23:11 - Waiting for process to exit...
10:23:13 - Replacing executable...
10:23:13 - Move successful.
10:23:13 - Starting new version...
10:23:13 - Update complete.
```

`Move failed: ... is being used by another process` 가 반복되면 종료 대기 시간(ps1 `$timeout`) 을 늘리거나, 호스트 앱이 종료 시 잡고 있던 파일 핸들을 명시적으로 닫는지 확인.

---

## 8. 참고: 이 리포의 원본

| portable | 원본 |
| --- | --- |
| `UpdateService.cs` | `ClaudeUsageTray/Services/UpdateService.cs` |
| `UpdateDialog.xaml` | `ClaudeUsageTray/Views/UpdateDialog.xaml` |
| `UpdateDialog.xaml.cs` | `ClaudeUsageTray/Views/UpdateDialog.xaml.cs` |
| (참고) ViewModel 통합 | `ClaudeUsageTray/ViewModels/MainViewModel.cs` |

원본에는 `Loc.*` (i18n), `IsVersionSkipped` 의 `SettingsService` 연동, 토스트 알림 분기 등 호스트 앱 고유 코드가 추가로 묶여 있다. portable 사본은 이런 의존성을 모두 분리해 다른 프로젝트에서도 const 4개 + ViewModel 스니펫만으로 동작하도록 만든 것.

부록으로 본 리포에는 별도 EXE 형태의 `ClaudeUsageTray.Updater` 프로젝트도 있다(인자 5개 받아 진행률 윈도우 띄우는 독립 업데이터). 호스트 앱이 차지한 EXE 자체를 자기가 갈아끼우는 게 정책상 곤란할 때 유용하지만, 본 리포에서도 실사용 경로는 위의 인라인 PowerShell 방식 한 가지다. 휴대용 사본도 그쪽으로 통일했다.
