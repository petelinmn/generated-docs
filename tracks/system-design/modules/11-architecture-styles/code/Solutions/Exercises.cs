// Solutions to the exercises of system-design/11-architecture-styles.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace Architecture.Solutions;

public static class Exercises
{
    // Exercise 1: instability = outgoing / (incoming + outgoing).
    public static double Instability(int incoming, int outgoing) =>
        incoming + outgoing == 0 ? 0 : (double)outgoing / (incoming + outgoing);

    // Exercise 2: find out whether module dependencies contain a cycle.
    public static bool HasCycle(IReadOnlyDictionary<string, string[]> dependencies)
    {
        var visiting = new HashSet<string>();
        var done = new HashSet<string>();

        bool Visit(string module)
        {
            if (done.Contains(module)) return false;
            if (!visiting.Add(module)) return true; // back edge

            foreach (string next in dependencies.GetValueOrDefault(module, []))
                if (Visit(next)) return true;

            visiting.Remove(module);
            done.Add(module);
            return false;
        }

        return dependencies.Keys.Any(Visit);
    }
}

// Exercise 3: decision records are superseded, never edited.
public sealed record Adr(int Number, string Title, int? SupersededBy = null);

public sealed class DecisionLog
{
    private readonly List<Adr> _records = [];

    public Adr Add(string title, int? supersedes = null)
    {
        var record = new Adr(_records.Count + 1, title);
        if (supersedes is int old)
        {
            if (_records[old - 1].SupersededBy is not null)
                throw new InvalidOperationException($"ADR {old} is already superseded.");
            _records[old - 1] = _records[old - 1] with { SupersededBy = record.Number };
        }
        _records.Add(record);
        return record;
    }

    public IEnumerable<Adr> Active => _records.Where(r => r.SupersededBy is null);
}
