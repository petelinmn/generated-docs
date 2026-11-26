// Example: retry queues with growing delays; a retried message is overtaken by later ones.
// Module: system-design/08-messaging-and-events (expert)
// Tests: dotnet test --filter "FullyQualifiedName~RetryTiers"
namespace Messaging.Examples;

public static class RetryTiers
{
    // Simulated time, one consumer. failures[body] = how many attempts fail first.
    public static (List<string> Processed, List<string> DeadLetters) Run(
        IReadOnlyList<string> messages, IReadOnlyDictionary<string, int> failures, params int[] tierDelays)
    {
        var processed = new List<string>();
        var dead = new List<string>();
        // Priority: the time a message becomes visible, then the order of scheduling.
        var queue = new PriorityQueue<(string Body, int Attempt), (int Time, int Order)>();
        int order = 0;
        for (int i = 0; i < messages.Count; i++) queue.Enqueue((messages[i], 0), (i, order++));

        while (queue.TryDequeue(out var item, out var priority))
        {
            if (item.Attempt >= failures.GetValueOrDefault(item.Body)) processed.Add(item.Body);
            else if (item.Attempt < tierDelays.Length)
                queue.Enqueue((item.Body, item.Attempt + 1), (priority.Time + tierDelays[item.Attempt], order++));
            else dead.Add(item.Body);
        }
        return (processed, dead);
    }
}
