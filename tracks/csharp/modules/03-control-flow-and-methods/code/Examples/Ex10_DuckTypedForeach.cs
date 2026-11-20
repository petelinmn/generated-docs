// Example: foreach needs only a GetEnumerator() pattern, not the IEnumerable interface.
// Module: csharp/03-control-flow-and-methods (expert)
// Tests: dotnet test --filter "FullyQualifiedName~EvenNumbers"
namespace Examples;

// No interfaces at all: foreach finds GetEnumerator, MoveNext and Current by name.
// The enumerator is a struct, so the loop allocates nothing on the heap.
public readonly struct EvenNumbers
{
    private readonly int _max;

    public EvenNumbers(int max) => _max = max;

    public Enumerator GetEnumerator() => new(_max);

    public struct Enumerator
    {
        private readonly int _max;
        private int _current;

        public Enumerator(int max)
        {
            _max = max;
            _current = -2;
        }

        public readonly int Current => _current;

        public bool MoveNext()
        {
            _current += 2;
            return _current <= _max;
        }
    }
}
