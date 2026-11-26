// Example: upcasting old event payloads on read; stored events are never changed.
// Module: system-design/13-event-sourcing-and-cqrs (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Upcasting"
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EventSourcing.Examples;

public sealed record DepositedV2(decimal Amount, string Currency);

public static class Upcaster
{
    public static DepositedV2 Read(int version, string json)
    {
        JsonObject payload = JsonNode.Parse(json)!.AsObject();

        // Each step converts one version to the next, so steps can be chained.
        if (version == 1) payload["currency"] = "USD"; // v1 was recorded when only USD existed

        return payload.Deserialize<DepositedV2>(JsonSerializerOptions.Web)!;
    }
}
