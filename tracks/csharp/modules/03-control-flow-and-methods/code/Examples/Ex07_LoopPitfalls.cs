// Example: modifying a collection inside foreach, and closures over loop variables.
// Module: csharp/03-control-flow-and-methods (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~LoopPitfalls"
namespace Examples;

public static class LoopPitfalls
{
    // Wrong: changing a List<T> invalidates its enumerator, the next MoveNext throws.
    public static void RemoveNegativesWrong(List<int> values)
    {
        foreach (int value in values)
        {
            if (value < 0)
            {
                values.Remove(value);
            }
        }
    }

    // Right: walk backwards by index (or call values.RemoveAll(v => v < 0)).
    public static void RemoveNegatives(List<int> values)
    {
        for (int i = values.Count - 1; i >= 0; i--)
        {
            if (values[i] < 0)
            {
                values.RemoveAt(i);
            }
        }
    }

    // foreach creates a fresh variable for every iteration (since C# 5), so each closure sees its own value.
    public static int[] CaptureInForeach()
    {
        var getters = new List<Func<int>>();
        foreach (int i in new[] { 1, 2, 3 })
        {
            getters.Add(() => i);
        }
        return getters.Select(get => get()).ToArray();
    }

    // for has a single variable for the whole loop: every closure sees its final value.
    public static int[] CaptureInFor()
    {
        var getters = new List<Func<int>>();
        for (int i = 1; i <= 3; i++)
        {
            getters.Add(() => i);
        }
        return getters.Select(get => get()).ToArray();
    }
}
