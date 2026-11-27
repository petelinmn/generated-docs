// Example: counting clicks off the request path with a bounded channel and batching.
// Recipe: system-design/recipes/url-shortener (advanced, expert)
// Tests: dotnet test --filter "FullyQualifiedName~Clicks"
using System.Threading.Channels;

namespace Shortener.Examples;

public sealed class ClickCounter
{
    private readonly Channel<string> _clicks;
    private long _dropped;

    public ClickCounter(int capacity = 10_000) =>
        // With DropWrite a full queue discards the new item, so a redirect never waits.
        _clicks = Channel.CreateBounded<string>(
            new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.DropWrite },
            _ => Interlocked.Increment(ref _dropped));

    public long Dropped => Interlocked.Read(ref _dropped);

    public void Record(string code) => _clicks.Writer.TryWrite(code);

    // Writes everything queued so far as one update per code.
    public async Task<int> FlushAsync(LinkStore store)
    {
        var totals = new Dictionary<string, long>();
        while (_clicks.Reader.TryRead(out string? code))
            totals[code] = totals.GetValueOrDefault(code) + 1;

        foreach (var (code, count) in totals)
            await store.AddClicksAsync(code, count);
        return totals.Count;
    }
}
