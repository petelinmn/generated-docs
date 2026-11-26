using DataStorage.Examples;

public class LsmTests
{
    private static LsmStore Filled()
    {
        var store = new LsmStore(memtableLimit: 2);
        store.Put("a", "1");
        store.Put("b", "2"); // flush 1
        store.Put("a", "10");
        store.Delete("b");   // flush 2
        return store;
    }

    [Fact]
    public void Newest_version_wins_and_tombstone_hides_the_value()
    {
        LsmStore store = Filled();
        Assert.Equal("10", store.Get("a"));
        Assert.Null(store.Get("b"));
        Assert.Equal(4, store.StoredEntries);
    }

    [Fact]
    public void Missing_key_checks_every_structure()
    {
        Filled().Get("zzz", out int checkedCount);
        Assert.Equal(3, checkedCount);
    }

    [Fact]
    public void Compaction_removes_old_versions_and_tombstones()
    {
        LsmStore store = Filled();
        store.Compact();

        Assert.Equal(1, store.SegmentCount);
        Assert.Equal(1, store.StoredEntries);
        Assert.Equal("10", store.Get("a"));
    }
}

public class SecondaryIndexTests
{
    [Fact]
    public void Range_query_uses_the_index_and_follows_updates()
    {
        var table = new OrderTable();
        table.Upsert(new Order(1, 7, 50m));
        table.Upsert(new Order(2, 7, 50m));
        table.Upsert(new Order(3, 8, 200m));
        table.Upsert(new Order(1, 7, 300m));

        Assert.Equal([2, 3], table.TotalBetween(10m, 250m).Select(o => o.Id));
    }
}

public class QueryPlanTests
{
    private const string Query = "SELECT body FROM messages WHERE chat_id = 1 ORDER BY sent_at DESC LIMIT 20";

    [Fact]
    public void Without_an_index_the_table_is_scanned_and_sorted()
    {
        using var db = new MessagesDb();
        string plan = db.Plan(Query);

        Assert.Contains("SCAN messages", plan);
        Assert.Contains("TEMP B-TREE", plan);
    }

    [Fact]
    public void A_composite_index_serves_filter_and_order()
    {
        using var db = new MessagesDb();
        db.Execute("CREATE INDEX ix_chat ON messages (chat_id, sent_at)");
        string plan = db.Plan(Query);

        Assert.Contains("SEARCH messages USING INDEX ix_chat", plan);
        Assert.DoesNotContain("TEMP B-TREE", plan);
    }

    [Fact]
    public void A_covering_index_does_not_touch_the_table()
    {
        using var db = new MessagesDb();
        db.Execute("CREATE INDEX ix_cover ON messages (chat_id, sent_at, body)");

        Assert.Contains("COVERING INDEX ix_cover", db.Plan(Query));
    }

    [Fact]
    public void Index_column_order_matters()
    {
        using var db = new MessagesDb();
        db.Execute("CREATE INDEX ix_wrong ON messages (sent_at, chat_id)");

        Assert.DoesNotContain("SEARCH", db.Plan(Query));
    }
}
