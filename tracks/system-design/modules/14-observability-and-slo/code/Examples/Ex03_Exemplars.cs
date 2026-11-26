// Example: an exemplar links a histogram bucket to one trace; and a synthetic probe from several places.
// Module: system-design/14-observability-and-slo (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Exemplar|FullyQualifiedName~Synthetic"
using System.Diagnostics;

namespace Observability.Examples;

// A model of the idea; with OpenTelemetry the SDK collects exemplars.
public sealed class ExemplarHistogram(params double[] upperBounds)
{
    private readonly long[] _counts = new long[upperBounds.Length + 1];
    private readonly (double Value, string TraceId)?[] _exemplars = new (double, string)?[upperBounds.Length + 1];

    public void Record(double value)
    {
        int i = 0;
        while (i < upperBounds.Length && value > upperBounds[i]) i++;
        _counts[i]++;

        // The current activity carries the trace id; without one there is nothing to link to.
        if (Activity.Current is { } activity) _exemplars[i] = (value, activity.TraceId.ToHexString());
    }

    public (double Value, string TraceId)? ExemplarOfSlowestBucket() =>
        _exemplars.Where((e, i) => e is not null && _counts[i] > 0).LastOrDefault();
}

public static class SyntheticCheck
{
    // One failing location is usually a problem of that location.
    public static bool ShouldAlert(IReadOnlyCollection<bool> succeededByLocation) =>
        succeededByLocation.Count(ok => !ok) > succeededByLocation.Count / 2;
}
