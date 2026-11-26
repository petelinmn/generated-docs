// Example: Amdahl's law and the Universal Scalability Law.
// Module: system-design/02-scalability-and-load-balancing (expert)
// Tests: dotnet test --filter "FullyQualifiedName~ScalingLaws"
namespace Scaling.Examples;

public static class ScalingLaws
{
    public static double Amdahl(double serialFraction, int n) => n / (1 + serialFraction * (n - 1));

    // contention: waiting for a shared resource; coherency: nodes keeping each other up to date.
    public static double Universal(double contention, double coherency, int n) =>
        n / (1 + contention * (n - 1) + coherency * n * (n - 1));
}
