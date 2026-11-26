// Example: how latency grows with utilization, and availability from failure and repair times.
// Module: system-design/01-design-process (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Queueing"
namespace DesignProcess.Examples;

public static class Queueing
{
    // M/M/1: random arrivals, exponential service times, one server.
    public static double MeanResponse(double serviceTime, double utilization)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(utilization);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(utilization, 1);
        return serviceTime / (1 - utilization);
    }

    // Erlang C: the probability that a request has to wait when `servers` share one queue.
    // load = arrival rate * service time, and must be smaller than the number of servers.
    public static double ProbabilityOfWaiting(int servers, double load)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(load, servers);

        // The Erlang B recursion avoids factorials, which overflow quickly.
        double blocking = 1;
        for (int n = 1; n <= servers; n++) blocking = load * blocking / (n + load * blocking);

        double utilization = load / servers;
        return blocking / (1 - utilization * (1 - blocking));
    }

    public static double MeanWait(int servers, double load, double serviceTime) =>
        ProbabilityOfWaiting(servers, load) * serviceTime / (servers - load);

    public static double Availability(TimeSpan meanTimeToFailure, TimeSpan meanTimeToRepair) =>
        meanTimeToFailure / (meanTimeToFailure + meanTimeToRepair);
}
