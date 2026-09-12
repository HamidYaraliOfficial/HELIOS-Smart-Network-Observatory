using Helios.Core.Models;

namespace Helios.Topology.Layouts;

/// <summary>
/// Classic Fruchterman-Reingold force-directed layout. Runs a bounded number
/// of iterations so large graphs stay responsive; the UI can call this
/// incrementally (a few iterations per animation frame) for smooth motion.
/// </summary>
public sealed class ForceDirectedLayout
{
    public void Apply(IReadOnlyList<NetworkNode> nodes, IReadOnlyList<NetworkEdge> edges,
        double width, double height, int iterations = 50)
    {
        if (nodes.Count == 0) return;

        var random = new Random(12345);
        foreach (var n in nodes)
        {
            if (n.CanvasX == 0 && n.CanvasY == 0)
            {
                n.CanvasX = random.NextDouble() * width;
                n.CanvasY = random.NextDouble() * height;
            }
        }

        double area = width * height;
        double k = Math.Sqrt(area / Math.Max(1, nodes.Count));
        var displacement = nodes.ToDictionary(n => n.Id, _ => (X: 0.0, Y: 0.0));

        for (int iter = 0; iter < iterations; iter++)
        {
            foreach (var key in displacement.Keys.ToList())
                displacement[key] = (0, 0);

            // Repulsive forces between all node pairs.
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    var a = nodes[i];
                    var b = nodes[j];
                    double dx = a.CanvasX - b.CanvasX;
                    double dy = a.CanvasY - b.CanvasY;
                    double dist = Math.Max(0.01, Math.Sqrt(dx * dx + dy * dy));
                    double force = (k * k) / dist;
                    double fx = dx / dist * force;
                    double fy = dy / dist * force;

                    var da = displacement[a.Id];
                    displacement[a.Id] = (da.X + fx, da.Y + fy);
                    var db = displacement[b.Id];
                    displacement[b.Id] = (db.X - fx, db.Y - fy);
                }
            }

            // Attractive forces along edges.
            foreach (var edge in edges)
            {
                var a = nodes.FirstOrDefault(n => n.Id == edge.SourceNodeId);
                var b = nodes.FirstOrDefault(n => n.Id == edge.TargetNodeId);
                if (a is null || b is null) continue;

                double dx = a.CanvasX - b.CanvasX;
                double dy = a.CanvasY - b.CanvasY;
                double dist = Math.Max(0.01, Math.Sqrt(dx * dx + dy * dy));
                double force = (dist * dist) / k;
                double fx = dx / dist * force;
                double fy = dy / dist * force;

                var da = displacement[a.Id];
                displacement[a.Id] = (da.X - fx, da.Y - fy);
                var db = displacement[b.Id];
                displacement[b.Id] = (db.X + fx, db.Y + fy);
            }

            double temperature = width / 10.0 * (1.0 - (double)iter / iterations);
            foreach (var node in nodes)
            {
                var d = displacement[node.Id];
                double dist = Math.Max(0.01, Math.Sqrt(d.X * d.X + d.Y * d.Y));
                node.CanvasX = Clamp(node.CanvasX + d.X / dist * Math.Min(dist, temperature), 0, width);
                node.CanvasY = Clamp(node.CanvasY + d.Y / dist * Math.Min(dist, temperature), 0, height);
            }
        }
    }

    private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
}
