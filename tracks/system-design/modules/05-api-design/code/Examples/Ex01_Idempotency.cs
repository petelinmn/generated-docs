// Example: idempotency keys: execute once, replay the stored result.
// Module: system-design/05-api-design (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Idempotency"
using System.Collections.Concurrent;

namespace ApiDesign.Examples;

public enum IdempotencyOutcome { Executed, Replayed, InProgress, KeyReused }

// Per process and without expiry. In production the record lives in the database,
// is written in the same transaction as the effect and is removed after a retention period.
public sealed class IdempotencyStore<TResult>
{
    private sealed record Entry(string Fingerprint, bool Done, TResult? Result);

    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    public async Task<(IdempotencyOutcome Outcome, TResult? Result)> RunAsync(
        string key, string fingerprint, Func<Task<TResult>> action)
    {
        var pending = new Entry(fingerprint, Done: false, default);

        // TryAdd is the atomic claim: only one caller gets to run the action.
        while (!_entries.TryAdd(key, pending))
        {
            if (!_entries.TryGetValue(key, out Entry? existing)) continue;
            if (existing.Fingerprint != fingerprint) return (IdempotencyOutcome.KeyReused, default);
            return existing.Done
                ? (IdempotencyOutcome.Replayed, existing.Result)
                : (IdempotencyOutcome.InProgress, default);
        }

        try
        {
            TResult result = await action();
            _entries[key] = pending with { Done = true, Result = result };
            return (IdempotencyOutcome.Executed, result);
        }
        catch
        {
            _entries.TryRemove(key, out _); // let the client retry a failed attempt
            throw;
        }
    }
}
