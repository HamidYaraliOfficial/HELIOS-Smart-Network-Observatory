using Helios.Core.Enums;
using Helios.Core.Models;
using Helios.Rules;
using Helios.UI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Helios.UI.Views;

/// <summary>
/// Full Settings surface: theme picker (5 themes + Windows Default), language
/// picker (English / Persian / Chinese, RTL applied live), and the fully
/// user-editable Monitoring Schedule ("open hours") with a live countdown to
/// the next transition - the user enters every day/time window themselves,
/// nothing is pre-populated.
/// </summary>
public sealed partial class SettingsWindow : Window
{
    private readonly ThemeService _themeService = App.Services.GetRequiredService<ThemeService>();
    private readonly LocalizationService _localizationService = App.Services.GetRequiredService<LocalizationService>();
    private readonly MonitoringScheduleService _scheduleService = App.Services.GetRequiredService<MonitoringScheduleService>();
    private readonly MonitoringSchedule _schedule = new() { Enabled = false };
    private DispatcherTimer? _statusTimer;

    public SettingsWindow()
    {
        InitializeComponent();
        ThemeComboBox.SelectedIndex = (int)_themeService.CurrentTheme;
        LanguageComboBox.SelectedIndex = (int)_localizationService.CurrentLanguage;
        ScheduleWindowsListView.ItemsSource = _schedule.Windows;

        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _statusTimer.Tick += (_, _) => RefreshScheduleStatus();
        _statusTimer.Start();
        RefreshScheduleStatus();
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeComboBox.SelectedItem is ComboBoxItem item && Enum.TryParse<ThemeKind>((string)item.Tag, out var theme))
            _themeService.Apply(theme);
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is ComboBoxItem item && Enum.TryParse<AppLanguage>((string)item.Tag, out var language))
            _localizationService.Apply(language);
    }

    private void ScheduleEnabledToggle_Toggled(object sender, RoutedEventArgs e)
    {
        _schedule.Enabled = ScheduleEnabledToggle.IsOn;
        RefreshScheduleStatus();
    }

    private void AddWindowButton_Click(object sender, RoutedEventArgs e)
    {
        if (DayOfWeekComboBox.SelectedIndex < 0 || OpenTimePicker.SelectedTime is null || CloseTimePicker.SelectedTime is null)
            return;

        // ComboBox items are Monday..Sunday (index 0..6); DayOfWeek enum is Sunday..Saturday.
        var dayNames = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
        var day = dayNames[DayOfWeekComboBox.SelectedIndex];

        _schedule.Windows.Add(new DailyTimeWindow
        {
            Day = day,
            IsActive = true,
            OpenTime = OpenTimePicker.SelectedTime.Value,
            CloseTime = CloseTimePicker.SelectedTime.Value
        });

        ScheduleWindowsListView.ItemsSource = null;
        ScheduleWindowsListView.ItemsSource = _schedule.Windows;
        RefreshScheduleStatus();
    }

    private void RefreshScheduleStatus()
    {
        var status = _scheduleService.Evaluate(_schedule);
        ScheduleStatusText.Text = status.StatusLabel;

        if (status.TimeUntilNextTransition is { } remaining)
        {
            var label = status.IsCurrentlyActive ? "Closes in" : "Opens in";
            ScheduleNextTransitionText.Text = remaining.TotalHours >= 1
                ? $"{label} {(int)remaining.TotalHours}h {remaining.Minutes}m"
                : $"{label} {remaining.Minutes}m";
        }
        else
        {
            ScheduleNextTransitionText.Text = string.Empty;
        }
    }
}
