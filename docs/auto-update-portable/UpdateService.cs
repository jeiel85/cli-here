using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace YourApp.Services;

// ============================================================================
//  Portable AutoUpdate — UpdateService
//  ----------------------------------------------------------------------------
//  GitHub Releases 기반 자동업데이트 코어. WPF (.NET 9) 기준.
//
//  적용 시 변경할 곳: 아래 [TODO] 표시된 const 4개만 수정하면 동작한다.
//
//  GitHub Release 자산 규약:
//   - {AssetExeName}                          : 실행 파일 (필수)
//   - {AssetExeName}.sha256  또는  SHA256.txt : SHA-256 해시 (선택, 강력 권장)
// ============================================================================

/// <summary>업데이트 확인 실패 원인 분류 — UI 가 카테고리별로 안내문구를 라우팅한다.</summary>
public enum UpdateCheckErrorKind
{
    Unknown,
    Network,     // 네트워크 도달 불가
    Timeout,     // 응답 지연
    RateLimit,   // GitHub API 무인증 60/h 초과 (HTTP 403 + "rate limit")
    ApiError,    // 그 외 GitHub API 측 에러 (HTTP 4xx/5xx)
}

/// <summary>분류된 업데이트 확인 예외 — Kind 별로 사용자 안내문구가 갈라진다.</summary>
public class UpdateCheckException : Exception
{
    public UpdateCheckErrorKind Kind { get; }
    public int? StatusCode { get; }
    public string? RetryAtLocal { get; }   // RateLimit 시 사용자 시간대 "HH:mm" 문자열

    public UpdateCheckException(string message, UpdateCheckErrorKind kind,
                                int? statusCode = null, string? retryAtLocal = null,
                                Exception? inner = null)
        : base(message, inner)
    {
        Kind = kind;
        StatusCode = statusCode;
        RetryAtLocal = retryAtLocal;
    }
}

public class UpdateService
{
    // ====== [TODO] 프로젝트마다 수정할 4가지 ======
    private const string Repo          = "owner/repo";        // 예: "jeiel85/claude-usage-tray-windows"
    private const string AssetExeName  = "YourApp.exe";       // GitHub Release 자산 파일 이름
    private const string ProcessName   = "YourApp";           // 실행 중 프로세스 이름 (확장자 제외)
    private const string UserAgent     = "YourApp-Updater";   // GitHub API User-Agent (필수)
    // ==============================================

    private const string ApiListUrl   = $"https://api.github.com/repos/{Repo}/releases?per_page=30";
    public  const string ReleasePage  = $"https://github.com/{Repo}/releases/latest";

    // 100초 기본은 너무 길어 사용자가 멈춘 줄 안다 — 15초로 단축
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public record UpdateInfo(Version version, string downloadUrl, string sha256Url, string releaseNotes);

    static UpdateService()
    {
        Http.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

    /// <summary>
    /// 더 새로운 릴리스가 있으면 UpdateInfo 반환, 최신이면 null.
    /// 분류 가능한 실패는 <see cref="UpdateCheckException"/> 으로 던진다.
    /// 릴리스 노트는 현재 버전과 최신 버전 사이의 모든 버전을 합쳐서 돌려준다.
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        HttpResponseMessage response;
        string rawJson;
        try
        {
            response = await Http.GetAsync(ApiListUrl);
            rawJson = await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException ex)
        {
            throw new UpdateCheckException(ex.Message, UpdateCheckErrorKind.Timeout, inner: ex);
        }
        catch (HttpRequestException ex)
        {
            throw new UpdateCheckException(ex.Message, UpdateCheckErrorKind.Network, inner: ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            int code = (int)response.StatusCode;
            // GitHub 무인증 rate limit: 403 + body의 "rate limit" 패턴 + X-RateLimit-Remaining: 0
            bool isRateLimit = code == 403 &&
                               (rawJson.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
                                (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remVals) &&
                                 remVals.FirstOrDefault() == "0"));

            if (isRateLimit)
            {
                string? retryAt = null;
                // 우선순위: X-RateLimit-Reset (epoch) > Retry-After (delta seconds)
                if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetVals) &&
                    long.TryParse(resetVals.FirstOrDefault(), out var epoch))
                {
                    retryAt = DateTimeOffset.FromUnixTimeSeconds(epoch).ToLocalTime().ToString("HH:mm");
                }
                else if (response.Headers.TryGetValues("Retry-After", out var raVals) &&
                         int.TryParse(raVals.FirstOrDefault(), out var raSec))
                {
                    retryAt = DateTimeOffset.UtcNow.AddSeconds(raSec).ToLocalTime().ToString("HH:mm");
                }
                throw new UpdateCheckException(
                    "GitHub API rate limit exceeded (60/h, unauthenticated).",
                    UpdateCheckErrorKind.RateLimit,
                    statusCode: code,
                    retryAtLocal: retryAt);
            }

            throw new UpdateCheckException(
                $"GitHub API returned HTTP {code}.",
                UpdateCheckErrorKind.ApiError,
                statusCode: code);
        }

        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("message", out var msgEl))
            throw new UpdateCheckException(
                $"GitHub API: {msgEl.GetString()}",
                UpdateCheckErrorKind.ApiError);

        // 현재 버전보다 새로운 릴리스를 모두 모아서 최신순 정렬
        var newer = new List<(Version ver, string tag, string body, JsonElement element)>();
        foreach (var rel in root.EnumerateArray())
        {
            if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
            if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;

            var tag = rel.TryGetProperty("tag_name", out var tagEl) ? tagEl.GetString() ?? "" : "";
            var verStr = tag.TrimStart('v');
            if (!Version.TryParse(verStr, out var ver)) continue;
            if (ver <= CurrentVersion) continue;

            var body = rel.TryGetProperty("body", out var bodyEl) ? bodyEl.GetString() ?? "" : "";
            newer.Add((ver, tag, body, rel));
        }

        if (newer.Count == 0) return null;

        newer.Sort((a, b) => b.ver.CompareTo(a.ver)); // newest first
        var latest = newer[0];

        // 합쳐진 릴리스 노트 — 버전마다 헤더(##)
        var notesBuilder = new System.Text.StringBuilder();
        foreach (var (ver, tag, body, _) in newer)
        {
            if (string.IsNullOrWhiteSpace(body)) continue;
            if (notesBuilder.Length > 0) notesBuilder.AppendLine();
            notesBuilder.AppendLine($"## {tag}");
            notesBuilder.Append(body);
        }

        string? exeUrl = null;
        string? sha256Url = null;

        foreach (var asset in latest.element.GetProperty("assets").EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString() ?? "";
            var url  = asset.GetProperty("browser_download_url").GetString() ?? "";

            if (name.Equals(AssetExeName, StringComparison.OrdinalIgnoreCase))
                exeUrl = url;
            else if (name.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase) ||
                     name.Equals("SHA256.txt", StringComparison.OrdinalIgnoreCase))
                sha256Url = url;
        }

        if (exeUrl is null) return null;

        return new UpdateInfo(latest.ver, exeUrl,
            sha256Url ?? "", notesBuilder.ToString().TrimEnd());
    }

    /// <summary>
    /// 새 EXE 를 임시 위치에 다운로드하고 SHA256 검증.
    /// 진행률은 onProgress(percent, statusText) 로 통보.
    /// </summary>
    public async Task<string> DownloadAndPrepareUpdateAsync(string downloadUrl, string sha256Url, Action<int, string> onProgress)
    {
        var tempExe = Path.Combine(Path.GetTempPath(), $"{ProcessName}_new_{Guid.NewGuid():N}.exe");

        // 1. Download
        onProgress(0, "Downloading...");
        using (var response = await Http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            var totalBytes = response.Content.Headers.ContentLength ?? 0;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(tempExe, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int read;
            while ((read = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
                totalRead += read;
                if (totalBytes > 0)
                {
                    var pc = (int)((totalRead * 100) / totalBytes);
                    onProgress(pc, "Downloading...");
                }
            }
        }

        // 2. Verify SHA256
        if (!string.IsNullOrEmpty(sha256Url))
        {
            onProgress(100, "Verifying...");
            try
            {
                var expectedHashRaw = await Http.GetStringAsync(sha256Url);
                var expectedHash = expectedHashRaw.Split(' ')[0].Trim().ToLowerInvariant();

                using var fs = File.OpenRead(tempExe);
                var actualHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fs)).ToLowerInvariant();

                if (actualHash != expectedHash)
                {
                    File.Delete(tempExe);
                    throw new Exception("SHA256 mismatch");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SHA256 Error: {ex.Message}");
                // 검증 파일이 없거나 실패 시 정책에 따라 throw 로 바꿔도 됨.
            }
        }

        return tempExe;
    }

    /// <summary>
    /// PowerShell 자기-스왑 스크립트를 임시 ps1 으로 떨궈서 실행하고 호스트 앱을 종료한다.
    /// 단계: (1) 자기 종료 대기 graceful 20s → 강제 kill (2) 기존 EXE 삭제 → 새 EXE 이동 (5회 재시도) (3) 새 EXE 실행 (4) ps1 자기 정리.
    /// </summary>
    public void ApplyPreparedUpdate(string preparedExePath)
    {
        var currentExe = Process.GetCurrentProcess().MainModule?.FileName
            ?? Environment.ProcessPath
            ?? Path.Combine(AppContext.BaseDirectory, AssetExeName);

        // PowerShell single-quoted 리터럴 내부의 ' 는 '' 로 escape
        string Esc(string? s) => (s ?? "").Replace("'", "''");

        var ps1Path = Path.Combine(Path.GetTempPath(), $"{ProcessName}_swap_{Guid.NewGuid():N}.ps1");
        var logPath = Path.Combine(Path.GetTempPath(), $"{ProcessName}_update_debug.log");

        var psCommand = @"
$ErrorActionPreference = 'Stop'
$log = '{LOG_PATH}'
""Update started at $(Get-Date)"" | Out-File -LiteralPath $log

$oldExe = '{OLD_EXE}'
$newExe = '{NEW_EXE}'
$procName = '{PROC_NAME}'

function Log($msg) {
    ""$(Get-Date -Format 'HH:mm:ss') - $msg"" | Out-File -LiteralPath $log -Append
}

try {
    # 1. 정상 종료 대기
    Log ""Waiting for process to exit...""
    $timeout = 20
    while ($timeout -gt 0) {
        $p = Get-Process -Name $procName -ErrorAction SilentlyContinue
        if (-not $p) { break }
        Start-Sleep -Seconds 1
        $timeout--
    }

    # 2. 그래도 살아있으면 강제 종료
    $p = Get-Process -Name $procName -ErrorAction SilentlyContinue
    if ($p) {
        Log ""Process still running. Force killing...""
        Stop-Process -Name $procName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }

    # 3. 파일 잠금 잔류 대비 Move-Item 재시도
    Log ""Replacing executable...""
    $retry = 5
    $success = $false
    while ($retry -gt 0) {
        try {
            if (Test-Path -LiteralPath $oldExe) {
                Remove-Item -LiteralPath $oldExe -Force -ErrorAction Stop
            }
            Move-Item -LiteralPath $newExe -Destination $oldExe -Force -ErrorAction Stop
            $success = $true
            Log ""Move successful.""
            break
        } catch {
            Log ""Move failed: $($_.Exception.Message). Retrying ($retry)...""
            $retry--
            Start-Sleep -Seconds 2
        }
    }

    if (-not $success) { throw ""Failed to replace executable after retries."" }

    # 4. 재시작
    Log ""Starting new version...""
    Start-Process -FilePath $oldExe
    Log ""Update complete.""
}
catch {
    Log ""CRITICAL ERROR: $($_.Exception.Message)""
}
finally {
    Remove-Item -LiteralPath '{PS1_PATH}' -Force -ErrorAction SilentlyContinue
}
"
        .Replace("{LOG_PATH}",  Esc(logPath))
        .Replace("{OLD_EXE}",   Esc(currentExe))
        .Replace("{NEW_EXE}",   Esc(preparedExePath))
        .Replace("{PROC_NAME}", Esc(ProcessName))
        .Replace("{PS1_PATH}",  Esc(ps1Path));

        try
        {
            // 중요: PowerShell 5.1 이 한글 경로를 정확히 읽도록 UTF-8 BOM 으로 저장
            var encoding = new System.Text.UTF8Encoding(true);
            File.WriteAllText(ps1Path, psCommand, encoding);

            Process.Start(new ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{ps1Path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            File.AppendAllText(logPath, $"Failed to launch PowerShell: {ex.Message}");
        }

        // 호스트 앱 종료 — ps1 가 이어서 EXE 교체 진행
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            System.Windows.Application.Current.Shutdown());
    }
}
