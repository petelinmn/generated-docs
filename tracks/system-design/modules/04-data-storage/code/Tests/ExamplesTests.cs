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

public class WriteAheadLogTests
{
    private static MemoryStream Written()
    {
        var log = new MemoryStream();
        WriteAheadLog.Append(log, "a", "1");
        WriteAheadLog.Append(log, "b", "2");
        WriteAheadLog.Append(log, "a", "3");
        return log;
    }

    private static Dictionary<string, string> Recover(byte[] bytes) => WriteAheadLog.Recover(new MemoryStream(bytes));

    [Fact]
    public void Replay_restores_the_last_values() =>
        Assert.Equal("3", Recover(Written().ToArray())["a"]);

    [Fact]
    public void A_torn_or_damaged_tail_is_ignored()
    {
        byte[] full = Written().ToArray();
        byte[] torn = full[..^2];
        byte[] damaged = (byte[])full.Clone();
        damaged[^1] ^= 0xFF;

        Assert.Equal("1", Recover(torn)["a"]);
        Assert.Equal("1", Recover(damaged)["a"]);
        Assert.Equal("2", Recover(torn)["b"]);
    }
}

public class ColumnarTests
{
    [Fact]
    public void Columns_read_less_and_sorted_columns_compress()
    {
        Assert.Equal(1_000_000, Columnar.ValuesRead(1_000_000, 50, columnLayout: true));
        Assert.Equal(50_000_000, Columnar.ValuesRead(1_000_000, 50, columnLayout: false));

        string[] column = ["de", "de", "de", "fr", "fr", "us"];
        Assert.Equal([("de", 3), ("fr", 2), ("us", 1)], Columnar.RunLengthEncode(column));
        Assert.Equal(6, Columnar.RunLengthEncode(["de", "fr", "de", "fr", "de", "us"]).Count);
    }
}

public class SnapshotTests
{
    [Fact]
    public void Reader_does_not_see_a_commit_made_during_its_transaction()
    {
        string folder = Directory.CreateTempSubdirectory("sd04-").FullName;
        try
        {
            Assert.Equal((1L, 2L), SnapshotDemo.Run(Path.Combine(folder, "demo.db")));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}

public class BloomTests
{
    [Fact]
    public void No_false_negatives_and_few_false_positives()
    {
        var filter = new BloomFilter(bits: 10_000, hashes: 7);
        for (int i = 0; i < 1_000; i++) filter.Add($"in:{i}");

        Assert.All(Enumerable.Range(0, 1_000), i => Assert.True(filter.MightContain($"in:{i}")));
        int falsePositives = Enumerable.Range(0, 10_000).Count(i => filter.MightContain($"out:{i}"));
        Assert.InRange(falsePositives / 10_000.0, 0.001, 0.02);
    }
}

public class SecondaryLayoutTests
{
    [Fact]
    public void A_global_index_asks_fewer_partitions_for_a_selective_query()
    {
        Assert.Equal(100, SecondaryLayout.PartitionsAsked(100, 2, globalIndex: false));
        Assert.Equal(3, SecondaryLayout.PartitionsAsked(100, 2, globalIndex: true));
    }
}
