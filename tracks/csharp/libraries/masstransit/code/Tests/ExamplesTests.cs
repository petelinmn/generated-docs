using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shop;

namespace Tests;

public class Ex01_PublishSendTests
{
    [Fact]
    public async Task Publish_ReachesEverySubscriber_SendReachesOne()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using var sp = Harness.Create(x =>
        {
            x.AddConsumer<OrderSubmittedConsumer>();
            x.AddConsumer<AuditConsumer>();
        });
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();
        var orders = harness.GetConsumerHarness<OrderSubmittedConsumer>();
        var audit = harness.GetConsumerHarness<AuditConsumer>();

        OrderSubmitted published = Harness.NewOrder();
        await harness.Bus.Publish(published, ct);
        Assert.True(await orders.Consumed.Any<OrderSubmitted>(m => m.Context.Message.OrderId == published.OrderId, ct));
        Assert.True(await audit.Consumed.Any<OrderSubmitted>(m => m.Context.Message.OrderId == published.OrderId, ct));

        OrderSubmitted sent = Harness.NewOrder();
        ISendEndpoint endpoint = await harness.Bus.GetSendEndpoint(new Uri("queue:Audit"));
        await endpoint.Send(sent, ct);
        Assert.True(await audit.Consumed.Any<OrderSubmitted>(m => m.Context.Message.OrderId == sent.OrderId, ct));
        await harness.InactivityTask;
        Assert.False(orders.Consumed.Select<OrderSubmitted>(ct).Any(m => m.Context.Message.OrderId == sent.OrderId));
    }
}

public class Ex02_RetryAndFaultTests
{
    [Fact]
    public async Task Retry_CallsTheConsumerAgain_UntilItSucceeds()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        var log = new AttemptLog();
        await using var sp = Harness.Create(
            x =>
            {
                x.AddConsumer<FlakyConsumer>();
                x.AddConfigureEndpointsCallback((_, endpoint) => endpoint.UseMessageRetry(r => r.Immediate(3)));
            },
            s => s.AddSingleton(log));
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(Harness.NewOrder(), ct);

        Assert.True(await harness.Consumed.Any<OrderSubmitted>(ct));
        await Harness.UntilAsync(() => log.RetryAttempts.Count >= 3, ct);
        Assert.Equal([0, 1, 2], log.RetryAttempts);
        await harness.InactivityTask;
        Assert.False(harness.Published.Select<Fault<OrderSubmitted>>(ct).Any());
    }

    [Fact]
    public async Task ExhaustedRetries_PublishAFault()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        var log = new AttemptLog();
        await using var sp = Harness.Create(
            x =>
            {
                x.AddConsumer<AlwaysFailsConsumer>();
                x.AddConsumer<FaultObserver>();
                x.AddConfigureEndpointsCallback((_, endpoint) => endpoint.UseMessageRetry(r => r.Immediate(2)));
            },
            s => s.AddSingleton(log));
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(Harness.NewOrder(), ct);

        Assert.True(await harness.GetConsumerHarness<FaultObserver>().Consumed.Any<Fault<OrderSubmitted>>(ct));
        Assert.Equal([0, 1, 2], log.RetryAttempts); // the first call and two retries
    }
}

public class Ex03_RequestResponseTests
{
    [Fact]
    public async Task Request_GetsTheTypedResponse()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using var sp = Harness.Create(x => x.AddConsumer<CheckOrderConsumer>());
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();
        var client = harness.GetRequestClient<CheckOrder>();
        var id = Guid.NewGuid();

        var response = await client.GetResponse<OrderStatus>(new CheckOrder(id), ct);

        Assert.Equal(new OrderStatus(id, "Accepted"), response.Message);
    }

    [Fact]
    public async Task Request_WithoutAConsumer_TimesOut()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using var sp = Harness.Create(_ => { });
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();
        var client = harness.Bus.CreateRequestClient<CheckOrder>(RequestTimeout.After(s: 1));

        await Assert.ThrowsAsync<RequestTimeoutException>(
            () => client.GetResponse<OrderStatus>(new CheckOrder(Guid.NewGuid()), ct));
    }
}

public class Ex04_ConcurrencyTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public async Task ConcurrentMessageLimit_BoundsParallelism(int limit)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        var probe = new ConcurrencyProbe();
        await using var sp = Harness.Create(
            x =>
            {
                x.AddConsumer<SlowConsumer>();
                x.AddConfigureEndpointsCallback((_, endpoint) => endpoint.ConcurrentMessageLimit = limit);
            },
            s => s.AddSingleton(probe));
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();

        for (int i = 0; i < 8; i++)
            await harness.Bus.Publish(Harness.NewOrder(), ct);
        await Harness.UntilAsync(() => probe.Max >= limit, ct);
        await Task.Delay(200, ct); // would let an extra message slip in if the limit did not hold
        int observed = probe.Max;
        probe.Release();

        Assert.Equal(limit, observed);
    }
}

public class Ex05_OrderSagaTests
{
    [Fact]
    public async Task OrderSaga_RejectsAnEventOutOfOrder_AndCompletesInOrder()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using var sp = Harness.Create(x => x.AddSagaStateMachine<OrderStateMachine, OrderState>().InMemoryRepository());
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        var id = Guid.NewGuid();

        await harness.Bus.Publish(new OrderSubmitted(id, "alice", 5m), ct);
        Assert.NotNull(await saga.Exists(id, m => m.Submitted));

        await harness.Bus.Publish(new OrderShipped(id), ct); // not paid yet
        Assert.True(await harness.Published.Any<Fault<OrderShipped>>(ct));
        Assert.NotNull(await saga.Exists(id, m => m.Submitted));

        await harness.Bus.Publish(new OrderPaid(id), ct);
        Assert.NotNull(await saga.Exists(id, m => m.Paid));

        await harness.Bus.Publish(new OrderShipped(id), ct);
        Assert.Null(await saga.NotExists(id)); // null: the finalized instance is gone
    }
}

[Trait("Category", "integration")]
public class Ex06_RabbitMqTests
{
    private static bool BrokerIsUp()
    {
        using var tcp = new System.Net.Sockets.TcpClient();
        try
        {
            return tcp.ConnectAsync("localhost", 5672).Wait(TimeSpan.FromSeconds(1)) && tcp.Connected;
        }
        catch (AggregateException)
        {
            return false;
        }
    }

    [Fact]
    public async Task RabbitMq_DeliversAPublishedMessage()
    {
        Assert.SkipUnless(BrokerIsUp(), "No RabbitMQ on localhost:5672");
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using var sp = Harness.Create(x =>
        {
            x.AddConsumer<OrderSubmittedConsumer>();
            RabbitMqSetup.Configure(x);
        });
        var harness = sp.GetRequiredService<ITestHarness>();
        await harness.Start();
        OrderSubmitted order = Harness.NewOrder();

        await harness.Bus.Publish(order, ct);

        Assert.True(await harness.Consumed.Any<OrderSubmitted>(m => m.Context.Message.OrderId == order.OrderId, ct));
        await harness.Stop(ct);
    }
}
