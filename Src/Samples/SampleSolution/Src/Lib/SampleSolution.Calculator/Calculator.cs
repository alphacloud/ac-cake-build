namespace SampleSolution.Calculator;

/// <summary>Trivial calculator used to produce code coverage.</summary>
public static class Calculator
{
    /// <summary>Adds two numbers.</summary>
    public static int Add(int a, int b) => a + b;

    /// <summary>Divides two numbers.</summary>
    public static int Divide(int a, int b) =>
        b == 0 ? throw new DivideByZeroException() : a / b;

    /// <summary>Returns the name of the target framework the assembly was compiled for.</summary>
    public static string Framework()
    {
#if NET10_0
        return "net10.0";
#elif NET8_0
        return "net8.0";
#else
        return "netstandard2.0";
#endif
    }

    /// <summary>Not called by tests: shows up as uncovered.</summary>
    public static int Multiply(int a, int b) => a * b;
}
