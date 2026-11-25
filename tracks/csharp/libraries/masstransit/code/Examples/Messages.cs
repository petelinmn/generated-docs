// Message contracts shared by the examples. A message type must live in a namespace.
// Module: csharp/libraries/masstransit (basics)
namespace Shop;

public record OrderSubmitted(Guid OrderId, string Customer, decimal Amount);
public record OrderPaid(Guid OrderId);
public record OrderShipped(Guid OrderId);
public record CheckOrder(Guid OrderId);
public record OrderStatus(Guid OrderId, string Status);
