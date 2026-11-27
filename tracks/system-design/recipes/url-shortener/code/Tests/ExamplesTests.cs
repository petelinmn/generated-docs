using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Shortener.Examples;

namespace Shortener.Tests;

internal static class Web
{
    // Redirects are not followed: the tests must not leave the loopback interface.
    public static async Task<HttpClient> ClientFor(WebApplication app)
    {
        await app.StartAsync(TestContext.Current.CancellationToken);
        return new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(app.Urls.First()) };
    }

    public static async Task<string> Code(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("code").GetString()!;
}

// A database file per test, removed afterwards.
internal sealed class TempDatabase : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"links-{Guid.NewGuid():N}.db");

    public string ConnectionString => $"Data Source={_path}";

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }
}

public class MinimalTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Minimal_RedirectsToTheStoredAddress()
    {
        await using var app = MinimalShortener.Build();
        using var client = await Web.ClientFor(app);

        var created = await client.PostAsJsonAsync("/links", new CreateLink("https://example.com/a"), Ct);
        string code = await created.Code();
        var redirect = await client.GetAsync($"/{code}", Ct);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(7, code.Length);
        Assert.Equal(HttpStatusCode.Found, redirect.StatusCode);
        Assert.Equal("https://example.com/a", redirect.Headers.Location!.ToString());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/nothing", Ct)).StatusCode);
    }
}

public class ValidationTests
{
    [Theory]
    [InlineData("https://example.com/x", null)]
    [InlineData("javascript:alert(1)", "only http and https")]
    [InlineData("example.com/x", "not an absolute URL")]
    [InlineData("https://bank.example@evil.example/", "credentials are not allowed")]
    [InlineData("https://SHORT.test/abc", "points to the shortener")]
    [InlineData("  ", "empty")]
    public void Validation_Url(string url, string? reason) => Assert.Equal(reason, LinkRules.CheckUrl(url, "short.test"));

    [Theory]
    [InlineData("my-link", true)]
    [InlineData("ab", false)]
    [InlineData("Admin", false)]
    [InlineData("a/b/c", false)]
    [InlineData("abc\n", false)]
    public void Validation_Alias(string alias, bool valid) => Assert.Equal(valid, LinkRules.IsValidAlias(alias));
}

public class StoreTests
{
    [Fact]
    public async Task Store_TakenCodeIsRejectedAndAnotherIsTried()
    {
        using var db = new TempDatabase();
        var store = new LinkStore(db.ConnectionString);
        await store.InitializeAsync();
        var codes = new Queue<string>(["aaa", "aaa", "bbb"]);

        Assert.Equal("aaa", await store.CreateAsync("https://one.example", null, codes.Dequeue));
        Assert.Equal("bbb", await store.CreateAsync("https://two.example", null, codes.Dequeue));
        Assert.Equal("https://one.example", (await store.FindAsync("aaa"))!.Url);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.CreateAsync("https://x.example", null, () => "aaa"));
    }

    [Fact]
    public async Task Store_ConcurrentClaimsOfOneAliasHaveOneWinner()
    {
        using var db = new TempDatabase();
        var store = new LinkStore(db.ConnectionString);
        await store.InitializeAsync();

        var results = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(i => Task.Run(() => store.TryAddAsync("promo", $"https://example.com/{i}", null))));

        Assert.Equal(1, results.Count(won => won));
    }
}

public class CacheTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // One "instance" of the service: its own process-local cache over the shared database.
    private static (LinkResolver Resolver, ServiceProvider Services) Instance(LinkStore store, TimeProvider clock)
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        var provider = services.BuildServiceProvider();
        return (new LinkResolver(store, provider.GetRequiredService<HybridCache>(), clock), provider);
    }

    private static async Task<LinkStore> Store(TempDatabase db)
    {
        var store = new LinkStore(db.ConnectionString);
        await store.InitializeAsync();
        return store;
    }

    [Fact]
    public async Task Cache_HitsAndMissesBothAvoidTheDatabase()
    {
        using var db = new TempDatabase();
        var store = await Store(db);
        var (resolver, services) = Instance(store, TimeProvider.System);
        await using var _ = services;
        await store.TryAddAsync("abc", "https://example.com", null);

        for (int i = 0; i < 3; i++) Assert.Equal("https://example.com", await resolver.ResolveAsync("abc", Ct));
        for (int i = 0; i < 3; i++) Assert.Null(await resolver.ResolveAsync("zzz", Ct));

        Assert.Equal(2, store.Reads);
    }

    [Fact]
    public async Task Cache_ExpiryIsCheckedOnACachedEntry()
    {
        using var db = new TempDatabase();
        var store = await Store(db);
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var (resolver, services) = Instance(store, clock);
        await using var _ = services;
        await store.TryAddAsync("abc", "https://example.com", clock.GetUtcNow().AddSeconds(10));

        Assert.NotNull(await resolver.ResolveAsync("abc", Ct));
        clock.Advance(TimeSpan.FromSeconds(11));

        Assert.Null(await resolver.ResolveAsync("abc", Ct));
        Assert.Equal(1, store.Reads);
    }

    [Fact]
    public async Task Cache_AnotherInstanceKeepsItsLocalCopyAfterDisable()
    {
        using var db = new TempDatabase();
        var store = await Store(db);
        var (first, servicesA) = Instance(store, TimeProvider.System);
        var (second, servicesB) = Instance(store, TimeProvider.System);
        await using var a = servicesA;
        await using var b = servicesB;
        await store.TryAddAsync("abc", "https://example.com", null);
        await first.ResolveAsync("abc", Ct);
        await second.ResolveAsync("abc", Ct);

        await first.DisableAsync("abc");

        Assert.Null(await first.ResolveAsync("abc", Ct));
        Assert.Equal("https://example.com", await second.ResolveAsync("abc", Ct));
    }
}

public class ClicksTests
{
    [Fact]
    public async Task Clicks_AreBatchedAndOverflowIsCounted()
    {
        using var db = new TempDatabase();
        var store = new LinkStore(db.ConnectionString);
        await store.InitializeAsync();
        await store.TryAddAsync("abc", "https://example.com", null);
        var clicks = new ClickCounter(capacity: 3);

        for (int i = 0; i < 5; i++) clicks.Record("abc");

        Assert.Equal(2, clicks.Dropped);
        Assert.Equal(1, await clicks.FlushAsync(store));
        Assert.Equal(3, await store.ClicksAsync("abc"));
    }
}

public class ServiceTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Service_AliasLifecycle()
    {
        using var db = new TempDatabase();
        await using var app = await ShortenerService.BuildAsync(new(db.ConnectionString, "short.test"));
        using var client = await Web.ClientFor(app);

        // Looked up before it exists: creation has to clear the cached miss.
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/promo", Ct)).StatusCode);
        var body = new NewLink("https://example.com/sale", "promo");
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/links", body, Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/links", body, Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Found, (await client.GetAsync("/promo", Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/links/promo", Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/promo", Ct)).StatusCode);

        await app.StopAsync(Ct); // the flusher writes what is still queued
        Assert.Equal(1, await new LinkStore(db.ConnectionString).ClicksAsync("promo"));
    }

    [Fact]
    public async Task Service_RejectsBadInputAndLimitsCreation()
    {
        using var db = new TempDatabase();
        await using var app = await ShortenerService.BuildAsync(new(db.ConnectionString, "short.test", CreatesPerMinute: 2));
        using var client = await Web.ClientFor(app);

        var bad = await client.PostAsJsonAsync("/links", new NewLink("ftp://example.com"), Ct);
        var good = await client.PostAsJsonAsync("/links", new NewLink("https://example.com"), Ct);
        var limited = await client.PostAsJsonAsync("/links", new NewLink("https://example.com"), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        Assert.Equal(HttpStatusCode.Created, good.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    [Fact]
    public void Service_DefaultRejectionStatusIs503() =>
        Assert.Equal(503, new RateLimiterOptions().RejectionStatusCode);
}
