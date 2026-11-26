// Example: a secondary index is a second sorted structure kept in step with the data.
// Module: system-design/04-data-storage (basics)
// Tests: dotnet test --filter "FullyQualifiedName~SecondaryIndex"
namespace DataStorage.Examples;

public sealed record Order(int Id, int CustomerId, decimal Total);

public sealed class OrderTable
{
    private readonly Dictionary<int, Order> _rows = [];                    // primary key
    private readonly SortedSet<(decimal Total, int Id)> _byTotal = [];     // secondary index

    // Every write pays for every index.
    public void Upsert(Order order)
    {
        if (_rows.TryGetValue(order.Id, out Order? old)) _byTotal.Remove((old.Total, old.Id));
        _rows[order.Id] = order;
        _byTotal.Add((order.Total, order.Id));
    }

    public Order? Find(int id) => _rows.GetValueOrDefault(id);

    // The id is part of the index key, so equal totals do not collide.
    public IEnumerable<Order> TotalBetween(decimal min, decimal max) =>
        _byTotal.GetViewBetween((min, int.MinValue), (max, int.MaxValue)).Select(e => _rows[e.Id]);
}
