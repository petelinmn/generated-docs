// Example: an orchestrated saga with compensations and a pivot step.
// Module: system-design/10-distributed-transactions (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Saga"
namespace Transactions.Examples;

// A step without a compensation is the pivot or comes after it.
public sealed record SagaStep(string Name, Func<Task> Execute, Func<Task>? Compensate = null);

public enum SagaOutcome { Completed, Compensated, NeedsAttention }

// In memory only. A real orchestrator persists its state after every step.
public static class Saga
{
    public static async Task<SagaOutcome> RunAsync(IReadOnlyList<SagaStep> steps, List<string> journal)
    {
        var done = new Stack<SagaStep>();
        bool pastPivot = false;

        foreach (SagaStep step in steps)
        {
            try
            {
                await step.Execute();
                journal.Add(step.Name);
                done.Push(step);
                pastPivot |= step.Compensate is null;
            }
            catch (Exception)
            {
                // After the pivot there is no way back: someone has to finish forward.
                if (pastPivot) return SagaOutcome.NeedsAttention;

                while (done.TryPop(out SagaStep? previous))
                {
                    await previous.Compensate!(); // reverse order
                    journal.Add($"undo {previous.Name}");
                }
                return SagaOutcome.Compensated;
            }
        }
        return SagaOutcome.Completed;
    }
}
