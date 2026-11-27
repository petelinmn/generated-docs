// Example: links in a database; the primary key resolves code collisions.
// Recipe: system-design/recipes/url-shortener (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Store"
using Microsoft.Data.Sqlite;

namespace Shortener.Examples;

public sealed record Link(string Code, string Url, DateTimeOffset? ExpiresAt, bool Disabled)
{
    public bool IsLive(DateTimeOffset now) => !Disabled && (ExpiresAt is null || ExpiresAt > now);
}

public sealed class LinkStore(string connectionString)
{
    private const int ConstraintViolation = 19; // SQLITE_CONSTRAINT
    private int _reads;

    public int Reads => _reads;

    public Task InitializeAsync() => ExecuteAsync("""
        CREATE TABLE IF NOT EXISTS links (
            code TEXT PRIMARY KEY,
            url TEXT NOT NULL,
            expires_at INTEGER NULL,
            disabled INTEGER NOT NULL DEFAULT 0,
            clicks INTEGER NOT NULL DEFAULT 0)
        """);

    // false means the code is taken. Checking first and inserting later would race.
    public async Task<bool> TryAddAsync(string code, string url, DateTimeOffset? expiresAt)
    {
        try
        {
            await ExecuteAsync("INSERT INTO links (code, url, expires_at) VALUES ($code, $url, $expires)",
                ("$code", code), ("$url", url), ("$expires", expiresAt?.ToUnixTimeSeconds()));
            return true;
        }
        catch (SqliteException e) when (e.SqliteErrorCode == ConstraintViolation)
        {
            return false;
        }
    }

    public async Task<string> CreateAsync(string url, DateTimeOffset? expiresAt, Func<string> nextCode, int attempts = 5)
    {
        for (int i = 0; i < attempts; i++)
        {
            string code = nextCode();
            if (await TryAddAsync(code, url, expiresAt)) return code;
        }

        throw new InvalidOperationException("Could not find a free code.");
    }

    public async Task<Link?> FindAsync(string code)
    {
        Interlocked.Increment(ref _reads);
        await using var connection = await OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT url, expires_at, disabled FROM links WHERE code = $code";
        command.Parameters.AddWithValue("$code", code);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        DateTimeOffset? expires = reader.IsDBNull(1) ? null : DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(1));
        return new Link(code, reader.GetString(0), expires, reader.GetBoolean(2));
    }

    public Task DisableAsync(string code) =>
        ExecuteAsync("UPDATE links SET disabled = 1 WHERE code = $code", ("$code", code));

    public Task AddClicksAsync(string code, long count) =>
        ExecuteAsync("UPDATE links SET clicks = clicks + $count WHERE code = $code", ("$code", code), ("$count", count));

    public async Task<long> ClicksAsync(string code)
    {
        await using var connection = await OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT clicks FROM links WHERE code = $code";
        command.Parameters.AddWithValue("$code", code);
        return (long)(await command.ExecuteScalarAsync() ?? 0L);
    }

    private async Task<SqliteConnection> OpenAsync()
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private async Task ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = await OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }
}
