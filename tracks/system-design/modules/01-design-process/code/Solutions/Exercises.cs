// Solutions to the exercises of system-design/01-design-process.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace DesignProcess.Solutions;

public static class Exercises
{
    // Exercise 1: servers needed so that peak load fits at the target utilization, plus spares.
    public static int ServersNeeded(double peakQps, double qpsPerServer, double targetUtilization, int spares) =>
        (int)Math.Ceiling(peakQps / (qpsPerServer * targetUtilization)) + spares;

    // Exercise 2: smallest number of independent replicas that reaches the target.
    public static int ReplicasFor(double single, double target)
    {
        if (single is <= 0 or >= 1) throw new ArgumentOutOfRangeException(nameof(single));
        if (target is < 0 or >= 1) throw new ArgumentOutOfRangeException(nameof(target));

        int n = 1;
        // The tolerance absorbs floating-point error when the target is hit exactly.
        while (1 - Math.Pow(1 - single, n) < target - 1e-12) n++;
        return n;
    }

    // Exercise 3: burn rate = observed error ratio / allowed error ratio.
    public static double BurnRate(double slo, long total, long failed) =>
        total == 0 ? 0 : (double)failed / total / (1 - slo);
}
