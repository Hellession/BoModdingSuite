using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace BoModdingSuite.Model.Ptb;

public class PtbJsonConverter : JsonConverter<PtbNode>
{
    public override PtbNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //reader.pos
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, PtbNode value, JsonSerializerOptions options)
    {
        if (value.IsRoot || string.IsNullOrEmpty(value.Name))
        {
            writer.WriteStartObject();
            switch (value.Type)
            {
                case PtbNodeType.Null:
                    writer.WriteNullValue();
                    break;
                case PtbNodeType.Value:
                    writer.WriteStringValue(value.Value);
                    break;
                case PtbNodeType.Object:
                    if (value.Children == null)
                        throw new InvalidOperationException("Root node is Object, but has null children");
                    foreach (var childNode in value.Children)
                    {
                        Write(writer, childNode, options);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            writer.WriteEndObject();
            return;
        }
        switch (value.Type)
        {
            case PtbNodeType.Null:
                writer.WriteNull(value.Name);
                break;
            case PtbNodeType.Value:
                writer.WriteString(value.Name, value.Value);
                break;
            case PtbNodeType.Object:
                writer.WriteStartObject(value.Name);
                if (value.Children == null)
                    throw new InvalidOperationException("Root node is Object, but has null children");
                foreach (var childNode in value.Children)
                {
                    Write(writer, childNode, options);
                }
                writer.WriteEndObject();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}