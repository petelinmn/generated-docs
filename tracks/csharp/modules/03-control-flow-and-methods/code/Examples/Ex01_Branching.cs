// Example: if/else, switch statement, switch expression, relational patterns.
// Module: csharp/03-control-flow-and-methods (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Branching"
using System.Globalization;

namespace Examples;

public static class Branching
{
    public static string Sign(int value)
    {
        if (value > 0)
        {
            return "positive";
        }
        else if (value < 0)
        {
            return "negative";
        }
        else
        {
            return "zero";
        }
    }

    // A switch statement: every section must end with break, return, throw or goto — no fall-through.
    public static string DayType(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Saturday:
            case DayOfWeek.Sunday:
                return "weekend";
            default:
                return "workday";
        }
    }

    // A switch expression over a tuple (C# 8+): the first matching arm wins.
    public static string FizzBuzz(int n) => (n % 3, n % 5) switch
    {
        (0, 0) => "FizzBuzz",
        (0, _) => "Fizz",
        (_, 0) => "Buzz",
        _ => n.ToString(CultureInfo.InvariantCulture),
    };

    // Relational and logical patterns (C# 9+).
    public static string Classify(int celsius) => celsius switch
    {
        < 0 => "freezing",
        >= 0 and < 20 => "cold",
        >= 20 and < 30 => "warm",
        _ => "hot",
    };
}
