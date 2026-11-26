// Example: what turning method calls into network calls costs.
// Module: system-design/11-architecture-styles (expert)
// Tests: dotnet test --filter "FullyQualifiedName~NetworkCost"
namespace Architecture.Examples;

public static class NetworkCost
{
    // Sequential calls: latencies add up and availabilities multiply.
    public static (TimeSpan Latency, double Availability) Chain(int hops, TimeSpan perHop, double hopAvailability) =>
        (perHop * hops, Math.Pow(hopAvailability, hops));
}
