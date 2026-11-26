// Example: bounded redelivery and a dead-letter queue for poison messages.
// Module: system-design/08-messaging-and-events (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~DeadLetter"
namespace Messaging.Examples;

public sealed class WorkQueue(int maxAttempts)
{
    private readonly Queue<(string Body, int Attempt)> _queue = new();

    public List<string> DeadLetters { get; } = [];
    public List<string> Done { get; } = [];

    public void Publish(string body) => _queue.Enqueue((body, 1));

    public void Drain(Action<string> handle)
    {
        while (_queue.TryDequeue(out var item))
        {
            try
            {
                handle(item.Body);
                Done.Add(item.Body);
            }
            catch (Exception)
            {
                // Requeue at the tail so a failing message does not block the others.
                if (item.Attempt < maxAttempts) _queue.Enqueue((item.Body, item.Attempt + 1));
                else DeadLetters.Add(item.Body);
            }
        }
    }
}
