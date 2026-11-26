// Example: a transactional outbox on SQLite, a relay and an idempotent inbox.
// Module: system-design/10-distributed-transactions (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Outbox"
using Microsoft.Data.Sqlite;

namespace Transactions.Examples;

public sealed class OrderDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public OrderDatabase()
    {
        _connection.Open();
        Execute("CREATE TABLE orders (id INTEGER PRIMARY KEY)");
        Execute("CREATE TABLE outbox (id INTEGER PRIMARY KEY, payload TEXT NOT NULL, sent INTEGER NOT NULL DEFAULT 0)");
    }

    // The order and its event are committed together or not at all.
    public void PlaceOrder(int orderId, bool failBeforeCommit = false)
    {
        using SqliteTransaction tx = _connection.BeginTransaction();
        Execute("INSERT INTO orders (id) VALUES ($a)", orderId);
        Execute("INSERT INTO outbox (payload) VALUES ($a)", $"OrderPlaced:{orderId}");
        if (failBeforeCommit) throw new InvalidOperationException("Crash"); // dispose rolls back
        tx.Commit();
    }

    // Publish first, mark second: a crash between them causes a duplicate, never a loss.
    public void Relay(Action<long, string> publish, bool crashBeforeMark = false)
    {
        foreach (var (id, payload) in Pending())
        {
            publish(id, payload);
            if (crashBeforeMark) return;
            Execute("UPDATE outbox SET sent = 1 WHERE id = $a", id);
        }
    }

    public long Count(string table)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = $"SELECT count(*) FROM {table}"; // table names are constants of this example
        return (long)command.ExecuteScalar()!;
    }

    private List<(long, string)> Pending()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT id, payload FROM outbox WHERE sent = 0 ORDER BY id";
        using SqliteDataReader reader = command.ExecuteReader();
        var rows = new List<(long, string)>();
        while (reader.Read()) rows.Add((reader.GetInt64(0), reader.GetString(1)));
        return rows;
    }

    private void Execute(string sql, object? a = null)
    {
        using SqliteCommand command = _connection.CreateCommand(); // joins the active transaction
        command.CommandText = sql;
        if (a is not null) command.Parameters.AddWithValue("$a", a);
        command.ExecuteNonQuery();
    }

    public void Dispose() => _connection.Dispose();
}

public sealed class Inbox
{
    private readonly HashSet<long> _seen = [];

    public List<string> Handled { get; } = [];

    // In production the id is stored in the same transaction as the effect.
    public void Receive(long messageId, string payload)
    {
        if (_seen.Add(messageId)) Handled.Add(payload);
    }
}
