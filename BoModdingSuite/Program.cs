using System.Diagnostics;

namespace BoModdingSuite;

public static class Program
{
    public static void LogLineTimed(Stopwatch stopwatch, object? entry)
    {
        LogLineTimed(stopwatch.Elapsed.TotalSeconds, entry);
    }
    
    public static void LogLineTimed(double time, object? entry)
    {
        Console.WriteLine($"[{Math.Round(time, 2)}] " + entry);
    }
    
    public static void Main(string[] args)
    {
        Console.WriteLine("I just created this repo bro, so far there's nothing");
        
    }
}