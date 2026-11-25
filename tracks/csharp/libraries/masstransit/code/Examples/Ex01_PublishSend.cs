// Example: an in-memory bus with a consumer; with two consumers Publish reaches both, Send only one.
// Module: csharp/libraries/masstransit (basics)
// Tests: dotnet test --filter "FullyQualifiedName~PublishSend"
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Shop;

public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        Console.WriteLine($"{context.Message.Customer} {context.Message.Amount} | MessageId set: {context.MessageId.HasValue}");
        return Task.CompletedTask;
    }
}

public class AuditConsumer : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        Console.WriteLine($"[AuditConsumer] saw {context.Message.OrderId}");
        return Task.CompletedTask;
    }
}

public static class InMemoryDemo
{
    public static async Task RunAsync()
    {
        // await using: some MassTransit services only support asynchronous disposal
        await using var sp = new ServiceCollection()
            .AddMassTransit(x =>
            {
                x.AddConsumer<OrderSubmittedConsumer>();
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            })
            .BuildServiceProvider();

        var bus = sp.GetRequiredService<IBusControl>();
        await bus.StartAsync();
        await bus.Publish(new OrderSubmitted(Guid.NewGuid(), "alice", 19.99m));
        await Task.Delay(500);
        await bus.StopAsync();
    }
}
