// Example: a contract check: which changes of a message break existing clients.
// Module: system-design/05-api-design (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Compatibility"
namespace ApiDesign.Examples;

public sealed record Field(string Type, bool Required);

public enum Direction { Request, Response }

public static class Compatibility
{
    // A deliberately small model: flat objects only.
    public static IReadOnlyList<string> BreakingChanges(
        IReadOnlyDictionary<string, Field> before, IReadOnlyDictionary<string, Field> after, Direction direction)
    {
        var problems = new List<string>();
        foreach (var (name, old) in before)
        {
            if (!after.TryGetValue(name, out Field? now))
            {
                // A server that stops reading a request field does not break the sender.
                if (direction == Direction.Response) problems.Add($"removed {name}");
                continue;
            }
            if (now.Type != old.Type) problems.Add($"type of {name} changed");
            else if (direction == Direction.Request && now.Required && !old.Required) problems.Add($"{name} became required");
            else if (direction == Direction.Response && !now.Required && old.Required) problems.Add($"{name} became optional");
        }

        if (direction == Direction.Request)
            problems.AddRange(after.Where(f => f.Value.Required && !before.ContainsKey(f.Key)).Select(f => $"new required {f.Key}"));
        return problems;
    }
}
