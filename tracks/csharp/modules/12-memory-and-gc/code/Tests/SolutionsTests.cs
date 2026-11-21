using System.Runtime.CompilerServices;
using Solutions;

namespace Tests;

public class S01_TempFileTests
{
    [Fact]
    public void File_IsDeletedOnDispose()
    {
        var temp = new TempFile("hello");
        Assert.Equal("hello", temp.Read());

        temp.Dispose();
        temp.Dispose();

        Assert.False(File.Exists(temp.Path));
        Assert.Throws<ObjectDisposedException>(() => temp.Read());
    }
}

public class S02_CompositeDisposableTests
{
    private sealed class Recorder(string name, List<string> log, bool fail = false) : IDisposable
    {
        public void Dispose()
        {
            log.Add(name);
            if (fail)
            {
                throw new InvalidOperationException(name);
            }
        }
    }

    [Fact]
    public void DisposesInReverseOrder_EvenWhenOneThrows()
    {
        var log = new List<string>();
        var composite = new CompositeDisposable();
        composite.Add(new Recorder("a", log));
        composite.Add(new Recorder("b", log, fail: true));
        composite.Add(new Recorder("c", log));

        var error = Assert.Throws<AggregateException>(composite.Dispose);

        Assert.Equal(["c", "b", "a"], log);
        Assert.Equal("b", Assert.Single(error.InnerExceptions).Message);
    }

    [Fact]
    public void SecondDispose_DoesNothing_AndLateItemIsDisposedImmediately()
    {
        var log = new List<string>();
        var composite = new CompositeDisposable();
        composite.Add(new Recorder("a", log));
        composite.Dispose();
        composite.Dispose();
        composite.Add(new Recorder("late", log));

        Assert.Equal(["a", "late"], log);
    }
}

public class S03_MessageHubTests
{
    private sealed class Listener
    {
        public List<string> Received { get; } = [];

        public void On(string message) => Received.Add(message);
    }

    [Fact]
    public void DisposingToken_Unsubscribes()
    {
        var hub = new MessageHub();
        var listener = new Listener();
        IDisposable token = hub.Subscribe(listener.On);
        hub.Publish("one");
        token.Dispose();
        token.Dispose();
        hub.Publish("two");

        Assert.Equal(["one"], listener.Received);
        Assert.Equal(0, hub.Count);
    }

    [Fact]
    public void UnsubscribedListener_CanBeCollected()
    {
        var hub = new MessageHub();
        WeakReference listener = SubscribeAndDispose(hub);
        GC.Collect();

        Assert.False(listener.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference SubscribeAndDispose(MessageHub hub)
    {
        var listener = new Listener();
        using (hub.Subscribe(listener.On))
        {
            hub.Publish("hello");
        }
        return new WeakReference(listener);
    }
}

public class S04_WeakEventTests
{
    private sealed class Listener
    {
        public List<int> Received { get; } = [];

        public void On(int value) => Received.Add(value);
    }

    [Fact]
    public void LiveSubscriber_ReceivesEvents()
    {
        var weakEvent = new WeakEvent<int>();
        var listener = new Listener();
        weakEvent.Subscribe(listener.On);

        weakEvent.Raise(1);
        weakEvent.Raise(2);

        Assert.Equal([1, 2], listener.Received);
        GC.KeepAlive(listener);
    }

    [Fact]
    public void ForgottenSubscriber_IsCollected_AndRemoved()
    {
        var weakEvent = new WeakEvent<int>();
        WeakReference listener = SubscribeAndForget(weakEvent);
        GC.Collect();

        Assert.False(listener.IsAlive);
        weakEvent.Raise(1);
        Assert.Equal(0, weakEvent.Count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference SubscribeAndForget(WeakEvent<int> weakEvent)
    {
        var listener = new Listener();
        weakEvent.Subscribe(listener.On);
        return new WeakReference(listener);
    }
}
