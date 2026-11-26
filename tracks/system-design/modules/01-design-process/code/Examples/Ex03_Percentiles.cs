// Example: latency percentiles and tail amplification.
// Module: system-design/01-design-process (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Percentiles"
namespace DesignProcess.Examples;

public static class Latency
{
    // Nearest-rank method: the smallest sample such that at least p% of samples are <= it.
    public static double Percentile(IReadOnlyCollection<double> samples, double p)
    {
        if (samples.Count == 0) throw new ArgumentException("No samples.", nameof(samples));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(p, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(p, 100);

        double[] sorted = [.. samples.Order()];
        int rank = (int)Math.Ceiling(p * sorted.Length / 100.0);
        return sorted[rank - 1];
    }

    // Probability that a fan-out request hits at least one slow dependency call.
    public static double ChanceOfSlowRequest(double slowCallProbability, int fanOut) =>
        1.0 - Math.Pow(1.0 - slowCallProbability, fanOut);
}
