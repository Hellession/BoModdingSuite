using System.Diagnostics;

namespace BoModdingSuite.Features;

public static class PtbToJson
{
    public static double? Run(string[] args)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            //this is the first I am writing these arguments that should be supported at any position (-c or -o)
            //any argument can be one of those. Maybe I can handle arguments kind of like a queue?
            //proper arguments still have to be in the right order
            bool beautify = true;
            var targetFile = args[0];
            if(!File.Exists(targetFile))
                throw new InvalidOperationException("Input PTB file doesn't exist. Check your spelling.");
            string outputPath;
            if (args.Length < 2)
            {
                outputPath = Path.Join(Path.GetDirectoryName(targetFile),
                    Path.GetFileNameWithoutExtension(targetFile) + ".json");
                //I want to minimize accidental file overwrites. So let's try an alternate filename.
                outputPath = TryFindVacantNameFor(outputPath);
            }
            else if (args[1] == "-c" || args[1] == "--compact")
            {
                beautify = false;
            }
            else
            {
                //outputPath 
            }
            
            
            Program.LogLineTimed(stopwatch, beautify ? "Resulting JSON will be beautified (formatted). If you want a compact, but harder to read JSON, provide '-c' or '--compact' as an argument." : "Resulting JSON will be compact.");
        }
        catch (Exception e)
        {
            Program.LogLineTimed(stopwatch, e.Message);
            //TODO: Remove when tool is complete
            Program.LogLineTimed(stopwatch, e.StackTrace);
        }

        return null;
    }

    public static string TryFindVacantNameFor(string fullPath)
    {
        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            return fullPath;
        int suffix = 0;
        var directory = Path.GetDirectoryName(fullPath);
        var ext = Path.GetExtension(fullPath);
        var fileName = Path.GetFileNameWithoutExtension(fullPath);
        while (File.Exists(fullPath) || Directory.Exists(fullPath))
        {
            if(suffix>=51)
                throw new InvalidOperationException("Unable to find a vacant name for the output file (to avoid overwriting). Overwrites aren't allowed and there are 50 files or folders with names similar to the input file. Either remove some of those or allow overwrites with '-o' or '--overwrite'");
            suffix++;
            fullPath  = Path.Join(directory, fileName + "_" + suffix + ext);
        }

        return fullPath;
    }
}