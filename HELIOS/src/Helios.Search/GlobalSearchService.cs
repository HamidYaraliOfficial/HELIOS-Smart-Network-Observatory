using Microsoft.Data.Sqlite;

namespace Helios.Search;

public sealed record SearchResult(string EntityType, string EntityId, string Title, string Snippet, double Rank);

/// <summary>
/// Powers Global Search and the Ctrl+K Command Palette using SQLite's FTS5
/// full-text index for fast fuzzy/prefix matching across devices, services,
/// events, alerts, snapshots, rules and tags.
/// </summary>
public sealed class GlobalSearchService : IDisposable
{
    private readonly SqliteConnection _connection;

    public GlobalSearchService(string databasePath)
    {
        _connection = new SqliteConnection($"Data Source={databasePath}");
        _connection.Open();
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE VIRTUAL TABLE IF NOT EXISTS search_index
            USING fts5(entity_type, entity_id, title, snippet, tokenize = 'porter unicode61');
        """;
        cmd.ExecuteNonQuery();
    }

    public void IndexItem(string entityType, string entityId, string title, string snippet)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM search_index WHERE entity_id = $id AND entity_type = $type;" +
                           "INSERT INTO search_index(entity_type, entity_id, title, snippet) VALUES ($type, $id, $title, $snippet);";
        cmd.Parameters.AddWithValue("$id", entityId);
        cmd.Parameters.AddWithValue("$type", entityType);
        cmd.Parameters.AddWithValue("$title", title);
        cmd.Parameters.AddWithValue("$snippet", snippet);
        cmd.ExecuteNonQuery();
    }

    public List<SearchResult> Search(string query, int limit = 25)
    {
        var results = new List<SearchResult>();
        if (string.IsNullOrWhiteSpace(query)) return results;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            SELECT entity_type, entity_id, title, snippet, bm25(search_index) AS rank
            FROM search_index
            WHERE search_index MATCH $query
            ORDER BY rank
            LIMIT $limit;
        """;
        cmd.Parameters.AddWithValue("$query", query.Trim() + "*");
        cmd.Parameters.AddWithValue("$limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new SearchResult(
                reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetDouble(4)));
        }

        return results;
    }

    public void Dispose() => _connection.Dispose();
}
