// Example: percentiles from histogram buckets are estimates, but buckets can be merged.
// Module: system-design/14-observability-and-slo (advanced, expert)
// Tests: dotnet test --filter "FullyQualifiedName~Histogram"
namespace Observability.Examples;

public sealed class Histogram(params double[] upperBounds)
{
    private readonly long[] _counts = new long[upperBounds.Length + 1]; // the last bucket is "larger than all"

    public void Record(double value)
    {
        int i = 0;
        while (i < upperBounds.Length && value > upperBounds[i]) i++;
        _counts[i]++;
    }

    public void Merge(Histogram other)
    {
        for (int i = 0; i < _counts.Length; i++) _counts[i] += other._counts[i];
    }

    // The upper bound of the bucket that holds the requested rank.
    public double QuantileUpperBound(double q)
    {
        long rank = (long)Math.Ceiling(q * _counts.Sum()), seen = 0;
        for (int i = 0; i < upperBounds.Length; i++)
        {
            seen += _counts[i];
            if (seen >= rank) return upperBounds[i];
        }
        return double.PositiveInfinity;
    }
}
