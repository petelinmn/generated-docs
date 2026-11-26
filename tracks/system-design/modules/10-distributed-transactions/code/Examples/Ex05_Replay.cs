// Example: how a workflow engine resumes: it replays the code against recorded results.
// Module: system-design/10-distributed-transactions (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Replay"
namespace Transactions.Examples;

public sealed record HistoryEntry(string Step, string Result);

// The workflow code must be deterministic: the same steps in the same order on every run.
public sealed class WorkflowContext(List<HistoryEntry> history)
{
    private int _position;

    public async Task<string> CallAsync(string step, Func<Task<string>> activity)
    {
        if (_position < history.Count)
        {
            HistoryEntry recorded = history[_position++];
            if (recorded.Step != step)
                throw new InvalidOperationException($"Non-deterministic workflow: expected {recorded.Step}, got {step}.");
            return recorded.Result; // the activity is not executed again
        }

        string result = await activity();
        history.Add(new HistoryEntry(step, result));
        _position++;
        return result;
    }
}
