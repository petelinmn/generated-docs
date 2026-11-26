// Example: a news feed with fan-out on write for ordinary authors and on read for popular ones.
// Module: system-design/17-design-case-studies (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~NewsFeed"
namespace CaseStudies.Examples;

public sealed class FeedService(int popularThreshold)
{
    private readonly Dictionary<string, HashSet<string>> _followers = [];
    private readonly Dictionary<string, List<long>> _inbox = [];     // written at publish time
    private readonly Dictionary<string, List<long>> _popular = [];   // merged at read time

    public int InboxWrites { get; private set; }

    public void Follow(string reader, string author) => Bucket(_followers, author).Add(reader);

    public void Publish(string author, long postId)
    {
        HashSet<string> followers = Bucket(_followers, author);
        if (followers.Count >= popularThreshold)
        {
            Bucket(_popular, author).Add(postId);
            return;
        }
        foreach (string reader in followers)
        {
            Bucket(_inbox, reader).Add(postId);
            InboxWrites++;
        }
    }

    public IReadOnlyList<long> Read(string reader, int size)
    {
        var fromPopular = _popular
            .Where(p => Bucket(_followers, p.Key).Contains(reader))
            .SelectMany(p => p.Value);

        // Ids grow with time, so ordering by id is ordering by time. Distinct covers a threshold change.
        return [.. _inbox.GetValueOrDefault(reader, []).Concat(fromPopular).Distinct().OrderDescending().Take(size)];
    }

    private static TValue Bucket<TValue>(Dictionary<string, TValue> map, string key) where TValue : new()
    {
        if (!map.TryGetValue(key, out TValue? value)) map[key] = value = new TValue();
        return value;
    }
}
