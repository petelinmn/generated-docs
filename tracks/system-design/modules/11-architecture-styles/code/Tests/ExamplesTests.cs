using Architecture.Examples;
using Architecture.Examples.Billing;
using Architecture.Examples.Contracts;
using Architecture.Examples.Orders;

public class ModularTests
{
    [Fact]
    public void Billing_reacts_to_orders_through_the_contract()
    {
        var bus = new EventBus();
        var billing = new InvoiceService(bus);

        int id = new OrderService(bus).Place(50m);

        Assert.Equal(50m, billing.Invoices[id]);
    }

    [Fact]
    public void A_failing_handler_fails_the_publisher()
    {
        var bus = new EventBus();
        bus.Subscribe<OrderPlaced>(_ => throw new InvalidOperationException());

        Assert.Throws<InvalidOperationException>(() => new OrderService(bus).Place(1m));
    }
}

public class BoundaryTests
{
    private const string Root = "Architecture.Examples";

    [Theory]
    [InlineData("Orders", "Billing")]
    [InlineData("Billing", "Orders")]
    public void Modules_do_not_reference_each_other(string module, string other) =>
        Assert.Empty(BoundaryRules.Violations(typeof(OrderService).Assembly, $"{Root}.{module}", $"{Root}.{other}"));

    [Fact]
    public void The_rule_detects_a_real_dependency() =>
        Assert.Contains("OrderService -> EventBus",
            BoundaryRules.Violations(typeof(OrderService).Assembly, $"{Root}.Orders", $"{Root}.Contracts"));
}

public class NetworkCostTests
{
    [Fact]
    public void Five_hops()
    {
        var (latency, availability) = NetworkCost.Chain(5, TimeSpan.FromMilliseconds(20), 0.999);

        Assert.Equal(TimeSpan.FromMilliseconds(100), latency);
        Assert.Equal(0.99501, availability, 5);
    }
}
