using Helios.Core.Enums;
using Helios.Core.Models;
using Helios.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;

namespace Helios.UI;

/// <summary>
/// Application shell: wires the MainViewModel's observable collections to the
/// three-pane layout (Device Explorer / Topology Canvas / Inspector) plus the
/// bottom Timeline / Flow Explorer / Alert Center tab strip, and hosts the
/// global command bar (search, Ctrl+K palette, discovery controls).
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MainViewModel>();

        DeviceListView.ItemsSource = ViewModel.Nodes;
        AlertCenterListView.ItemsSource = ViewModel.ActiveAlerts;

        TopologyCanvas.NodeSelected += OnNodeSelected;
        ViewModel.Nodes.CollectionChanged += (_, _) => RedrawCanvas();
        ViewModel.Edges.CollectionChanged += (_, _) => RedrawCanvas();

        RegisterCommandPaletteShortcut();
    }

    private void RedrawCanvas()
    {
        TopologyCanvas.Render(ViewModel.Nodes.ToList(), ViewModel.Edges.ToList(), GraphViewKind.Physical);
    }

    private void OnNodeSelected(NetworkNode node)
    {
        ViewModel.SelectNodeCommand.Execute(node);
        InspectorName.Text = node.DisplayName;
        InspectorIp.Text = node.IPv4Address ?? "-";
        InspectorMac.Text = node.MacAddress ?? "-";
        InspectorHealthText.Text = node.Health.ToString();
        InspectorHealthReason.Text = node.HealthReason ?? string.Empty;
        InspectorLatency.Text = node.LastLatencyMs is { } lat ? $"Latency: {lat:F0} ms" : "Latency: -";
        InspectorLoss.Text = node.PacketLossPercent is { } loss ? $"Packet loss: {loss:F1}%" : "Packet loss: -";
        InspectorHealthDot.Fill = (Brush)Application.Current.Resources[node.Health switch
        {
            HealthState.Healthy => "HeliosHealthyBrush",
            HealthState.Warning => "HeliosWarningBrush",
            HealthState.Degraded => "HeliosDegradedBrush",
            HealthState.Critical => "HeliosCriticalBrush",
            _ => "HeliosUnknownBrush"
        }];
    }

    private void StartDiscoveryButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CidrRangeInput = CidrInputBox.Text;
        ViewModel.StartDiscoveryCommand.Execute(null);
    }

    private void RunLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RunLayoutCommand.Execute(null);
        RedrawCanvas();
    }

    private void DeviceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DeviceListView.SelectedItem is NetworkNode node)
            OnNodeSelected(node);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new Views.SettingsWindow();
        settingsWindow.Activate();
    }

    private void RegisterCommandPaletteShortcut()
    {
        Content.KeyDown += (_, e) =>
        {
            var ctrlDown = Microsoft.UI.Input.InputKeyboardSource
                .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (ctrlDown && e.Key == Windows.System.VirtualKey.K)
            {
                var palette = new Views.CommandPaletteWindow();
                palette.Activate();
            }
        };
    }
}
