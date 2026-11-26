// Example: SQLite in WAL mode: a reader keeps its snapshot while a writer commits.
// Module: system-design/04-data-storage (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Snapshot"
using Microsoft.Data.Sqlite;

namespace DataStorage.Examples;

public static class SnapshotDemo
{
    public static (long DuringWrite, long AfterReaderEnds) Run(string path)
    {
        string connectionString = $"Data Source={path};Pooling=False";
        using var writer = new SqliteConnection(connectionString);
        using var reader = new SqliteConnection(connectionString);
        writer.Open();
        reader.Open();
        Scalar(writer, "PRAGMA journal_mode=WAL");
        Scalar(writer, "CREATE TABLE t (x INTEGER)");
        Scalar(writer, "INSERT INTO t VALUES (1)");

        long during;
        using (SqliteTransaction read = reader.BeginTransaction(deferred: true))
        {
            Scalar(reader, "SELECT count(*) FROM t", read); // the snapshot starts with the first read
            Scalar(writer, "INSERT INTO t VALUES (2)");     // the writer is not blocked
            during = (long)Scalar(reader, "SELECT count(*) FROM t", read)!;
        }
        return (during, (long)Scalar(reader, "SELECT count(*) FROM t")!);
    }

    private static object? Scalar(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        return command.ExecuteScalar();
    }
}
