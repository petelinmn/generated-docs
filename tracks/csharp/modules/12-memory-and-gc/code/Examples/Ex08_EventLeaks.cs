// Example: the classic managed memory leak — an event of a long-lived publisher keeps subscribers alive.
// Module: csharp/12-memory-and-gc (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~EventLeaks"
using System.Runtime.CompilerServices;

namespace Examples;

// Lives for the whole application, like a static service or a singleton.
public sealed class PriceFeed
{
    public event EventHandler<decimal>? PriceChanged;

    public int SubscriberCount => PriceChanged?.GetInvocationList().Length ?? 0;

    public void Publish(decimal price) => PriceChanged?.Invoke(this, price);
}

// A short-lived object, e.g. a view model of a closed window.
public sealed class PriceView : IDisposable
{
    private readonly PriceFeed _feed;

    public PriceView(PriceFeed feed)
    {
        _feed = feed;
        _feed.PriceChanged += OnPriceChanged; // the feed now references this view
    }

    public decimal LastPrice { get; private set; }

    public void Dispose() => _feed.PriceChanged -= OnPriceChanged;

    private void OnPriceChanged(object? sender, decimal price) => LastPrice = price;
}

public static class EventLeaks
{
    // The view is no longer used, but the delegate in the event references it: it cannot be collected.
    public static (bool ViewAlive, int Subscribers) Forgotten()
    {
        var feed = new PriceFeed();
        WeakReference view = OpenView(feed, unsubscribe: false);
        GC.Collect();
        return (view.IsAlive, feed.SubscriberCount);
    }

    public static (bool ViewAlive, int Subscribers) Unsubscribed()
    {
        var feed = new PriceFeed();
        WeakReference view = OpenView(feed, unsubscribe: true);
        GC.Collect();
        return (view.IsAlive, feed.SubscriberCount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference OpenView(PriceFeed feed, bool unsubscribe)
    {
        var view = new PriceView(feed);
        feed.Publish(42m);
        if (unsubscribe)
        {
            view.Dispose();
        }
        return new WeakReference(view);
    }
}
