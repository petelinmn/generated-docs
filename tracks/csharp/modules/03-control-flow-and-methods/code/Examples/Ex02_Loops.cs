// Example: for, while, do-while, foreach, break and continue.
// Module: csharp/03-control-flow-and-methods (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Loops"
namespace Examples;

public static class Loops
{
    public static int SumTo(int n)
    {
        int sum = 0;
        for (int i = 1; i <= n; i++)
        {
            sum += i;
        }
        return sum;
    }

    // while checks the condition before each iteration, so the body may never run.
    public static int DigitSum(int n)
    {
        n = Math.Abs(n);
        int sum = 0;
        while (n > 0)
        {
            sum += n % 10;
            n /= 10;
        }
        return sum;
    }

    // do-while runs the body at least once: 0 still has one digit.
    public static int CountDigits(int n)
    {
        n = Math.Abs(n);
        int count = 0;
        do
        {
            count++;
            n /= 10;
        }
        while (n > 0);
        return count;
    }

    // break leaves the loop, continue jumps to the next iteration.
    public static int SumEvenUntilZero(int[] values)
    {
        int sum = 0;
        foreach (int value in values)
        {
            if (value == 0)
            {
                break;
            }
            if (value % 2 != 0)
            {
                continue;
            }
            sum += value;
        }
        return sum;
    }

    // C# 15: break and continue can name the label of an enclosing loop.
    // Before C# 15, leaving both loops at once needed goto or a flag variable.
    public static (int Row, int Column)? FindFirst(int[][] grid, int target)
    {
        (int Row, int Column)? found = null;
        outer:
        for (int row = 0; row < grid.Length; row++)
        {
            for (int column = 0; column < grid[row].Length; column++)
            {
                if (grid[row][column] == target)
                {
                    found = (row, column);
                    break outer;
                }
            }
        }
        return found;
    }

    // continue with a label skips the rest of the current row and moves to the next one.
    public static int CountRowsWithoutNegatives(int[][] grid)
    {
        int count = 0;
        rows:
        foreach (int[] row in grid)
        {
            foreach (int value in row)
            {
                if (value < 0)
                {
                    continue rows;
                }
            }
            count++;
        }
        return count;
    }
}
