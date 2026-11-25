// Example: a saga as a state machine: submitted, paid, shipped.
// Module: csharp/libraries/masstransit (expert)
// Tests: dotnet test --filter "FullyQualifiedName~OrderSaga"
using MassTransit;

namespace Shop;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
    public string? Customer { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = null!;
    public State Paid { get; private set; } = null!;
    public Event<OrderSubmitted> SubmittedEvent { get; private set; } = null!;
    public Event<OrderPaid> PaidEvent { get; private set; } = null!;
    public Event<OrderShipped> ShippedEvent { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => SubmittedEvent, e => e.CorrelateById(c => c.Message.OrderId));
        Event(() => PaidEvent, e => e.CorrelateById(c => c.Message.OrderId));
        Event(() => ShippedEvent, e => e.CorrelateById(c => c.Message.OrderId));

        Initially(When(SubmittedEvent).Then(c => c.Saga.Customer = c.Message.Customer).TransitionTo(Submitted));
        During(Submitted, When(PaidEvent).TransitionTo(Paid));
        During(Paid, When(ShippedEvent).Finalize());
        SetCompletedWhenFinalized();
    }
}
