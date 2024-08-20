using System;

namespace DesktopUtilsSharedLib.Extensions;

public static partial class ConsoleGreetings
{
    public static void Completed()
    {
        Console.WriteLine();
        Console.WriteLine("...completed.");
    }

    public static void Abort()
    {
        Console.WriteLine();
        Console.WriteLine("...aborted.");
    }

    public static void Abort(string reason)
    {
        Console.WriteLine();
        Console.WriteLine($"...aborted. (Reason: {reason})");
    }
}