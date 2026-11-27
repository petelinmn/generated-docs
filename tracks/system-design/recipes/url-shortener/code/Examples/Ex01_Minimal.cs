// Example: the smallest working URL shortener, links live in memory.
// Recipe: system-design/recipes/url-shortener (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Minimal"
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shortener.Examples;

public sealed record CreateLink(string Url);

public static class ShortCode
{
    public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    // Random codes cannot be walked one after another, unlike a counter.
    public static string Random(int length = 7) => RandomNumberGenerator.GetString(Alphabet, length);
}

public static class MinimalShortener
{
    public static WebApplication Build()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var app = builder.Build();
        var links = new ConcurrentDictionary<string, string>();

        app.MapPost("/links", (CreateLink request) =>
        {
            string code = ShortCode.Random();
            while (!links.TryAdd(code, request.Url))
                code = ShortCode.Random();
            return Results.Created($"/{code}", new { code });
        });

        app.MapGet("/{code}", (string code) =>
            links.TryGetValue(code, out string? url) ? Results.Redirect(url) : Results.NotFound());

        return app;
    }
}
