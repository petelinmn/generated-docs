---
title: "Сокращатель ссылок: продвинутая теория"
lang: ru
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

# Сокращатель ссылок: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Полная сборка — `ShortenerService`. Файл: [Ex06_Service.cs](code/Examples/Ex06_Service.cs).

## Какие адреса принимать

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

Перенаправление на `javascript:` или другую схему, а также адрес, прячущий настоящий хост в части с учётными данными, — инструменты атак. Сокращателю обычно не нужно запрашивать целевую страницу; если добавите предпросмотр или проверки, которые это делают, защитите их от SSRF.

Свои псевдонимы:

```csharp
// \z, not $: $ also matches before a trailing newline.
[GeneratedRegex(@"\A[A-Za-z0-9_-]{3,32}\z")]
private static partial Regex AliasPattern();
```

`$` совпал бы и перед завершающим переводом строки, поэтому `\z`. Зарезервируйте имена собственных маршрутов. Файл: [Ex02_Validation.cs](code/Examples/Ex02_Validation.cs).

## Хранение и коллизии

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

Первичный ключ делает захват кода атомарным: из восьми одновременных запросов на один псевдоним побеждает ровно один (это показывает тест). Случайные коды перебираются ограниченное число раз:

```csharp
for (int i = 0; i < attempts; i++)
{
    string code = nextCode();
    if (await TryAddAsync(code, url, expiresAt)) return code;
}

throw new InvalidOperationException("Could not find a free code.");
```

Файл: [Ex03_Store.cs](code/Examples/Ex03_Store.cs).

## Кэш

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

- `HybridCache` в пределах экземпляра запускает для ключа одну фабрику одновременно.
- «Не найдено» тоже кэшируется; поэтому при создании ключ удаляют, иначе код, который уже искали, продолжал бы отвечать 404.
- Срок жизни проверяется после кэша.

```csharp
public async Task DisableAsync(string code)
{
    await store.DisableAsync(code);
    await cache.RemoveAsync(Key(code));
```

Файл: [Ex04_Cache.cs](code/Examples/Ex04_Cache.cs).

## Подсчёт переходов

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

Перенаправление лишь кладёт код в ограниченную очередь. При перегрузке переходы отбрасываются и подсчитываются, а не тормозят пользователей. Фоновый сервис записывает их раз в пять секунд и при остановке; при падении очередь теряется. Файл: [Ex05_Clicks.cs](code/Examples/Ex05_Clicks.cs).

## Перенаправление и лимиты

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

Код отказа middleware по умолчанию — 503, поэтому 429 задаётся явно.

## Перед запуском в бой

- Аутентификация и проверка владельца для создания и удаления.
- HTTPS, структурированные журналы без секретов из целевых адресов.
- Способ пожаловаться на вредную ссылку и отключить её.
- Метрики: перенаправления, доля попаданий в кэш, потерянные переходы, отклонённые создания.

## Частые ошибки

- ❌ Кэшировать решение «ссылка жива» → ✅ кэшировать данные, решать при каждом запросе.
- ❌ Писать переход в базу внутри перенаправления → ✅ очередь и пачки.
- ❌ Забыть про отрицательный кэш при создании → ✅ удалять ключ.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [HybridCache](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [RFC 9110, redirection](https://www.rfc-editor.org/rfc/rfc9110#name-redirection-3xx)
- [Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Microsoft.Data.Sqlite errors](https://learn.microsoft.com/dotnet/standard/data/sqlite/database-errors)
