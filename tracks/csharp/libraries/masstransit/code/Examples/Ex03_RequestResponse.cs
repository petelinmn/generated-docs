// Example: request/response with IRequestClient<T>.
// Module: csharp/libraries/masstransit (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~RequestResponse"
using MassTransit;

namespace Shop;

public class CheckOrderConsumer : IConsumer<CheckOrder>
{
    public Task Consume(ConsumeContext<CheckOrder> context) =>
        context.RespondAsync(new OrderStatus(context.Message.OrderId, "Accepted"));
}
