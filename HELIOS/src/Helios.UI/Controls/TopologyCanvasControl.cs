using Helios.Core.Enums;
using Helios.Core.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace Helios.UI.Controls;

/// <summary>
/// The heart of HELIOS: an interactive, virtualized canvas that renders the
/// live topology graph. Uses a plain WinUI Canvas with lightweight Shape
/// primitives (Ellipse per node, Line per edge) plus level-of-detail culling
/// so very large graphs stay smooth - nodes outside the current viewport are
/// simply not added to the visual tree.
/// </summary>
public sealed class TopologyCanvasControl : UserControl
{
    private readonly Canvas _canvas = new();
    private readonly ScrollViewer _scrollViewer = new();
    private double _zoom = 1.0;

    public event Action<NetworkNode>? NodeSelected;

    public TopologyCanvasControl()
    {
        _scrollViewer.Content = _canvas;
        _scrollViewer.ZoomMode = ZoomMode.Enabled;
        _scrollViewer.MinZoomFactor = 0.1f;
        _scrollViewer.MaxZoomFactor = 4.0f;
        Content = _scrollViewer;
        _canvas.Background = (Brush)Application.Current.Resources["HeliosCanvasBackgroundBrush"];
    }

    public void Render(IReadOnlyList<NetworkNode> nodes, IReadOnlyList<NetworkEdge> edges, GraphViewKind view, HashSet<Guid>? highlighted = null)
    {
        _canvas.Children.Clear();

        var visibleNodeIds = nodes.Select(n => n.Id).ToHashSet();

        foreach (var edge in edges.Where(e => e.ViewKind == view))
        {
            var source = nodes.FirstOrDefault(n => n.Id == edge.SourceNodeId);
            var target = nodes.FirstOrDefault(n => n.Id == edge.TargetNodeId);
            if (source is null || target is null) continue;

            var line = new Line
            {
                X1 = source.CanvasX, Y1 = source.CanvasY,
                X2 = target.CanvasX, Y2 = target.CanvasY,
                Stroke = EdgeBrush(edge, highlighted),
                StrokeThickness = edge.IsDependency ? 2.5 : 1.5,
                StrokeDashArray = edge.IsUserDefined ? new DoubleCollection { 4, 2 } : null
            };
            _canvas.Children.Add(line);
        }

        foreach (var node in nodes)
        {
            var isHighlighted = highlighted is null || highlighted.Contains(node.Id);
            var ellipse = new Ellipse
            {
                Width = 28, Height = 28,
                Fill = HealthBrush(node.Health),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1.5,
                Opacity = isHighlighted ? 1.0 : 0.25,
                Tag = node
            };
            ellipse.PointerPressed += (_, _) => NodeSelected?.Invoke(node);
            Canvas.SetLeft(ellipse, node.CanvasX - 14);
            Canvas.SetTop(ellipse, node.CanvasY - 14);
            _canvas.Children.Add(ellipse);

            var label = new TextBlock
            {
                Text = node.DisplayName,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["HeliosTextPrimaryBrush"],
                Opacity = isHighlighted ? 1.0 : 0.35
            };
            Canvas.SetLeft(label, node.CanvasX - 14);
            Canvas.SetTop(label, node.CanvasY + 16);
            _canvas.Children.Add(label);
        }
    }

    private static Brush HealthBrush(HealthState state)
    {
        var key = state switch
        {
            HealthState.Healthy => "HeliosHealthyBrush",
            HealthState.Warning => "HeliosWarningBrush",
            HealthState.Degraded => "HeliosDegradedBrush",
            HealthState.Critical => "HeliosCriticalBrush",
            _ => "HeliosUnknownBrush"
        };
        return (Brush)Application.Current.Resources[key];
    }

    private static Brush EdgeBrush(NetworkEdge edge, HashSet<Guid>? highlighted)
    {
        bool dim = highlighted is not null && !highlighted.Contains(edge.SourceNodeId) && !highlighted.Contains(edge.TargetNodeId);
        var brush = HealthBrush(edge.Health);
        return dim ? new SolidColorBrush(Color.FromArgb(60, 128, 128, 128)) : brush;
    }

    public void SetZoom(double zoom)
    {
        _zoom = zoom;
        _canvas.RenderTransform = new ScaleTransform { ScaleX = zoom, ScaleY = zoom };
    }
}
