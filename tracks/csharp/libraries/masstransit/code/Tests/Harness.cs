using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shop;

namespace Tests;

internal static class Harness
{
    public static ServiceProvider Create(Action<IBusRegistrationConfigurator> configure, Action<IServiceCollection>? services = null)
    {
        var collection = new ServiceCollection();
        services?.Invoke(collection);
        collection.AddMassTransitTestHarness(x =>
        {
            x.DisableUsageTelemetry(); // tests must not talk to the network
            configure(x);
        });
        return collection.BuildServiceProvider(validateScopes: true);
    }

    public static OrderSubmitted NewOrder() => new(Guid.NewGuid(), "alice", 19.99m);

    // Fails the test instead of hanging when the condition never holds.
    public static async Task UntilAsync(Func<bool> condition, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        while (!condition())
            await Task.Delay(20, timeout.Token);
    }
}
