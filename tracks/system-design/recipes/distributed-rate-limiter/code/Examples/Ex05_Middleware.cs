// Example: ASP.NET Core middleware that answers 429 with Retry-After.
// Recipe: system-design/recipes/distributed-rate-limiter (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Middleware"
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RateLimiting.Examples;

public static class RateLimitMiddleware
{
    public static IApplicationBuilder UseSharedRateLimit(
        this IApplicationBuilder app, ILimiter limiter, Func<HttpContext, string> keyOf) =>
        app.Use(async (context, next) =>
        {
            var decision = await limiter.AcquireAsync(keyOf(context), 1, context.RequestAborted);
            context.Response.Headers["X-RateLimit-Remaining"] =
                decision.Remaining.ToString(CultureInfo.InvariantCulture);

            if (!decision.Allowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                // Whole seconds, rounded up: rounding down invites a request that fails again.
                context.Response.Headers.RetryAfter =
                    Math.Max(1, (int)Math.Ceiling(decision.RetryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                return;
            }

            await next(context);
        });

    public static WebApplication Build(ILimiter limiter)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var app = builder.Build();

        // In a real service the key comes from an authenticated identity, not from a header anyone can set.
        app.UseSharedRateLimit(limiter, c => c.Request.Headers["X-Api-Key"].FirstOrDefault() ?? "anonymous");
        app.MapGet("/", () => "ok");
        return app;
    }
}
