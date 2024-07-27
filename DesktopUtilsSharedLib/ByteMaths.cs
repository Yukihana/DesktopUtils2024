using System;

namespace DesktopUtilsSharedLib;

public static partial class ByteMaths
{
    public static long MegabytesToBytes(double mb, RoundingMode mode = RoundingMode.Floor)
    {
        double b = mb * 1_000_000;
        b = mode switch
        {
            RoundingMode.Ceiling => Math.Ceiling(b),
            RoundingMode.Round => Math.Round(b),
            _ => b,
        };
        return (long)b;
    }

    public static long MebibytesToBytes(double mib, RoundingMode mode = RoundingMode.Floor)
    {
        double b = mib * 1024 * 1024;
        b = mode switch
        {
            RoundingMode.Ceiling => Math.Ceiling(b),
            RoundingMode.Round => Math.Round(b),
            _ => b,
        };
        return (long)b;
    }
}