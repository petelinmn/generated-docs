// Example: how an index changes the query plan (SQLite, in memory).
// Module: system-design/04-data-storage (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~QueryPlan"
using Microsoft.Data.Sqlite;

namespace DataStorage.Examples;

public sealed class MessagesDb : IDisposable
{
    // The in-memory database lives as long as this connection.
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public MessagesDb()
    {
        _connection.Open();
        Execute("CREATE TABLE messages (id INTEGER PRIMARY KEY, chat_id INTEGER, sent_at INTEGER, body TEXT)");
    }

    public void Execute(string sql)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    // The wording of the plan may change between SQLite releases: for humans, not for program logic.
    public string Plan(string query)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "EXPLAIN QUERY PLAN " + query;
        using SqliteDataReader reader = command.ExecuteReader();

        var steps = new List<string>();
        while (reader.Read()) steps.Add(reader.GetString(3));
        return string.Join("; ", steps);
    }

    public void Dispose() => _connection.Dispose();
}
