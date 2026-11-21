// Example: using statements and declarations — when Dispose runs and in which order.
// Module: csharp/12-memory-and-gc (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Using"
namespace Examples;

// Writes to a shared log, so the order of Dispose calls becomes visible.
public sealed class Logged(string name, List<string> log) : IDisposable
{
    public void Dispose() => log.Add($"dispose {name}");
}

public static class UsingExamples
{
    // using statement: Dispose runs when the block ends.
    public static List<string> Statement()
    {
        var log = new List<string>();
        using (var file = new Logged("file", log))
        {
            log.Add("body");
        }
        log.Add("after");
        return log;
    }

    // using declarations: disposed at the end of the enclosing block, in reverse order.
    public static List<string> Declarations()
    {
        var log = new List<string>();
        {
            using var a = new Logged("a", log);
            using var b = new Logged("b", log);
            log.Add("body end");
        }
        log.Add("after block");
        return log;
    }

    // using is try/finally: Dispose runs even when the body throws.
    public static List<string> WithException()
    {
        var log = new List<string>();
        try
        {
            using var resource = new Logged("resource", log);
            throw new InvalidOperationException("boom");
        }
        catch (InvalidOperationException)
        {
            log.Add("caught");
        }
        return log;
    }

    // using with null is allowed: the compiler checks for null before calling Dispose.
    public static bool NullIsAllowed()
    {
        using (IDisposable? nothing = null)
        {
            return true;
        }
    }
}
