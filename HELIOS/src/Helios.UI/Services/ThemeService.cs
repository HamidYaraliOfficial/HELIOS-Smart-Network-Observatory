using Helios.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

namespace Helios.UI.Services;

/// <summary>
/// Applies one of five themes (Windows Default, Light, Dark, AMOLED/Black,
/// Red, Blue) by swapping the merged ResourceDictionary at the Application
/// level, and persists the user's choice in local settings so it survives
/// restarts. "Windows Default" follows the OS's own light/dark/Mica setting.
/// </summary>
public sealed class ThemeService
{
    private const string SettingsKey = "Helios.SelectedTheme";

    public ThemeKind CurrentTheme { get; private set; } = ThemeKind.WindowsDefault;
    public event Action<ThemeKind>? ThemeChanged;

    public void ApplyPersistedTheme()
    {
        var stored = ApplicationData.Current.LocalSettings.Values[SettingsKey] as string;
        var theme = Enum.TryParse<ThemeKind>(stored, out var parsed) ? parsed : ThemeKind.WindowsDefault;
        Apply(theme);
    }

    public void Apply(ThemeKind theme)
    {
        CurrentTheme = theme;
        ApplicationData.Current.LocalSettings.Values[SettingsKey] = theme.ToString();

        var dictionaryPath = theme switch
        {
            ThemeKind.Light => "Themes/Light.xaml",
            ThemeKind.Dark => "Themes/Dark.xaml",
            ThemeKind.Amoled => "Themes/Amoled.xaml",
            ThemeKind.Red => "Themes/Red.xaml",
            ThemeKind.Blue => "Themes/Blue.xaml",
            _ => null // WindowsDefault: follow system, use Light dictionary as the token base + Mica backdrop.
        };

        var app = Application.Current;
        app.Resources.MergedDictionaries.Clear();
        app.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri($"ms-appx:///{dictionaryPath ?? "Themes/Light.xaml"}")
        });

        app.RequestedTheme = theme switch
        {
            ThemeKind.Dark or ThemeKind.Amoled or ThemeKind.Red or ThemeKind.Blue => ApplicationTheme.Dark,
            ThemeKind.Light => ApplicationTheme.Light,
            _ => app.RequestedTheme
        };

        ThemeChanged?.Invoke(theme);
    }
}
