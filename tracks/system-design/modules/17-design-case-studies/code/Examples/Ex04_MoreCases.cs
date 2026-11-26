// Example: the core of five more case studies: rate limiter, chat, autocomplete, file storage, payments.
// Module: system-design/17-design-case-studies (expert)
// Tests: dotnet test --filter "FullyQualifiedName~MoreCases"
using System.Security.Cryptography;

namespace CaseStudies.Examples;

// Rate limiter: check and update must be one atomic step in the shared store (a script in Redis).
public sealed class SharedTokenBucket(int capacity, double perSecond, TimeProvider clock)
{
    private readonly Dictionary<string, (double Tokens, DateTimeOffset At)> _buckets = [];
    private readonly Lock _gate = new();

    public bool TryTake(string key)
    {
        lock (_gate)
        {
            DateTimeOffset now = clock.GetUtcNow();
            var (tokens, at) = _buckets.GetValueOrDefault(key, (capacity, now));
            tokens = Math.Min(capacity, tokens + (now - at).TotalSeconds * perSecond);

            bool allowed = tokens >= 1;
            _buckets[key] = (allowed ? tokens - 1 : tokens, now);
            return allowed;
        }
    }
}

// Chat: a number per conversation gives order; the client id makes a resend harmless. Not thread-safe.
public sealed class Conversation
{
    private readonly Dictionary<string, long> _byClientId = [];
    private readonly List<string> _messages = [];

    public long Send(string clientMessageId, string text)
    {
        if (_byClientId.TryGetValue(clientMessageId, out long number)) return number;
        _messages.Add(text);
        return _byClientId[clientMessageId] = _messages.Count;
    }

    // A client that was offline asks for everything after the last number it has.
    public IReadOnlyList<string> After(long number) => _messages[(int)number..];
}

// Autocomplete: precomputed top suggestions for every prefix: fast reads, heavier updates.
public sealed class Autocomplete
{
    private readonly Dictionary<string, List<string>> _top = [];

    public Autocomplete(IReadOnlyDictionary<string, int> frequencies, int perPrefix = 3)
    {
        foreach (var (word, _) in frequencies.OrderByDescending(f => f.Value).ThenBy(f => f.Key, StringComparer.Ordinal))
            for (int length = 1; length <= word.Length; length++)
            {
                if (!_top.TryGetValue(word[..length], out var list)) _top[word[..length]] = list = [];
                if (list.Count < perPrefix) list.Add(word);
            }
    }

    public IReadOnlyList<string> Suggest(string prefix) => _top.GetValueOrDefault(prefix, []);
}

// File storage: split into chunks addressed by content; identical chunks are stored once.
public sealed class ChunkStore(int chunkSize)
{
    private readonly Dictionary<string, byte[]> _chunks = [];

    public int StoredChunks => _chunks.Count;

    public IReadOnlyList<string> Put(byte[] file) =>
        [.. file.Chunk(chunkSize).Select(chunk =>
        {
            string id = Convert.ToHexString(SHA256.HashData(chunk));
            _chunks.TryAdd(id, chunk);
            return id;
        })];

    public byte[] Get(IEnumerable<string> manifest) => [.. manifest.SelectMany(id => _chunks[id])];
}

// Payments: an append-only double-entry ledger; every transfer sums to zero and happens once.
public sealed class Ledger
{
    private readonly List<(string Account, long Cents)> _entries = [];
    private readonly HashSet<string> _transfers = [];

    public long Balance(string account) => _entries.Where(e => e.Account == account).Sum(e => e.Cents);
    public long Total => _entries.Sum(e => e.Cents);

    public bool Transfer(string transferId, string from, string to, long cents)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cents);
        if (!_transfers.Add(transferId)) return false;

        _entries.Add((from, -cents));
        _entries.Add((to, cents));
        return true;
    }
}
