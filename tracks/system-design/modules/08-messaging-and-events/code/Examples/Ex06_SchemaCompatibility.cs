// Example: backward and forward compatibility of a record schema.
// Module: system-design/08-messaging-and-events (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Schema"
namespace Messaging.Examples;

public sealed record SchemaField(string Name, bool HasDefault);

public static class SchemaCompatibility
{
    // A reader copes with data of a writer if every field it expects is written or has a default.
    // Simplification: types and renames are ignored.
    public static bool CanRead(IReadOnlyList<SchemaField> reader, IReadOnlyList<SchemaField> writer) =>
        reader.All(r => r.HasDefault || writer.Any(w => w.Name == r.Name));

    // New consumers read old data.
    public static bool Backward(IReadOnlyList<SchemaField> oldSchema, IReadOnlyList<SchemaField> newSchema) =>
        CanRead(newSchema, oldSchema);

    // Old consumers read new data.
    public static bool Forward(IReadOnlyList<SchemaField> oldSchema, IReadOnlyList<SchemaField> newSchema) =>
        CanRead(oldSchema, newSchema);
}
