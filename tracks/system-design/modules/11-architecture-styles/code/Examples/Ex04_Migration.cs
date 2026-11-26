// Example: an anti-corruption layer and branch by abstraction.
// Module: system-design/11-architecture-styles (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Migration"
namespace Architecture.Examples.Migration;

// The legacy model: money in cents, a status encoded as a letter.
public sealed record LegacyCustomer(string CUST_NO, long BAL_CENTS, string STATUS_CD);

public sealed record Customer(string Id, decimal Balance, bool IsActive);

// Anti-corruption layer: the only place that knows the legacy shape.
public static class LegacyTranslator
{
    public static Customer ToDomain(LegacyCustomer c) => new(
        c.CUST_NO.Trim(),
        c.BAL_CENTS / 100m,
        c.STATUS_CD switch
        {
            "A" => true,
            "C" => false,
            _ => throw new InvalidOperationException($"Unknown legacy status '{c.STATUS_CD}'."), // do not guess
        });
}

// Branch by abstraction: callers depend on this, both implementations live side by side.
public interface IPriceCalculator
{
    decimal Price(int quantity);
}

public sealed class OldPrices : IPriceCalculator
{
    public decimal Price(int quantity) => quantity * 10m;
}

public sealed class NewPrices : IPriceCalculator
{
    public decimal Price(int quantity) => quantity >= 10 ? quantity * 9m : quantity * 10m;
}

// Answers with the old result and records where the new one differs.
public sealed class Comparing(IPriceCalculator current, IPriceCalculator candidate) : IPriceCalculator
{
    public List<(int Quantity, decimal Current, decimal Candidate)> Differences { get; } = [];

    public decimal Price(int quantity)
    {
        decimal result = current.Price(quantity);
        try
        {
            decimal other = candidate.Price(quantity);
            if (other != result) Differences.Add((quantity, result, other));
        }
        catch (Exception)
        {
            // a broken candidate must not break the caller
        }
        return result;
    }
}
