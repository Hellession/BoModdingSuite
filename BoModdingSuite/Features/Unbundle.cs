using System.Diagnostics;
using System.Text;
using BoModdingSuite.Model;
using BoModdingSuite.Model.Ptb;

namespace BoModdingSuite.Features;

public static class Unbundle
{
    /*
     * TODO: Rewrite a lot of this and move it to .md, there is no reason for this to be a huge comment.
     * also when I wrote this initially, I had the wrong information on many parts. This is just wrong
     * 
     * Credit to Spikeyy's PtB tool for giving me clues on the format.
     * I still haven't fully figured it out tho
     * 
     * DATA TYPE FORMATTING IN HASHBUNDLES:
     * Strings always take the following structure:
     * 1. VARINT of string length in bytes
     * 2. string itself
     *
     * Format for hashbundles in these games:
     * MAGIC NUMBERS - 50 53 42
     * 
     * VERSION (maybe) - 02 - for the PC port, seems to be 01 in the mobile versions
     * Version 02 appears to support tags, Version 01 doesn't have tags
     *
     * FLAG - AF in the case of BO2's PC port. The flag entry might be the same for every object,
     * as this format isn't just used for Hashbundles
     * SIZE? - VARINT of the amount of children present... I think, because what I have doesn't match up.
     *
     * [List of items follows until the very end of file]
     *
     * FILE PATH (string) - keep reading this until the separator
     * FLAG? - A1 (if the item has no tags) A2 (if the item HAS tags)
     * SHA1 Marker (string) - it always says 'sha1' (73 68 61 31).
     * SEPARATOR - 9F
     * HASH (string) - the hash for the file, keep reading until the hex characters stop or you hit separator
     * 
     * [only if the item has tags]
     * HEADER (string) - 'tags' (74 61 67 73), preceded by 04 for length
     * SEPARATOR/FLAG? - A1
     * TAG? (string) - [any characters]. Known tags: '-lite', '+ogg' and '+steam'.
     * Idk what '-' or '+' before the name mean
     * TERMINATOR - 80. It's unknown if items with multiple tags would use this as a separator or not
     *
     * The next entry immediately starts with the first string, in particular, its length.
     */

    public const string HashTableName = "hashtable";
    public const string HashBundleName = "hashbundle";
    public const string DefaultUnbundleDestination = "unbundled";
    
    
    public static double? UnbundleFile(string? hashtablePath, string? outputDestination = null)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            if (string.IsNullOrEmpty(hashtablePath))
            {
                //try to find the hashtable in the current location
                if (File.Exists(HashTableName))
                    hashtablePath = HashTableName;
                else if (File.Exists(Path.Join(HashBundleName, HashTableName)))
                    hashtablePath = Path.Join(HashBundleName, HashTableName);
                else
                    throw new ArgumentNullException(nameof(hashtablePath),
                        "Path of the hashbundle cannot be null, if the command isn't fired in hashbundle's directory. Unable to find hashtable of the bundle.");
            }

            if (Directory.Exists(hashtablePath))
            {
                //odd repeating of expressions lol
                if (File.Exists(Path.Join(hashtablePath, HashTableName)))
                    hashtablePath = Path.Join(hashtablePath, HashTableName);
                else if (File.Exists(Path.Join(hashtablePath, HashBundleName, HashTableName)))
                    hashtablePath = Path.Join(hashtablePath, HashBundleName, HashTableName);
                else
                    throw new ArgumentException(
                        "Cannot find the hashbundle at the specified directory. Make sure the directory provided contains the hashtable file within.", nameof(hashtablePath));
            }

            if (!File.Exists(hashtablePath))
                throw new ArgumentException(
                    "Specified hashtable file doesn't exist. Check your spelling.", nameof(hashtablePath));

            //output destination may not be provided
            //if it's not, in the same folder as hashtable, created an 'unbundled' folder
            //however you need to make sure a folder like that doesn't exist, if it does and contains 'hashtable'
            //add a suffix to the destination until you find something that doesn't yet exist
            //if the output destination is provided, throw an error if 'hashtable' exists in that folder
            string hashbundleDirectory = Path.GetDirectoryName(hashtablePath) ?? throw new InvalidOperationException("The housing directory of the hashtable is either root or null.");
            if (string.IsNullOrEmpty(outputDestination))
            {
                outputDestination = Path.Join(hashbundleDirectory, DefaultUnbundleDestination);
                int suffix = 0;
                while ((Directory.Exists(outputDestination) && File.Exists(Path.Join(outputDestination, HashTableName))) || File.Exists(outputDestination))
                {
                    if(suffix>=51)
                        throw new ArgumentNullException(nameof(outputDestination),
                            "Unable to find a fitting destination for the output. Tried over 50 names in the current folder. Please specify the output or clear out the space.");
                    suffix++;
                    outputDestination  = Path.Join(Path.GetDirectoryName(hashtablePath), DefaultUnbundleDestination + "_" + suffix);
                }
            }
            if(Directory.Exists(outputDestination) && File.Exists(Path.Join(outputDestination, HashTableName)))
                throw new ArgumentNullException(nameof(outputDestination),
                    "The output location you specified already appears to have an unbundled hashtable. It will not be overwritten. Delete that folder and try again or pick a different one.");
            if(File.Exists(outputDestination))
                throw new ArgumentNullException(nameof(outputDestination),
                    "A file exists at the specified output location. Cannot write things there.");

            if (!Directory.Exists(outputDestination))
                Directory.CreateDirectory(outputDestination);

            PtbNode result;
            try
            {
                result = PtbNode.ReadPtb(stopwatch, hashtablePath);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("PTB Read: " + e.Message);
            }
            Program.LogLineTimed(stopwatch, $"Finished reading PTB file. Root node has {result.Length} children. Total amount of nodes: {result.GetTotalNodeCount()}");
            if(result.Children == null)
                throw new InvalidOperationException("Malformed hashtable: Root node's Children are null. Root node is of type " + result.Type);
            if(result.Children.Count == 0)
                throw new InvalidOperationException("Malformed hashtable: Root node has 0 children");
            int copedFiles = 0;
            int malformedFiles = 0;
            int fileErrors = 0;
            foreach (var file in result.Children)
            {
                if(file.Children == null)
                {
                    Program.LogLineTimed(stopwatch,
                        $"Malformed file {file.Name}: Node's children are null. Root node is of type " + file.Type);
                    malformedFiles++;
                    continue;
                }
                if(file.Children.Count == 0)
                {
                    Program.LogLineTimed(stopwatch,
                        $"Malformed file {file.Name}: Node has 0 children. Cannot determine SHA1 hash");
                    malformedFiles++;
                    continue;
                }

                var hashNode = file.Children.FirstOrDefault(t => t.Name == "sha1");
                if(hashNode == null)
                {
                    Program.LogLineTimed(stopwatch, $"{file.Name}: Has no SHA1 hash node");
                    malformedFiles++;
                    continue;
                }
                if(hashNode.Type != PtbNodeType.Value)
                {
                    Program.LogLineTimed(stopwatch, $"{file.Name}: SHA1 hash node is of type {hashNode.Type}. Expected Value type");
                    malformedFiles++;
                    continue;
                }

                var hashedFilePath = Path.Join(hashbundleDirectory, hashNode.Value);
                if(!File.Exists(hashedFilePath))
                {
                    Program.LogLineTimed(stopwatch, $"{file.Name}: Hashed file doesn't exist. Hash: {hashNode.Value}");
                    fileErrors++;
                    continue;
                }

                var destinationPath = Path.Join(outputDestination, file.Name);
                var destinationDirectory = Path.GetDirectoryName(destinationPath);

                if (!Directory.Exists(destinationDirectory) && destinationDirectory != null)
                    Directory.CreateDirectory(destinationDirectory);
                if (File.Exists(destinationPath))
                {
                    Program.LogLineTimed(stopwatch, $"WARNING - {file.Name} (Hash {hashNode.Value}): destination file already exists. NOT overwriting.");
                    fileErrors++;
                    continue;
                }
                File.Copy(hashedFilePath, destinationPath);
                copedFiles++;
            }
            //this literally makes no sense and shouldn't be possible,
            //but I only added this to supress the stupid warning on the following log call
            if(result.Children == null)
                throw new InvalidOperationException("Malformed hashtable???? somehow?: Root node's Children are null. Root node is of type " + result.Type);
            
            Program.LogLineTimed(stopwatch, $"Total files in bundle: {result.Length} ({result.Children.Count} children)");
            Program.LogLineTimed(stopwatch, $"Files copied successfully: {copedFiles}");
            Program.LogLineTimed(stopwatch, $"Malformed file nodes: {malformedFiles}");
            Program.LogLineTimed(stopwatch, $"File errors: {fileErrors}");
            Program.LogLineTimed(stopwatch, "Making a copy of the hashtable in the output...");
            File.Copy(hashtablePath, Path.Join(outputDestination, HashTableName));
            Program.LogLineTimed(stopwatch, "Operation complete.");
            return stopwatch.Elapsed.TotalSeconds;
        }
        catch (Exception e)
        {
            Program.LogLineTimed(stopwatch, e.Message);
        }

        return null;
    }
}