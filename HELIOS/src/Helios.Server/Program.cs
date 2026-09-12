using Helios.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AgentTokenValidator>();
builder.Services.AddSingleton<IngestBuffer>();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapPost("/api/agents/ingest", (
        HttpRequest request,
        AgentTokenValidator validator,
        IngestBuffer buffer,
        System.Text.Json.JsonElement payload) =>
    {
        if (!validator.TryValidate(request, out var agentId))
            return Results.Unauthorized();

        buffer.Add(agentId!, payload);
        return Results.Accepted();
    })
    .WithName("AgentIngest");

app.MapGet("/api/agents/{agentId}/latest", (string agentId, IngestBuffer buffer) =>
    {
        var latest = buffer.GetLatest(agentId);
        return latest is null ? Results.NotFound() : Results.Ok(latest);
    })
    .WithName("AgentLatest");

app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTimeOffset.UtcNow }));

app.Run();
