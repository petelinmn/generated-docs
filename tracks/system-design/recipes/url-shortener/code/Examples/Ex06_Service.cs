// Example: the pieces together: validation, storage, cache, clicks and a limit on creation.
// Recipe: system-design/recipes/url-shortener (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Service"
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shortener.Examples;

public sealed record NewLink(string Url, string? Alias = null, int? TtlSeconds = null);

public sealed record ShortenerOptions(string ConnectionString, string PublicHost, int CreatesPerMinute = 30);

// Flushes clicks on a timer and once more on shutdown.
public sealed class ClickFlusher(ClickCounter clicks, LinkStore store, ILogger<ClickFlusher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await FlushSafelyAsync();
        }
        catch (OperationCanceledException)
        {
        }

        await FlushSafelyAsync();
    }

    // An unhandled exception in a background service stops the host; statistics are not worth that.
    private async Task FlushSafelyAsync()
    {
        try
        {
            await clicks.FlushAsync(store);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Click counts were lost.");
        }
    }
}

public static class ShortenerService
{
    public static async Task<WebApplication> BuildAsync(ShortenerOptions options, TimeProvider? clock = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddSingleton(clock ?? TimeProvider.System);
        builder.Services.AddSingleton(new LinkStore(options.ConnectionString));
        builder.Services.AddSingleton(_ => new ClickCounter());
        builder.Services.AddHybridCache();
        builder.Services.AddSingleton<LinkResolver>();
        builder.Services.AddHostedService<ClickFlusher>();
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
        });

        var app = builder.Build();
        await app.Services.GetRequiredService<LinkStore>().InitializeAsync();
        app.UseRateLimiter();

        app.MapPost("/links", async (NewLink request, LinkStore store, LinkResolver resolver, TimeProvider time) =>
        {
            if (LinkRules.CheckUrl(request.Url, options.PublicHost) is { } problem)
                return Results.BadRequest(new { error = problem });
            if (request.TtlSeconds is <= 0)
                return Results.BadRequest(new { error = "ttl must be positive" });

            DateTimeOffset? expires = request.TtlSeconds is { } ttl ? time.GetUtcNow().AddSeconds(ttl) : null;
            string code;
            if (request.Alias is { } alias)
            {
                if (!LinkRules.IsValidAlias(alias)) return Results.BadRequest(new { error = "bad alias" });
                if (!await store.TryAddAsync(alias, request.Url, expires)) return Results.Conflict();
                code = alias;
            }
            else
            {
                code = await store.CreateAsync(request.Url, expires, () => ShortCode.Random());
            }

            await resolver.ForgetAsync(code); // the code may be cached as missing
            return Results.Created($"/{code}", new { code });
        }).RequireRateLimiting("create");

        app.MapGet("/{code}", async (string code, LinkResolver resolver, ClickCounter clicks, CancellationToken ct) =>
        {
            // Anything that cannot be a code never becomes a cache key or a query.
            if (!LinkRules.IsValidAlias(code)) return Results.NotFound();
            string? url = await resolver.ResolveAsync(code, ct);
            if (url is null) return Results.NotFound();
            clicks.Record(code);
            return Results.Redirect(url); // 302: clicks stay visible and the target can be changed
        });

        // A real service must check that the caller owns the link.
        app.MapDelete("/links/{code}", async (string code, LinkResolver resolver) =>
        {
            await resolver.DisableAsync(code);
            return Results.NoContent();
        });

        return app;
    }
}
