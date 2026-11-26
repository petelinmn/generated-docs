// Example: metrics and traces with System.Diagnostics.
// Module: system-design/14-observability-and-slo (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Instrumentation"
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Observability.Examples;

public sealed class CheckoutService : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _orders;
    private readonly Histogram<double> _duration;

    public ActivitySource Source { get; }

    // The name is a parameter only so that parallel tests do not hear each other.
    public CheckoutService(string name = "Shop.Checkout")
    {
        _meter = new Meter(name);
        _orders = _meter.CreateCounter<long>("shop.checkout.orders");
        _duration = _meter.CreateHistogram<double>("shop.checkout.duration", unit: "s");
        Source = new ActivitySource(name);
    }

    public void Place(bool succeed, double seconds)
    {
        // Null when nobody listens, hence the "?." below.
        using Activity? activity = Source.StartActivity("PlaceOrder");

        // Tags take few distinct values: never a user id or an order id.
        var outcome = new KeyValuePair<string, object?>("outcome", succeed ? "ok" : "error");
        _orders.Add(1, outcome);
        _duration.Record(seconds, outcome);

        if (!succeed) activity?.SetStatus(ActivityStatusCode.Error);
    }

    public void Dispose()
    {
        _meter.Dispose();
        Source.Dispose();
    }
}
