// Example: HybridCache in front of the store, with negative caching and invalidation.
// Recipe: system-design/recipes/url-shortener (advanced, expert)
// Tests: dotnet test --filter "FullyQualifiedName~Cache"
using Microsoft.Extensions.Caching.Hybrid;

namespace Shortener.Examples;

public sealed class LinkResolver(LinkStore store, HybridCache cache, TimeProvider clock)
{
    private static readonly HybridCacheEntryOptions Options = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1),
    };

    // The target address, or null when there is no live link.
    public async Task<string?> ResolveAsync(string code, CancellationToken cancellationToken = default)
    {
        // A missing code is cached too (as null), so that scanning does not reach the database every time.
        Link? link = await cache.GetOrCreateAsync(
            Key(code),
            async _ => await store.FindAsync(code),
            Options,
            cancellationToken: cancellationToken);

        // Expiry is checked on every request: the cached copy may outlive the link.
        return link is not null && link.IsLive(clock.GetUtcNow()) ? link.Url : null;
    }

    public async Task DisableAsync(string code)
    {
        await store.DisableAsync(code);
        await cache.RemoveAsync(Key(code));
    }

    public ValueTask ForgetAsync(string code) => cache.RemoveAsync(Key(code));

    private static string Key(string code) => "link:" + code;
}
