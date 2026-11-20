// Example: a switch expression that covers every named enum value can still fail at run time.
// Module: csharp/03-control-flow-and-methods (expert)
// Tests: dotnet test --filter "FullyQualifiedName~SwitchExhaustiveness"
namespace Examples;

public enum TrafficLight
{
    Red,
    Yellow,
    Green,
}

public static class SwitchExhaustiveness
{
    // An enum variable can hold any value of its underlying type: (TrafficLight)42 is legal.
    // The compiler warns with CS8524 and emits a throw of SwitchExpressionException for unmatched input.
#pragma warning disable CS8524
    public static string Action(TrafficLight light) => light switch
    {
        TrafficLight.Red => "stop",
        TrafficLight.Yellow => "wait",
        TrafficLight.Green => "go",
    };
#pragma warning restore CS8524

    // An explicit discard arm documents what happens with unexpected values.
    public static string SafeAction(TrafficLight light) => light switch
    {
        TrafficLight.Red => "stop",
        TrafficLight.Yellow => "wait",
        TrafficLight.Green => "go",
        _ => throw new ArgumentOutOfRangeException(nameof(light), light, "Unknown traffic light"),
    };
}
