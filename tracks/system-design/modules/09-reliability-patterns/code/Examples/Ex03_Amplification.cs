// Example: retries at several layers multiply.
// Module: system-design/09-reliability-patterns (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Amplification"
namespace Reliability.Examples;

public static class Amplification
{
    // Calls that reach the failing dependency for one user request.
    public static int CallsAtTheBottom(int attemptsPerLayer, int layers) =>
        Enumerable.Repeat(attemptsPerLayer, layers).Aggregate(1, (total, n) => total * n);
}
