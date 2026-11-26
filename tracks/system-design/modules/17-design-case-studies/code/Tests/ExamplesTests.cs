using CaseStudies.Examples;
using Microsoft.Extensions.Time.Testing;

public class UrlShortenerTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(61, "z")]
    [InlineData(62, "10")]
    [InlineData(long.MaxValue, "AzL8n0Y58m7")]
    public void Base62_codes(long value, string code) => Assert.Equal(code, Base62.Encode(value));

    [Fact]
    public void Ids_are_unique_and_grow()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
        var generator = new IdGenerator(node: 1, clock);

        long first = generator.Next(), second = generator.Next();
        clock.Advance(TimeSpan.FromMilliseconds(1));
        long third = generator.Next();

        Assert.True(first < second && second < third);
        Assert.Equal(1, second - first); // same millisecond, next sequence number
        Assert.NotEqual(first, new IdGenerator(node: 2, clock).Next());
    }

    private sealed class SteppingClock : TimeProvider
    {
        public DateTimeOffset Now = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void A_clock_that_moves_backwards_is_refused()
    {
        var clock = new SteppingClock();
        var generator = new IdGenerator(1, clock);
        generator.Next();
        clock.Now -= TimeSpan.FromSeconds(1);

        Assert.Throws<InvalidOperationException>(() => generator.Next());
    }
}

public class NewsFeedTests
{
    [Fact]
    public void Popular_authors_are_merged_at_read_time()
    {
        var feed = new FeedService(popularThreshold: 3);
        feed.Follow("ann", "bob");
        foreach (string reader in new[] { "ann", "kim", "lee" }) feed.Follow(reader, "star");

        feed.Publish("bob", 1);
        feed.Publish("star", 2);
        feed.Publish("bob", 3);

        Assert.Equal([3L, 2L, 1L], feed.Read("ann", 10));
        Assert.Equal([2L], feed.Read("kim", 10));
        Assert.Equal(2, feed.InboxWrites); // the popular post was not copied
    }
}

public class NotificationTests
{
    [Fact]
    public void Deduplicates_limits_retries_and_dead_letters()
    {
        int calls = 0;
        var service = new NotificationService(
            n => { calls++; return n.Text != "broken" && calls > 1; }, maxPerUser: 2, maxAttempts: 3);

        Assert.Equal(SendResult.Sent, service.Send(new("1", "ann", "hi")));       // second attempt succeeds
        Assert.Equal(SendResult.Duplicate, service.Send(new("1", "ann", "hi")));
        Assert.Equal(SendResult.DeadLettered, service.Send(new("2", "ann", "broken")));
        Assert.Equal(SendResult.Sent, service.Send(new("3", "ann", "again")));
        Assert.Equal(SendResult.RateLimited, service.Send(new("4", "ann", "more")));
        Assert.Single(service.DeadLetters);
        Assert.Equal(6, calls);
    }
}

public class MoreCasesTests
{
    [Fact]
    public void Token_bucket_refills_with_time()
    {
        var clock = new FakeTimeProvider();
        var limiter = new SharedTokenBucket(capacity: 2, perSecond: 1, clock);

        Assert.True(limiter.TryTake("a"));
        Assert.True(limiter.TryTake("a"));
        Assert.False(limiter.TryTake("a"));
        Assert.True(limiter.TryTake("b"));
        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.True(limiter.TryTake("a"));
        Assert.False(limiter.TryTake("a"));
    }

    [Fact]
    public void Chat_numbers_messages_and_ignores_resends()
    {
        var chat = new Conversation();

        Assert.Equal(1, chat.Send("c1", "hi"));
        Assert.Equal(1, chat.Send("c1", "hi"));
        Assert.Equal(2, chat.Send("c2", "there"));
        Assert.Equal(["there"], chat.After(1));
    }

    [Fact]
    public void Autocomplete_returns_the_most_frequent_first()
    {
        var index = new Autocomplete(
            new Dictionary<string, int> { ["car"] = 5, ["cat"] = 9, ["cart"] = 1, ["dog"] = 3 }, perPrefix: 2);

        Assert.Equal(["cat", "car"], index.Suggest("ca"));
        Assert.Equal(["car", "cart"], index.Suggest("car"));
        Assert.Empty(index.Suggest("z"));
    }

    [Fact]
    public void Equal_chunks_are_stored_once()
    {
        var store = new ChunkStore(chunkSize: 4);
        byte[] first = "AAAABBBBCC"u8.ToArray();

        var a = store.Put(first);
        store.Put("AAAADDDD"u8.ToArray());

        Assert.Equal(4, store.StoredChunks); // AAAA, BBBB, CC, DDDD
        Assert.Equal(first, store.Get(a));

        store.Put("xAAAABBBBCC"u8.ToArray()); // one inserted byte shifts every boundary
        Assert.Equal(7, store.StoredChunks);
    }

    [Fact]
    public void Ledger_stays_balanced_and_applies_a_transfer_once()
    {
        var ledger = new Ledger();

        Assert.True(ledger.Transfer("t1", "ann", "shop", 500));
        Assert.False(ledger.Transfer("t1", "ann", "shop", 500));
        Assert.Equal((-500L, 500L, 0L), (ledger.Balance("ann"), ledger.Balance("shop"), ledger.Total));
    }
}

public class GeohashTests
{
    [Fact]
    public void Known_value() => Assert.Equal("u4pruydqqvj", Geohash.Encode(57.64911, 10.40744, 11));

    [Fact]
    public void Neighbours_across_a_cell_border_may_share_nothing()
    {
        string west = Geohash.Encode(10, -0.0001, 6), east = Geohash.Encode(10, 0.0001, 6);

        Assert.NotEqual(west[0], east[0]);
    }
}
