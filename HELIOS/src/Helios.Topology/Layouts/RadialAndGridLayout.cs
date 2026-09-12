using Helios.Core.Models;

namespace Helios.Topology.Layouts;

public sealed class RadialLayout
{
    public void Apply(IReadOnlyList<NetworkNode> nodes, Guid centerNodeId, double centerX, double centerY, double radiusStep = 160)
    {
        var center = nodes.FirstOrDefault(n => n.Id == centerNodeId) ?? nodes.FirstOrDefault();
        if (center is null) return;

        center.CanvasX = centerX;
        center.CanvasY = centerY;

        var others = nodes.Where(n => n.Id != center.Id).ToList();
        double angleStep = others.Count == 0 ? 0 : 2 * Math.PI / others.Count;

        for (int i = 0; i < others.Count; i++)
        {
            double angle = i * angleStep;
            others[i].CanvasX = centerX + radiusStep * Math.Cos(angle);
            others[i].CanvasY = centerY + radiusStep * Math.Sin(angle);
        }
    }
}

public sealed class GridLayout
{
    public void Apply(IReadOnlyList<NetworkNode> nodes, double cellWidth = 160, double cellHeight = 140, int columns = 6)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;
            nodes[i].CanvasX = col * cellWidth + 80;
            nodes[i].CanvasY = row * cellHeight + 80;
        }
    }
}
