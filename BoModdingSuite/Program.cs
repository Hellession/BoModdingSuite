using System.Diagnostics;
using BoModdingSuite.Features;

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
    
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ./BoModdingSuite <function> [args...]");
            PrintHelp();
            return 0;
        }

        switch (args[0].ToLower())
        {
            case "unbundle":
            {
                var result = Unbundle.UnbundleFile(args.Length >= 2 ? args[1] : null,
                    args.Length >= 3 ? args[2] : null);
                if (result != null)
                    LogLineTimed(result.Value, "Operation complete.");
                else
                    return 1;
            }
                break;
            case "ptbToJson":
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Insufficient arguments for ptbToJson: ./BoModdingSuite ptbToJson <inputPtbPath> [outputPath] [-c|--compact]");
                    PrintHelp();
                    return 1;
                }
                var result = PtbToJson.Run(args[1..]);
                if(result != null)
                    LogLineTimed(result.Value, "Operation complete.");
                else
                    return 1;
            }
                break;
            default:
                Console.WriteLine("Unknown app function.");
                PrintHelp();
                return 1;
        }

        return 0;
    }

    public static void PrintHelp()
    {
        Console.WriteLine("<> = required argument, [] = optional argument. Don't include <> or []");
        Console.WriteLine("For arguments that contain spaces, like some file paths, surround them with \"");
        Console.WriteLine("The following functions are supported:");
        Console.WriteLine("- ./BoModdingSuite unbundle [hashtablePath] [outputDir] - Reads provided hashtable inside hashbundle, then appropriately unbundles all assets by copying into outputDir.");
        Console.WriteLine("- ./BoModdingSuite ptbToJson <inputPtbPath> [outputPath] [-c|--compact] - Converts Phobic Tree Binary into JSON. Beautifies by default, unless '--compact' is requested.");
    }
}