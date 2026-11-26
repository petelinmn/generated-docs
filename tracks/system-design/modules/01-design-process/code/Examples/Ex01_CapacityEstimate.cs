// Example: back-of-the-envelope capacity estimation.
// Module: system-design/01-design-process (basics)
// Tests: dotnet test --filter "FullyQualifiedName~CapacityEstimate"
namespace DesignProcess.Examples;

public sealed record Workload(
    long DailyActiveUsers,
    double WritesPerUserPerDay,
    double ReadsPerWrite,
    int BytesPerWrite,
    double PeakFactor,
    int RetentionDays,
    int ReplicationFactor);

public sealed record Capacity(
    double AverageWriteQps,
    double PeakWriteQps,
    double PeakReadQps,
    long StorageBytes,
    double PeakEgressBytesPerSecond);

public static class CapacityEstimator
{
    private const int SecondsPerDay = 86_400;

    public static Capacity Estimate(Workload w)
    {
        double writesPerDay = w.DailyActiveUsers * w.WritesPerUserPerDay;
        double averageWriteQps = writesPerDay / SecondsPerDay;
        double peakWriteQps = averageWriteQps * w.PeakFactor;
        double peakReadQps = peakWriteQps * w.ReadsPerWrite;

        // Replicas multiply disk usage, not the incoming traffic.
        long storage = (long)(writesPerDay * w.BytesPerWrite * w.RetentionDays * w.ReplicationFactor);

        return new Capacity(averageWriteQps, peakWriteQps, peakReadQps, storage, peakReadQps * w.BytesPerWrite);
    }

    // Little's law: items in the system = arrival rate * time in the system.
    public static double InFlight(double requestsPerSecond, TimeSpan latency) =>
        requestsPerSecond * latency.TotalSeconds;
}
