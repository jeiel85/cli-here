using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CliHere.App.Views;

public partial class UpdateDialog : Window
{
    private readonly Action _onSkip;
    private readonly string _errorTitle;

    public event Action? OnUpdateRequested;

    public UpdateDialog(
        string releaseNotes,
        Action onSkip,
        string title,
        string skipLabel,
        string updateLabel,
        string errorTitle)
    {
        InitializeComponent();
        _onSkip = onSkip;
        _errorTitle = errorTitle;
        TitleText.Text = title;
        SkipBtn.Content = skipLabel;
        UpdateBtn.Content = updateLabel;
        StatusText.Text = "Downloading...";
        PercentText.Text = "0%";
        RenderMarkdown(releaseNotes);
        MouseLeftButtonDown += (_, _) => DragMove();
    }

    private void RenderMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return;
        FlowDocument doc = new() { PagePadding = new Thickness(0) };
        string[] lines = markdown.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                doc.Blocks.Add(new Paragraph(new Run(line[4..]))
                {
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x93, 0xC5, 0xFD)),
                    Margin = new Thickness(0, 10, 0, 5),
                });
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                doc.Blocks.Add(new Paragraph(new Run(line[3..]))
                {
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 15, 0, 8),
                });
            }
            else if (line.StartsWith("* ", StringComparison.Ordinal) || line.StartsWith("- ", StringComparison.Ordinal))
            {
                Paragraph p = new() { Margin = new Thickness(10, 0, 0, 4) };
                p.Inlines.Add(new Run("• ") { Foreground = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)) });
                ParseInlines(p, line[2..]);
                doc.Blocks.Add(p);
            }
            else
            {
                Paragraph p = new();
                ParseInlines(p, line);
                doc.Blocks.Add(p);
            }
        }

        NotesRichText.Document = doc;
    }

    private static void ParseInlines(Paragraph paragraph, string text)
    {
        IEnumerable<string> parts = Regex.Split(text, @"(\*\*.*?\*\*|`.*?`)").Where(s => !string.IsNullOrEmpty(s));
        foreach (string part in parts)
        {
            if (part.StartsWith("**", StringComparison.Ordinal) && part.EndsWith("**", StringComparison.Ordinal))
            {
                paragraph.Inlines.Add(new Bold(new Run(part[2..^2])));
            }
            else if (part.StartsWith('`') && part.EndsWith('`'))
            {
                paragraph.Inlines.Add(new Run(part[1..^1])
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2F, 0x45)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x93, 0xC5, 0xFD)),
                    FontFamily = new FontFamily("Consolas, Lucida Console, Courier New"),
                });
            }
            else
            {
                paragraph.Inlines.Add(new Run(part));
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
            ThemedMessageBox.Show(message, _errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
        _onSkip.Invoke();
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
