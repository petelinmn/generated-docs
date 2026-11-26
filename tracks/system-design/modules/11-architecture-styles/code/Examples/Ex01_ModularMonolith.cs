// Example: a modular monolith: modules talk through contracts, not through each other's internals.
// Module: system-design/11-architecture-styles (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Modular"
namespace Architecture.Examples.Contracts
{
    public sealed record OrderPlaced(int OrderId, decimal Total);

    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Action<object>>> _handlers = [];

        public void Subscribe<T>(Action<T> handler)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) _handlers[typeof(T)] = list = [];
            list.Add(e => handler((T)e));
        }

        // Synchronous and in process: a failing handler fails the publisher.
        public void Publish<T>(T e) where T : notnull
        {
            foreach (var handler in _handlers.GetValueOrDefault(typeof(T), [])) handler(e);
        }
    }
}

namespace Architecture.Examples.Orders
{
    using Architecture.Examples.Contracts;

    public sealed class OrderService(EventBus bus)
    {
        private int _lastId;

        public int Place(decimal total)
        {
            int id = ++_lastId;
            bus.Publish(new OrderPlaced(id, total));
            return id;
        }
    }
}

namespace Architecture.Examples.Billing
{
    using Architecture.Examples.Contracts;

    public sealed class InvoiceService
    {
        public Dictionary<int, decimal> Invoices { get; } = [];

        public InvoiceService(EventBus bus) =>
            bus.Subscribe<OrderPlaced>(e => Invoices[e.OrderId] = e.Total);
    }
}
