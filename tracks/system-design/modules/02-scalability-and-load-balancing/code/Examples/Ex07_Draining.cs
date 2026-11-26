// Example: graceful shutdown of ASP.NET Core: requests in flight finish within a timeout.
// Module: system-design/02-scalability-and-load-balancing (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Draining"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Scaling.Examples;

public sealed class SlowServer
{
    private readonly TaskCompletionSource _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public required WebApplication App { get; init; }
    public Task RequestEntered => _entered.Task;
    public void Release() => _release.TrySetResult();

    public static async Task<SlowServer> StartAsync(TimeSpan shutdownTimeout)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = shutdownTimeout);

        var app = builder.Build();
        var server = new SlowServer { App = app };
        app.MapGet("/slow", async (CancellationToken aborted) =>
        {
            server._entered.TrySetResult();
            await server._release.Task.WaitAsync(aborted);
            return "done";
        });
        await app.StartAsync();
        return server;
    }
}
