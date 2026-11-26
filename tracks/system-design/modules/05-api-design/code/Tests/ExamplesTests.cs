using System.Net;
using System.Net.Http.Json;
using ApiDesign.Examples;

public class IdempotencyTests
{
    [Fact]
    public async Task Executes_once_and_replays()
    {
        var store = new IdempotencyStore<int>();
        int calls = 0;
        Task<int> Charge() => Task.FromResult(++calls);

        var first = await store.RunAsync("k", "100 USD", Charge);
        var second = await store.RunAsync("k", "100 USD", Charge);
        var third = await store.RunAsync("k", "999 USD", Charge);

        Assert.Equal((IdempotencyOutcome.Executed, 1), first);
        Assert.Equal((IdempotencyOutcome.Replayed, 1), second);
        Assert.Equal(IdempotencyOutcome.KeyReused, third.Outcome);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Concurrent_duplicate_is_told_to_wait_and_a_failure_releases_the_key()
    {
        var store = new IdempotencyStore<int>();
        var gate = new TaskCompletionSource<int>();

        var first = store.RunAsync("k", "x", () => gate.Task);
        var duplicate = await store.RunAsync("k", "x", () => Task.FromResult(2));
        Assert.Equal(IdempotencyOutcome.InProgress, duplicate.Outcome);

        gate.SetException(new InvalidOperationException());
        await Assert.ThrowsAsync<InvalidOperationException>(() => first);

        var retry = await store.RunAsync("k", "x", () => Task.FromResult(3));
        Assert.Equal((IdempotencyOutcome.Executed, 3), retry);
    }
}

public class PaginationTests
{
    private static List<Post> Posts(int count) => [.. Enumerable.Range(1, count).Select(i => new Post(i, $"#{i}"))];

    [Fact]
    public void Offset_repeats_a_row_after_an_insert()
    {
        List<Post> posts = Posts(4);
        var first = Pagination.ByOffset(posts, 0, 2);
        posts.Add(new Post(5, "new"));
        var second = Pagination.ByOffset(posts, 2, 2);

        Assert.Equal([4L, 3L], first.Select(p => p.Id));
        Assert.Equal([3L, 2L], second.Select(p => p.Id));
    }

    [Fact]
    public void Keyset_continues_where_it_stopped()
    {
        List<Post> posts = Posts(4);
        var first = Pagination.AfterCursor(posts, null, 2);
        posts.Add(new Post(5, "new"));
        var second = Pagination.AfterCursor(posts, first.NextCursor, 2);

        Assert.Equal([2L, 1L], second.Items.Select(p => p.Id));
        Assert.Null(second.NextCursor);
    }
}

public class OrdersApiTests
{
    private static HttpRequestMessage Post(NewOrder order, string? key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/orders") { Content = JsonContent.Create(order) };
        if (key is not null) request.Headers.Add("Idempotency-Key", key);
        return request;
    }

    private static HttpRequestMessage Put(int id, string? ifMatch)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/orders/{id}")
        {
            Content = JsonContent.Create(new NewOrder("book", 5)),
        };
        if (ifMatch is not null) request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        return request;
    }

    [Fact]
    public async Task Repeated_post_creates_one_order()
    {
        var ct = TestContext.Current.CancellationToken;
        var (app, address) = await OrdersApi.StartAsync();
        await using var _ = app;
        using var client = new HttpClient { BaseAddress = address };
        var book = new NewOrder("book", 1);

        var first = await client.SendAsync(Post(book, "k1"), ct);
        var second = await client.SendAsync(Post(book, "k1"), ct);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(first.Headers.Location, second.Headers.Location);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(Post(book, null), ct)).StatusCode);

        var reused = await client.SendAsync(Post(new NewOrder("pen", 1), "k1"), ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reused.StatusCode);
        Assert.Equal("application/problem+json", reused.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Stale_if_match_is_rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var (app, address) = await OrdersApi.StartAsync();
        await using var _ = app;
        using var client = new HttpClient { BaseAddress = address };
        await client.SendAsync(Post(new NewOrder("book", 1), "k1"), ct);

        string tag = (await client.GetAsync("/orders/1", ct)).Headers.ETag!.Tag;

        Assert.Equal((HttpStatusCode)428, (await client.SendAsync(Put(1, null), ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(Put(1, tag), ct)).StatusCode);
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await client.SendAsync(Put(1, tag), ct)).StatusCode);
    }
}
