// Example: a lock-free stack (Treiber stack) built on Interlocked.CompareExchange.
// Module: csharp/18-concurrency (expert)
// Tests: dotnet test --filter "FullyQualifiedName~LockFreeStack"
namespace Examples;

public sealed class LockFreeStack<T>
{
    private sealed class Node(T value, Node? next)
    {
        public readonly T Value = value;
        public Node? Next = next;
    }

    private Node? _head;

    public void Push(T value)
    {
        var node = new Node(value, null);
        while (true)
        {
            Node? head = Volatile.Read(ref _head);
            node.Next = head;
            // Publish the node only if nobody changed the head since we read it; otherwise retry.
            if (Interlocked.CompareExchange(ref _head, node, head) == head)
            {
                return;
            }
        }
    }

    public bool TryPop(out T? value)
    {
        while (true)
        {
            Node? head = Volatile.Read(ref _head);
            if (head is null)
            {
                value = default;
                return false;
            }
            if (Interlocked.CompareExchange(ref _head, head.Next, head) == head)
            {
                value = head.Value;
                return true;
            }
        }
    }
}
