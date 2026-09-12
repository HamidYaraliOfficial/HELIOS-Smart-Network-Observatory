using Helios.Core.Enums;
using Microsoft.UI.Xaml;
using Windows.Globalization;
using Windows.Storage;

namespace Helios.UI.Services;

/// <summary>
/// Switches the app's active language between English, Persian and Chinese at
/// runtime, and sets the correct FlowDirection (RightToLeft for Persian,
/// LeftToRight for English/Chinese) on the root window content so the entire
/// shell mirrors correctly for RTL.
/// </summary>
public sealed class LocalizationService
{
    private const string SettingsKey = "Helios.SelectedLanguage";

    public AppLanguage CurrentLanguage { get; private set; } = AppLanguage.English;
    public event Action<AppLanguage>? LanguageChanged;

    public void ApplyPersistedLanguage()
    {
        var stored = ApplicationData.Current.LocalSettings.Values[SettingsKey] as string;
        var language = Enum.TryParse<AppLanguage>(stored, out var parsed) ? parsed : AppLanguage.English;
        Apply(language);
    }

    public void Apply(AppLanguage language)
    {
        CurrentLanguage = language;
        ApplicationData.Current.LocalSettings.Values[SettingsKey] = language.ToString();

        var bcp47 = language switch
        {
            AppLanguage.Persian => "fa-IR",
            AppLanguage.Chinese => "zh-CN",
            _ => "en-US"
        };

        ApplicationLanguages.PrimaryLanguageOverride = bcp47;

        if (App.MainAppWindow?.Content is FrameworkElement root)
        {
            root.FlowDirection = language == AppLanguage.Persian
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }

        LanguageChanged?.Invoke(language);
    }

    public string BuildDisplayLabel(AppLanguage language) => language switch
    {
        AppLanguage.English => "English",
        AppLanguage.Persian => "فارسی",
        AppLanguage.Chinese => "中文 (简体)",
        _ => language.ToString()
    };
}
