using System.Text;
using System.Text.Json;
using Helios.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Helios.Reports;

/// <summary>
/// Produces Markdown, JSON, CSV and PDF network reports. Every report states
/// its generation timestamp, scope and data source so readers understand
/// exactly what it does and doesn't cover.
/// </summary>
public sealed class ReportGenerator : IReportGenerator
{
    public Task<string> GenerateMarkdownAsync(ReportRequest request, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {request.Title}");
        sb.AppendLine();
        sb.AppendLine($"- **Generated:** {request.GeneratedUtc:u}");
        sb.AppendLine($"- **Scope:** {request.Scope}");
        sb.AppendLine($"- **Data source:** {request.DataSourceDescription}");
        if (!string.IsNullOrWhiteSpace(request.DataLimitationsNote))
            sb.AppendLine($"- **Data limitations:** {request.DataLimitationsNote}");
        sb.AppendLine();

        sb.AppendLine("## Devices").AppendLine();
        sb.AppendLine("| Name | IP | Health | Online |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var node in request.Nodes)
            sb.AppendLine($"| {node.DisplayName} | {node.IPv4Address} | {node.Health} | {node.IsOnline} |");

        sb.AppendLine().AppendLine("## Alerts").AppendLine();
        sb.AppendLine("| Title | Severity | Status | Created |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var alert in request.Alerts)
            sb.AppendLine($"| {alert.Title} | {alert.Severity} | {alert.Status} | {alert.CreatedUtc:u} |");

        sb.AppendLine().AppendLine("## Recent Events").AppendLine();
        sb.AppendLine("| Time | Category | Title |");
        sb.AppendLine("|---|---|---|");
        foreach (var evt in request.Events)
            sb.AppendLine($"| {evt.TimestampUtc:u} | {evt.Category} | {evt.Title} |");

        return Task.FromResult(sb.ToString());
    }

    public Task<string> GenerateJsonAsync(ReportRequest request, CancellationToken cancellationToken)
        => Task.FromResult(JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true }));

    public Task<string> GenerateCsvAsync(ReportRequest request, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Type,Name,Detail1,Detail2,Detail3");
        foreach (var n in request.Nodes)
            sb.AppendLine($"Node,{Csv(n.DisplayName)},{Csv(n.IPv4Address)},{n.Health},{n.IsOnline}");
        foreach (var a in request.Alerts)
            sb.AppendLine($"Alert,{Csv(a.Title)},{a.Severity},{a.Status},{a.CreatedUtc:u}");
        foreach (var e in request.Events)
            sb.AppendLine($"Event,{Csv(e.Title)},{e.Category},{e.Severity},{e.TimestampUtc:u}");
        return Task.FromResult(sb.ToString());
    }

    public Task<byte[]> GeneratePdfAsync(ReportRequest request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text(request.Title).FontSize(20).Bold();
                    col.Item().Text($"Generated {request.GeneratedUtc:u} | Scope: {request.Scope}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text("Devices").Bold().FontSize(14);
                    foreach (var node in request.Nodes)
                        col.Item().Text($"- {node.DisplayName} ({node.IPv4Address}) - {node.Health}");

                    col.Item().PaddingTop(10).Text("Alerts").Bold().FontSize(14);
                    foreach (var alert in request.Alerts)
                        col.Item().Text($"- [{alert.Severity}] {alert.Title} ({alert.Status})");
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("HELIOS Smart Network Observatory - ");
                    t.Span(request.DataSourceDescription).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    private static string Csv(string? value) => value is null ? "" : "\"" + value.Replace("\"", "\"\"") + "\"";
}
