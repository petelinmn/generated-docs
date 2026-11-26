// Example: a YARP reverse proxy balancing two in-process backends over loopback.
// Module: system-design/02-scalability-and-load-balancing (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Yarp"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;

namespace Scaling.Examples;

public static class YarpGateway
{
    private static WebApplicationBuilder LoopbackBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); // port 0: the OS picks a free port
        return builder;
    }

    public static string AddressOf(WebApplication app) =>
        app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();

    public static async Task<WebApplication> StartBackendAsync(string name)
    {
        var app = LoopbackBuilder().Build();
        app.MapGet("/", () => name);
        await app.StartAsync();
        return app;
    }

    public static async Task<WebApplication> StartGatewayAsync(params string[] backendAddresses)
    {
        var builder = LoopbackBuilder();

        var route = new RouteConfig
        {
            RouteId = "all",
            ClusterId = "api",
            Match = new RouteMatch { Path = "{**catch-all}" },
        };
        var cluster = new ClusterConfig
        {
            ClusterId = "api",
            LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
            Destinations = backendAddresses
                .Index()
                .ToDictionary(d => $"d{d.Index}", d => new DestinationConfig { Address = d.Item }),
        };
        builder.Services.AddReverseProxy().LoadFromMemory([route], [cluster]);

        var app = builder.Build();
        app.MapReverseProxy();
        await app.StartAsync();
        return app;
    }
}
