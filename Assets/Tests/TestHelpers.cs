using System;

public static class TestHelpers
{
    public static bool ApproximatelyEqual(double expected, double result, double margin)
    {
        return Math.Abs(expected - result) <= margin;
    }
}