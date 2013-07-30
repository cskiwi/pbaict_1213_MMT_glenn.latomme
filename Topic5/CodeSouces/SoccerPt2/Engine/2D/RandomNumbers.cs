using System;

public static class RandomNumbers {
    private static Random _r;

    internal static double NextNumber() {
        if (_r == null)
            Seed();
        return _r.Next();
    }

    internal static int NextNumber(int ceiling) {
        if (_r == null)
            Seed();

        return _r.Next(ceiling);
    }

    internal static void Seed() {
        _r = new Random();
    }

    internal static void Seed(int seed) {
        _r = new Random(seed);
    }

    public static double RandInRange(double x, double y)
    {
	    return x + _r.NextDouble()*(y-x);
    }
}