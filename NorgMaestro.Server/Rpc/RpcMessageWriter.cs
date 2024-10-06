using System.Text;
using System.Text.Json;

namespace NorgMaestro.Server.Rpc;

public interface IRpcWriter
{
    public void EncodeAndWrite(object o);
}

public interface IRpcReader
{
    public RpcMessage? Decode();
}

public class RpcMessageReader(Stream read) : IRpcReader
{
    private readonly StreamReader _reader = new(read, Encoding.UTF8);

    private const string ContentLengthHeader = "Content-Length: ";

    public RpcMessage? Decode()
    {
        string? header = _reader.ReadLine();
        bool headerExists = header?.StartsWith(ContentLengthHeader) ?? false;
        if (headerExists is false)
        {
            return null;
        }
        bool contentLengthExists = int.TryParse(
            string.Concat(header!.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit)),
            out int contentLength
        );
        if (contentLengthExists is false)
        {
            return null;
        }

        string? newline = _reader.ReadLine();
        bool newLinesExists = newline?.SequenceEqual("") ?? false;
        if (newLinesExists is false)
        {
            return null;
        }

        char[] buffer = new char[contentLength];
        _ = _reader.Read(buffer, 0, contentLength);
        string content = string.Concat(buffer);

        RpcMessage? pson = JsonSerializer.Deserialize<RpcMessage>(content);

        return pson;
    }
}

public class RpcMessageWriter(Stream write) : IRpcWriter
{
    private StreamWriter Writer { get; init; } = new(write, Encoding.UTF8);

    private const string ContentLengthHeader = "Content-Length: ";

    public void EncodeAndWrite(object o)
    {
        string body = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(o));
        Writer.Write(string.Concat(ContentLengthHeader, body.Length, "\r\n\r\n", body));
        Writer.Flush();
    }
}
