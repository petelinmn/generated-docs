// Example: Retry-After holds either a number of seconds or an HTTP date.
// Module: system-design/05-api-design (expert)
// Tests: dotnet test --filter "FullyQualifiedName~RetryAfter"
using System.Net.Http.Headers;

namespace ApiDesign.Examples;

public static class RetryAfter
{
    public static TimeSpan? Delay(string header, DateTimeOffset now)
    {
        if (!RetryConditionHeaderValue.TryParse(header, out RetryConditionHeaderValue? value)) return null;

        TimeSpan delay = value.Delta ?? value.Date!.Value - now;
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay; // the date may already be in the past
    }
}
