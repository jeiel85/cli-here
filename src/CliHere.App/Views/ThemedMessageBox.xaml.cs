using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CliHere.App.Models;
using CliHere.App.Services;
using CliHere.App.ViewModels;

namespace CliHere.App.Views;

public partial class ThemedMessageBox : Window
{
    private MessageBoxResult _result = MessageBoxResult.None;

    private ThemedMessageBox()
    {
        InitializeComponent();
    }

    public static MessageBoxResult Show(string message)
        => Show(message, defaultTitle: null, MessageBoxButton.OK, MessageBoxImage.None, owner: null);

    public static MessageBoxResult Show(string message, string title)
        => Show(message, title, MessageBoxButton.OK, MessageBoxImage.None, owner: null);

    public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons)
        => Show(message, title, buttons, MessageBoxImage.None, owner: null);

    public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        => Show(message, title, buttons, icon, owner: null);

    public static MessageBoxResult Show(string message, string? defaultTitle, MessageBoxButton buttons, MessageBoxImage icon, Window? owner)
    {
        if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            return dispatcher.Invoke(() => Show(message, defaultTitle, buttons, icon, owner));
        }

        ThemedMessageBox dialog = new()
        {
            Owner = owner ?? Application.Current?.MainWindow ?? null,
        };

        if (dialog.Owner is null)
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        dialog.Configure(defaultTitle, message, buttons, icon);
        dialog.ShowDialog();
        return dialog._result;
    }

    private void Configure(string? title, string message, MessageBoxButton buttons, MessageBoxImage icon)
    {
        TitleText.Text = title ?? ResolveAppTitle();
        MessageText.Text = message;

        ApplyIcon(icon);
        BuildButtons(buttons);
    }

    private void ApplyIcon(MessageBoxImage icon)
    {
        switch (icon)
        {
            case MessageBoxImage.Error:
                IconHost.Background = (Brush)FindResource("DangerBrush");
                IconGlyph.Text = "!";
                break;
            case MessageBoxImage.Warning:
                IconHost.Background = (Brush)FindResource("WarningBrush");
                IconGlyph.Text = "!";
                break;
            case MessageBoxImage.Question:
                IconHost.Background = (Brush)FindResource("AccentBrush");
                IconGlyph.Text = "?";
                break;
            case MessageBoxImage.Information:
                IconHost.Background = (Brush)FindResource("AccentBrush");
                IconGlyph.Text = "i";
                break;
            default:
                IconHost.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private void BuildButtons(MessageBoxButton buttons)
    {
        (string label, MessageBoxResult result, bool primary)[] specs = buttons switch
        {
            MessageBoxButton.OKCancel =>
            [
                (Localize("Common.Cancel", "Cancel", "취소"), MessageBoxResult.Cancel, false),
                (Localize("Common.OK", "OK", "확인"), MessageBoxResult.OK, true),
            ],
            MessageBoxButton.YesNo =>
            [
                (Localize("Common.No", "No", "아니오"), MessageBoxResult.No, false),
                (Localize("Common.Yes", "Yes", "예"), MessageBoxResult.Yes, true),
            ],
            MessageBoxButton.YesNoCancel =>
            [
                (Localize("Common.Cancel", "Cancel", "취소"), MessageBoxResult.Cancel, false),
                (Localize("Common.No", "No", "아니오"), MessageBoxResult.No, false),
                (Localize("Common.Yes", "Yes", "예"), MessageBoxResult.Yes, true),
            ],
            _ =>
            [
                (Localize("Common.OK", "OK", "확인"), MessageBoxResult.OK, true),
            ],
        };

        ButtonHost.Children.Clear();
        for (int i = 0; i < specs.Length; i++)
        {
            (string label, MessageBoxResult resultValue, bool primary) = specs[i];
            Button button = new()
            {
                Content = label,
                MinWidth = 88,
                Margin = new Thickness(i == 0 ? 0 : 8, 0, 0, 0),
            };
            if (!primary)
            {
                button.Style = (Style)FindResource("SecondaryButtonStyle");
            }
            button.Click += (_, _) =>
            {
                _result = resultValue;
                Close();
            };
            ButtonHost.Children.Add(button);
        }
    }

    private static string Localize(string key, string englishFallback, string koreanFallback)
    {
        if (TryResolveLocalizationService(out LocalizationService? localization, out LanguageMode language) && localization is not null)
        {
            string value = localization.Translate(key, language);
            if (!string.Equals(value, key, StringComparison.Ordinal))
            {
                return value;
            }
        }

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase)
            ? koreanFallback
            : englishFallback;
    }

    private static string ResolveAppTitle()
    {
        if (Application.Current?.MainWindow is { Title: { Length: > 0 } existing })
        {
            return existing;
        }
        return "CLI Here";
    }

    private static bool TryResolveLocalizationService(out LocalizationService? service, out LanguageMode language)
    {
        if (Application.Current?.MainWindow?.DataContext is MainViewModel vm)
        {
            service = vm.LocalizationService;
            language = vm.Language;
            return service is not null;
        }

        service = null;
        language = LanguageMode.System;
        return false;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        _result = MessageBoxResult.Cancel;
        Close();
    }
}
