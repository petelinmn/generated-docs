// Example: a notification sender: deduplication, a per-user limit, retries and a dead-letter list.
// Module: system-design/17-design-case-studies (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Notification"
namespace CaseStudies.Examples;

public sealed record Notification(string Id, string UserId, string Text);

public enum SendResult { Sent, Duplicate, RateLimited, DeadLettered }

// In memory and single-threaded: it shows the order of the checks.
public sealed class NotificationService(Func<Notification, bool> provider, int maxPerUser, int maxAttempts)
{
    private readonly HashSet<string> _accepted = [];
    private readonly Dictionary<string, int> _sentToUser = []; // a real limit is per time window

    public List<Notification> DeadLetters { get; } = [];

    public SendResult Send(Notification n)
    {
        if (_accepted.Contains(n.Id)) return SendResult.Duplicate;
        if (_sentToUser.GetValueOrDefault(n.UserId) >= maxPerUser) return SendResult.RateLimited;
        _accepted.Add(n.Id);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!provider(n)) continue;
            _sentToUser[n.UserId] = _sentToUser.GetValueOrDefault(n.UserId) + 1;
            return SendResult.Sent;
        }

        DeadLetters.Add(n);
        return SendResult.DeadLettered;
    }
}
