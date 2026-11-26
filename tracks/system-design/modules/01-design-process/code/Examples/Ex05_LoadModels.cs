// Example: a closed load model hides a stall (coordinated omission); an open one shows it.
// Module: system-design/01-design-process (expert)
// Tests: dotnet test --filter "FullyQualifiedName~LoadModel"
namespace DesignProcess.Examples;

// A simulation in milliseconds: one server, one request at a time, one pause.
public static class LoadModels
{
    // Closed model: the next request is sent only after the previous one has finished.
    public static List<double> Closed(int requests, double interval, double service, double stallAt, double stall)
    {
        var latencies = new List<double>(requests);
        double now = 0;
        for (int i = 0; i < requests; i++)
        {
            double sent = Math.Max(now, i * interval);
            double finished = Finish(sent, service, stallAt, stall);
            latencies.Add(finished - sent);
            now = finished;
        }
        return latencies;
    }

    // Open model: requests arrive on schedule whatever the server does.
    public static List<double> Open(int requests, double interval, double service, double stallAt, double stall)
    {
        var latencies = new List<double>(requests);
        double serverFree = 0;
        for (int i = 0; i < requests; i++)
        {
            double arrived = i * interval;
            double finished = Finish(Math.Max(arrived, serverFree), service, stallAt, stall);
            latencies.Add(finished - arrived); // includes the time spent in the queue
            serverFree = finished;
        }
        return latencies;
    }

    private static double Finish(double start, double service, double stallAt, double stall) =>
        start < stallAt + stall && start + service > stallAt
            ? stallAt + stall + service
            : start + service;
}
