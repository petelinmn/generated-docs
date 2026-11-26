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

public class MigrationTests
{
    [Fact]
    public void The_translator_hides_the_legacy_shape_and_refuses_unknown_codes()
    {
        var c = Architecture.Examples.Migration.LegacyTranslator.ToDomain(new(" 42 ", 1999, "A"));

        Assert.Equal(("42", 19.99m, true), (c.Id, c.Balance, c.IsActive));
        Assert.Throws<InvalidOperationException>(
            () => Architecture.Examples.Migration.LegacyTranslator.ToDomain(new("1", 0, "X")));
    }

    [Fact]
    public void Parallel_run_finds_differences_without_changing_answers()
    {
        var comparing = new Architecture.Examples.Migration.Comparing(
            new Architecture.Examples.Migration.OldPrices(), new Architecture.Examples.Migration.NewPrices());

        Assert.Equal(50m, comparing.Price(5));
        Assert.Equal(100m, comparing.Price(10));
        Assert.Equal([(10, 100m, 90m)], comparing.Differences);
    }
}

public class BffTests
{
    [Fact]
    public async Task An_optional_part_may_fail()
    {
        MobileHome home = await MobileBff.HomeAsync(
            () => Task.FromResult("Ann"),
            () => Task.FromException<IReadOnlyList<string>>(new HttpRequestException()));

        Assert.Equal("Ann", home.Name);
        Assert.Empty(home.Recommendations);
    }
}
