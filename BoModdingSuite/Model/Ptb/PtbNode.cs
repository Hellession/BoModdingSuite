using System.Diagnostics;
using System.Text;
using BoModdingSuite.Util;

namespace BoModdingSuite.Model.Ptb;

/// <summary>
/// Stands for Phobic Tree Binary
/// community members read it as Phobic Table Binary, but BO2 code refers to it as 'ptree'.
/// </summary>
public class PtbNode
{
    /// <summary>
    /// Should only be true for the very root node of the entire PTB
    /// </summary>
    public bool IsRoot { get; set; }
    /// <summary>
    /// Only null for root
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Children count if Type == Object
    /// Value length otherwise
    /// </summary>
    public int Length { get; set; }
    public PtbNodeType Type { get; set; }
    public string? Value { get; set; }
    public List<PtbNode>? Children { get; set; }
    public string Path { get; set; } = "/";
    public PtbNode? Parent { get; set; }
    
    public long AddressOfName { get; set; }

    public int GetTotalNodeCount()
    {
        if(Children != null)
            return (IsRoot ? 0 : 1) + Children.Sum(t => t.GetTotalNodeCount());
        return IsRoot ? 0 : 1;
    }


    public void Read(FileStream fs, byte[] buffer)
    {
        if(fs.Read(buffer, 0, 1) == 0)
            throw new InvalidOperationException(Symbols.PrematureEof + " while reading flag for " + Path);
        byte flagByte = buffer[0];
        int nodeType = flagByte >> 4;
        Type = nodeType switch
        {
            0x0A => PtbNodeType.Object,
            0x09 => PtbNodeType.Value,
            0x08 => PtbNodeType.Null,
            _ => throw new InvalidOperationException(
                $"Invalid node type at {Path} (char {AddressOfName}), its type flag is {(flagByte >> 4)}")
        };
        if ((flagByte & 0x0F) == 0x0F)
            Length = ReadVarInt(fs, buffer);
        else
            Length = (flagByte & 0x0F);
        if (Length > 0 && Type == PtbNodeType.Null)
            throw new InvalidOperationException(
                $"Node at {Path} (char {AddressOfName}), is of Null type, yet is {Length} bytes big..?");
        if (Length == 0 && Type != PtbNodeType.Null)
            throw new InvalidOperationException(
                $"Node at {Path} (char {AddressOfName}), is {Length} bytes big, when expected to have data");
        if (Type == PtbNodeType.Null)
            return;
        if (Type == PtbNodeType.Value)
        {
            if(fs.Read(buffer, 0, Length) < Length)
                throw new InvalidOperationException(Symbols.PrematureEof + " while reading value for " + Path);
            Value = Encoding.ASCII.GetString(buffer, 0, Length);
            return;
        }

        Children = new List<PtbNode>(Length);
        for (var i = 0; i < Length; i++)
        {
            var nameLength = ReadVarInt(fs, buffer);
            var addressOfName = fs.Position;
            if(nameLength == 0)
                throw new InvalidOperationException($"Name for child {Children.Count+1} of {Path} at {AddressOfName} is 0 characters long");
            if(fs.Read(buffer, 0, nameLength) < nameLength)
                throw new InvalidOperationException(Symbols.PrematureEof + " while reading value for " + Path);
            var newChild = new PtbNode()
            {
                Name = Encoding.ASCII.GetString(buffer, 0, nameLength),
                Path = Path,
                AddressOfName = addressOfName
            };
            if (!Path.EndsWith('/'))
                Path += '/';
            Path += newChild.Name;
            newChild.Read(fs, buffer);
            //adding this one line appears to have made the reader 50% slower
            Children.Add(newChild);
        }
    }

    public static int ReadVarInt(FileStream fs, byte[] buffer)
    {
        int length = 0;
        int result = 0;
        do
        {
            if (fs.Read(buffer, 0, 1) == 0)
                throw new InvalidOperationException(Symbols.PrematureEof + " while reading variable int");
            length++;
            if (length > 32)
                throw new InvalidOperationException(
                    $"Attempted to read variable integer at char {fs.Position - length}, but have been reading for more than 32 bytes and still haven't reached the end!");
            //I don't know why but a warning popped up and this is what my IDE did
            //Don't get why casting to uint is better over ulong
            result = (buffer[0] & 0x7F) | (result << 7);
        } while ((buffer[0] & 0x80) != 0);

        return result;
    }

    public static PtbNode ReadPtb(Stopwatch stopwatch, string hashtablePath)
    {
        //I am using a single array to not have to needlessly create new ones each time new data is read
        byte[] buffer = new byte[1024];
        int offset = 0;
        using FileStream fs = File.Open(hashtablePath, FileMode.Open);
        int bytesRead = fs.Read(buffer, 0, 3);
        if (bytesRead < 3 || buffer is not [0x50, 0x54, 0x42, ..])
        {
            throw new InvalidOperationException($"Invalid file header ({buffer[0]:X2} {buffer[1]:X2} {buffer[3]:X2}). Either the file is not a valid hashtable or is corrupt.");
        }
        if(fs.Read(buffer, 0, 1) == 0)
            throw new InvalidOperationException(Symbols.PrematureEof);
        byte ptbVersion = buffer[0];
        if(ptbVersion != 1 && ptbVersion != 2)
            throw new InvalidOperationException("Unknown PTB version " + ptbVersion);
        Program.LogLineTimed(stopwatch, "PTB Read: Identified version " + ptbVersion);

        var result = new PtbNode()
        {
            IsRoot = true,
            AddressOfName = fs.Position
        };
        result.Read(fs, buffer);
        //Encoding.ASCII.GetString() <--- use this later
        return result;
    }
}