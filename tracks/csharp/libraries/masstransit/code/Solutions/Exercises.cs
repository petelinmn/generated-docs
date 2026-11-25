// Solutions to exercises 1 and 2; exercises 3 and 4 are solved by the examples and their tests.
// Module: csharp/libraries/masstransit (exercises)
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using System.Collections.Concurrent;
using System.Globalization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Shop;

namespace Solutions;

public class Inbox
{
    public ConcurrentQueue<string> Entries { get; } = new();
}

public class Greeter(Inbox inbox) : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        inbox.Entries.Enqueue($"greeter: {context.Message.Customer} {context.Message.Amount.ToString(CultureInfo.InvariantCulture)}");
        return Task.CompletedTask;
    }
}

public class Auditor(Inbox inbox) : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        inbox.Entries.Enqueue("auditor");
        return Task.CompletedTask;
    }
}

public static class Exercises
{
    // Exercise 1: publish one message to an in-memory bus and return what the consumer saw.
    public static async Task<string> FirstMessageAsync(CancellationToken ct)
    {
        var inbox = new Inbox();
        await using var sp = Build(inbox, x => x.AddConsumer<Greeter>());
        var bus = sp.GetRequiredService<IBusControl>();
        await bus.StartAsync(ct);

        await bus.Publish(new OrderSubmitted(Guid.NewGuid(), "alice", 19.99m), ct);
        await WaitForAsync(inbox, 1, ct);

        await bus.StopAsync(ct);
        return inbox.Entries.First();
    }

    // Exercise 2: who receives a published event, and who receives a command sent to one queue.
    public static async Task<(int AfterPublish, string[] AfterSend)> PublishThenSendAsync(CancellationToken ct)
    {
        var inbox = new Inbox();
        await using var sp = Build(inbox, x =>
        {
            x.AddConsumer<Greeter>();
            x.AddConsumer<Auditor>();
        });
        var bus = sp.GetRequiredService<IBusControl>();
        await bus.StartAsync(ct);
        var order = new OrderSubmitted(Guid.NewGuid(), "alice", 19.99m);

        await bus.Publish(order, ct);
        await WaitForAsync(inbox, 2, ct);
        int afterPublish = inbox.Entries.Count;

        var endpoint = await bus.GetSendEndpoint(new Uri("queue:Auditor"));
        await endpoint.Send(order, ct);
        await WaitForAsync(inbox, 3, ct);
        await Task.Delay(200, ct); // would let a wrong second delivery show up

        await bus.StopAsync(ct);
        return (afterPublish, inbox.Entries.Skip(afterPublish).ToArray());
    }

    private static ServiceProvider Build(Inbox inbox, Action<IBusRegistrationConfigurator> consumers) =>
        new ServiceCollection()
            .AddSingleton(inbox)
            .AddMassTransit(x =>
            {
                x.DisableUsageTelemetry();
                consumers(x);
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            })
            .BuildServiceProvider();

    private static async Task WaitForAsync(Inbox inbox, int count, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        while (inbox.Entries.Count < count)
            await Task.Delay(20, timeout.Token);
    }
}
