// Example: the position of the acknowledgement decides what a crash does.
// Module: system-design/08-messaging-and-events (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Delivery"
namespace Messaging.Examples;

public sealed record Message(string Id, int Amount);

public enum Ack { BeforeProcessing, AfterProcessing }

public sealed class Account
{
    private readonly HashSet<string> _processed = [];

    public int Balance { get; private set; }

    public void Apply(Message message) => Balance += message.Amount;

    // In a real system the id and the effect are saved in one transaction.
    public void ApplyOnce(Message message)
    {
        if (_processed.Add(message.Id)) Balance += message.Amount;
    }
}

public static class Delivery
{
    // The consumer crashes once, on the given message, between the two steps, then restarts.
    public static void Consume(IReadOnlyList<Message> queue, Ack ack, string crashOn, Action<Message> handle)
    {
        int committed = 0; // offset of the next message after a restart
        bool crashed = false;

        while (committed < queue.Count)
        {
            Message message = queue[committed];
            bool crashNow = !crashed && message.Id == crashOn;
            crashed |= crashNow;

            if (ack == Ack.BeforeProcessing)
            {
                committed++;
                if (crashNow) continue; // acknowledged, never processed: lost
                handle(message);
            }
            else
            {
                handle(message);
                if (crashNow) continue; // processed, not acknowledged: delivered again
                committed++;
            }
        }
    }
}
