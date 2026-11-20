// Solution: exercise 3 — which shared frameworks an app asks the host for.
// Module: csharp/01-dotnet-platform (exercises)
// Tests: dotnet test --filter "FullyQualifiedName~RuntimeConfigReader"
using System.Text.Json;

namespace Solutions;

public static class RuntimeConfigReader
{
    // A console app has a single "framework"; an ASP.NET Core app may list several in "frameworks".
    public static IReadOnlyList<(string Name, string Version)> ReadFrameworks(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement options = document.RootElement.GetProperty("runtimeOptions");

        var result = new List<(string Name, string Version)>();
        if (options.TryGetProperty("framework", out JsonElement single))
        {
            result.Add(ReadOne(single));
        }
        if (options.TryGetProperty("frameworks", out JsonElement many))
        {
            foreach (JsonElement framework in many.EnumerateArray())
            {
                result.Add(ReadOne(framework));
            }
        }
        return result;
    }

    private static (string Name, string Version) ReadOne(JsonElement framework) =>
        (framework.GetProperty("name").GetString() ?? "",
         framework.GetProperty("version").GetString() ?? "");
}
