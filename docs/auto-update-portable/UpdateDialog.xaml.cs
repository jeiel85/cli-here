using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace YourApp.Views;

// ============================================================================
//  Portable AutoUpdate — UpdateDialog
//  ----------------------------------------------------------------------------
//  릴리스 노트 + Skip / Update 버튼 + 통합 진행률 바.
//  마크다운(##, ###, *, -, **bold**, `code`)만 자체 렌더 (외부 라이브러리 없음).
// ============================================================================

public partial class UpdateDialog : Window
{
    private readonly Action _onSkip;
    private readonly string _errorTitle;
    public event Action? OnUpdateRequested;

    /// <summary>모든 라벨/타이틀은 호출자가 주입한다 — i18n 의존 제거.</summary>
    public UpdateDialog(
        string releaseNotes,
        Action onSkip,
        string title       = "새로운 버전이 준비되었습니다",
        string skipLabel   = "이 버전 건너뛰기",
        string updateLabel = "지금 업데이트",
        string errorTitle  = "업데이트 오류")
    {
        InitializeComponent();
        _onSkip = onSkip;
        _errorTitle = errorTitle;

        TitleText.Text    = title;
        SkipBtn.Content   = skipLabel;
        UpdateBtn.Content = updateLabel;

        RenderMarkdown(releaseNotes);

        MouseLeftButtonDown += (s, e) => DragMove();
    }

    private void RenderMarkdown(string md)
    {
        if (string.IsNullOrWhiteSpace(md)) return;

        var doc = new FlowDocument { PagePadding = new Thickness(0) };

        var lines = md.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // H3
            if (line.StartsWith("### "))
            {
                doc.Blocks.Add(new Paragraph(new Run(line.Substring(4)))
                {
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA)),
                    Margin = new Thickness(0, 10, 0, 5)
                });
            }
            // H2
            else if (line.StartsWith("## "))
            {
                doc.Blocks.Add(new Paragraph(new Run(line.Substring(3)))
                {
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 15, 0, 8)
                });
            }
            // List item
            else if (line.StartsWith("* ") || line.StartsWith("- "))
            {
                var p = new Paragraph { Margin = new Thickness(10, 0, 0, 4) };
                p.Inlines.Add(new Run("• ") { Foreground = new SolidColorBrush(Color.FromRgb(0x8B, 0x5C, 0xF6)) });
                ParseInlines(p, line.Substring(2));
                doc.Blocks.Add(p);
            }
            // Plain
            else
            {
                var p = new Paragraph();
                ParseInlines(p, line);
                doc.Blocks.Add(p);
            }
        }

        NotesRichText.Document = doc;
    }

    private void ParseInlines(Paragraph p, string text)
    {
        // **bold** 와 `code` 만 처리 (가벼운 인라인 파서)
        var parts = Regex.Split(text, @"(\*\*.*?\*\*|`.*?`)").Where(s => !string.IsNullOrEmpty(s));
        foreach (var part in parts)
        {
            if (part.StartsWith("**") && part.EndsWith("**"))
            {
                p.Inlines.Add(new Bold(new Run(part.Substring(2, part.Length - 4))));
            }
            else if (part.StartsWith("`") && part.EndsWith("`"))
            {
                p.Inlines.Add(new Run(part.Substring(1, part.Length - 2))
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2F, 0x45)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x6E, 0xE7, 0xB7)),
                    FontFamily = new FontFamily("Consolas, Lucida Console, Courier New")
                });
            }
            else
            {
                p.Inlines.Add(new Run(part));
            }
        }
    }

    public void UpdateProgress(int percent, string status)
    {
        Dispatcher.Invoke(() =>
        {
            ActionPanel.Visibility = Visibility.Collapsed;
            ProgressPanel.Visibility = Visibility.Visible;
            ProgressBar.Value = percent;
            PercentText.Text = $"{percent}%";
            StatusText.Text = status;
        });
    }

    public void ShowError(string message)
    {
        Dispatcher.Invoke(() =>
        {
            ActionPanel.Visibility = Visibility.Visible;
            ProgressPanel.Visibility = Visibility.Collapsed;
            System.Windows.MessageBox.Show(message, _errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        ActionPanel.Visibility = Visibility.Collapsed;
        ProgressPanel.Visibility = Visibility.Visible;
        OnUpdateRequested?.Invoke();
    }

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        _onSkip?.Invoke();
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
