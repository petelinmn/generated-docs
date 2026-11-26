// Example: availability arithmetic.
// Module: system-design/01-design-process (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Availability"
namespace DesignProcess.Examples;

public static class Availability
{
    // All components are required: probabilities multiply.
    public static double Serial(params double[] parts) => parts.Aggregate(1.0, (acc, a) => acc * a);

    // Any one replica is enough. Valid only when failures are independent.
    public static double Parallel(params double[] replicas) =>
        1.0 - replicas.Aggregate(1.0, (acc, a) => acc * (1.0 - a));

    // At least k of n independent replicas must be up (for example, a majority quorum).
    public static double AtLeast(int k, int n, double replica)
    {
        double total = 0, ways = 1; // ways = C(n, i)
        for (int i = 0; i <= n; i++)
        {
            if (i >= k) total += ways * Math.Pow(replica, i) * Math.Pow(1 - replica, n - i);
            ways = ways * (n - i) / (i + 1);
        }
        return total;
    }

    public static TimeSpan AllowedDowntime(double slo, TimeSpan window) =>
        TimeSpan.FromSeconds(window.TotalSeconds * (1.0 - slo));

    // Share of the error budget already spent; above 1.0 the SLO is broken.
    public static double BudgetSpent(double slo, long totalRequests, long failedRequests) =>
        failedRequests / (totalRequests * (1.0 - slo));
}
