---
title: "URL shortener: advanced"
lang: en
track: system-design
kind: recipe
doc: advanced
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# URL shortener: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

The full assembly is `ShortenerService`. File: [Ex06_Service.cs](code/Examples/Ex06_Service.cs).

## Which addresses to accept

```csharp
public static string? CheckUrl(string? url, string ownHost)
{
    if (string.IsNullOrWhiteSpace(url)) return "empty";
    if (url.Length > MaxUrlLength) return "too long";
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return "not an absolute URL";
    if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return "only http and https";
    // https://bank.example@evil.example shows one host and opens another.
    if (uri.UserInfo.Length > 0) return "credentials are not allowed";
    // A short link that points to a short link can build a loop.
    if (string.Equals(uri.Host, ownHost, StringComparison.OrdinalIgnoreCase)) return "points to the shortener";
    return null;
```

A redirect to `javascript:` or another scheme, or an address that hides the real host in the credentials part, is a tool for attacks. A shortener usually does not need to fetch the target; if you add previews or checks that do, protect against SSRF.

Custom aliases:

```csharp
// \z, not $: $ also matches before a trailing newline.
[GeneratedRegex(@"\A[A-Za-z0-9_-]{3,32}\z")]
private static partial Regex AliasPattern();
```

`$` would also match before a trailing newline, hence `\z`. Reserve the names of your own routes. File: [Ex02_Validation.cs](code/Examples/Ex02_Validation.cs).

## Storage and collisions

```csharp
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
```

The primary key makes the claim atomic: of eight concurrent requests for one alias exactly one wins (a test shows it). Random codes are retried a limited number of times:

```csharp
for (int i = 0; i < attempts; i++)
{
    string code = nextCode();
    if (await TryAddAsync(code, url, expiresAt)) return code;
}

throw new InvalidOperationException("Could not find a free code.");
```

File: [Ex03_Store.cs](code/Examples/Ex03_Store.cs).

## Cache

```csharp
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
```

- `HybridCache` runs one factory per key at a time within an instance.
- "Not found" is cached too; therefore creation removes the key, otherwise a code looked up earlier would keep answering 404.
- Expiry is checked after the cache.

```csharp
public async Task DisableAsync(string code)
{
    await store.DisableAsync(code);
    await cache.RemoveAsync(Key(code));
```

File: [Ex04_Cache.cs](code/Examples/Ex04_Cache.cs).

## Counting clicks

```csharp
// With DropWrite a full queue discards the new item, so a redirect never waits.
_clicks = Channel.CreateBounded<string>(
    new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.DropWrite },
    _ => Interlocked.Increment(ref _dropped));
```

```csharp
// Writes everything queued so far as one update per code.
public async Task<int> FlushAsync(LinkStore store)
{
    var totals = new Dictionary<string, long>();
    while (_clicks.Reader.TryRead(out string? code))
        totals[code] = totals.GetValueOrDefault(code) + 1;

    foreach (var (code, count) in totals)
        await store.AddClicksAsync(code, count);
    return totals.Count;
```

The redirect only puts the code into a bounded queue. Under overload clicks are dropped and counted rather than slowing users down. A background service flushes every five seconds and on shutdown; a crash loses what was queued. File: [Ex05_Clicks.cs](code/Examples/Ex05_Clicks.cs).

## Redirect and limits

```csharp
app.MapGet("/{code}", async (string code, LinkResolver resolver, ClickCounter clicks, CancellationToken ct) =>
{
    string? url = await resolver.ResolveAsync(code, ct);
    if (url is null) return Results.NotFound();
    clicks.Record(code);
    return Results.Redirect(url); // 302: clicks stay visible and the target can be changed
```

```csharp
builder.Services.AddRateLimiter(limiter =>
{
    limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // the default is 503
    limiter.AddPolicy("create", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = options.CreatesPerMinute,
            Window = TimeSpan.FromMinutes(1),
        }));
```

The default rejection status of the middleware is 503, so set 429 explicitly.

## Before going live

- Authentication and ownership checks for creation and deletion.
- HTTPS, structured logs without secrets from target URLs.
- A way to report and disable abusive links.
- Metrics: redirects, cache hit ratio, dropped clicks, rejected creations.

## Common mistakes

- ❌ Caching the decision "alive" → ✅ cache the data, decide per request.
- ❌ Writing a click to the database inside the redirect → ✅ a queue and batches.
- ❌ Forgetting the negative cache on creation → ✅ remove the key.

## Related materials

- [Basics](basics.en.md)
- [Expert](expert.en.md)

## Sources

- [HybridCache](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [RFC 9110, redirection](https://www.rfc-editor.org/rfc/rfc9110#name-redirection-3xx)
- [Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Microsoft.Data.Sqlite errors](https://learn.microsoft.com/dotnet/standard/data/sqlite/database-errors)
