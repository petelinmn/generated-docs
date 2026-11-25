// Example: the same consumers on the RabbitMQ transport.
// Module: csharp/libraries/masstransit (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~RabbitMq" (skipped without a broker on localhost:5672)
using MassTransit;

namespace Shop;

public static class RabbitMqSetup
{
    public static void Configure(IBusRegistrationConfigurator x)
    {
        x.SetKebabCaseEndpointNameFormatter();
        x.UsingRabbitMq((ctx, c) =>
        {
            // default credentials of a local disposable broker only
            c.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });
            c.ConfigureEndpoints(ctx);
        });
    }
}
