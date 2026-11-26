// Example: a Minimal API with Idempotency-Key, ETag, If-Match and problem details.
// Module: system-design/05-api-design (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~OrdersApi"
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiDesign.Examples;

public sealed record NewOrder(string Item, int Quantity);
public sealed record OrderDto(int Id, string Item, int Quantity, int Version);

public static class OrdersApi
{
    private static string ETag(OrderDto order) => $"\"{order.Version}\"";

    public static async Task<(WebApplication App, Uri Address)> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); // port 0: the OS picks a free port
        var app = builder.Build();

        var orders = new ConcurrentDictionary<int, OrderDto>();
        var idempotency = new IdempotencyStore<OrderDto>();
        int lastId = 0;

        app.MapPost("/orders", async (NewOrder body, HttpContext http) =>
        {
            string? key = http.Request.Headers["Idempotency-Key"];
            if (string.IsNullOrEmpty(key))
                return Results.Problem("Idempotency-Key header is required.", statusCode: 400);

            // A record's ToString covers every property, which is enough as a fingerprint here.
            var (outcome, order) = await idempotency.RunAsync(key, body.ToString(), () =>
            {
                int id = Interlocked.Increment(ref lastId);
                var created = new OrderDto(id, body.Item, body.Quantity, Version: 1);
                orders[id] = created;
                return Task.FromResult(created);
            });

            return outcome switch
            {
                IdempotencyOutcome.KeyReused => Results.Problem("The key was used with another payload.", statusCode: 422),
                IdempotencyOutcome.InProgress => Results.Problem("The original request is still running.", statusCode: 409),
                _ => Results.Created($"/orders/{order!.Id}", order),
            };
        });

        app.MapGet("/orders/{id:int}", (int id, HttpContext http) =>
        {
            if (!orders.TryGetValue(id, out OrderDto? order)) return Results.NotFound();
            http.Response.Headers.ETag = ETag(order);
            return Results.Ok(order);
        });

        app.MapPut("/orders/{id:int}", (int id, NewOrder body, HttpContext http) =>
        {
            if (!orders.TryGetValue(id, out OrderDto? current)) return Results.NotFound();

            string? ifMatch = http.Request.Headers.IfMatch;
            if (string.IsNullOrEmpty(ifMatch)) return Results.Problem("If-Match is required.", statusCode: 428);

            var updated = new OrderDto(id, body.Item, body.Quantity, current.Version + 1);

            // TryUpdate makes check-and-write one atomic step.
            if (ifMatch != ETag(current) || !orders.TryUpdate(id, updated, current))
                return Results.Problem("The order has changed.", statusCode: 412);

            http.Response.Headers.ETag = ETag(updated);
            return Results.Ok(updated);
        });

        await app.StartAsync();
        string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        return (app, new Uri(address));
    }
}
