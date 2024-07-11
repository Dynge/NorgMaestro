using System.Text;
using System.Text.Json;

namespace NorgMaestro.Rpc;

public interface IRpcWriter
{
    public void EncodeAndWrite(object o);
}

public interface IRpcReader
{
    public RpcMessage? Decode();
}

public class RpcMessageReaderWriter : IRpcReader, IRpcWriter
{
    private StreamReader Reader { get; init; }
    private StreamWriter Writer { get; init; }

    public RpcMessageReaderWriter(Stream read, Stream write)
    {
        Reader = new(read, Encoding.UTF8);
        Writer = new(write, Encoding.UTF8) { AutoFlush = true };
    }

    private const string ContentLengthHeader = "Content-Length: ";

    public void EncodeAndWrite(object o)
    {
        string body = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(o));
        Writer.Write(string.Concat(ContentLengthHeader, body.Length, "\r\n\r\n", body));
    }

    public RpcMessage? Decode()
    {
        string? header = Reader.ReadLine();
        bool headerExists = header?.StartsWith(ContentLengthHeader) ?? false;
        if (headerExists is false)
        {
            // Console.WriteLine(
            //     string.Format(
            //         "The RPC message '{0}' did not start with '{1}'.",
            //         string.Concat(headerSize),
            //         ContentLengthHeader
            //     )
            // );
            return null;
        }
        bool contentLengthExists = int.TryParse(
            string.Concat(header!.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit)),
            out int contentLength
        );
        if (contentLengthExists is false)
        {
            // Console.WriteLine("Jamen kom nu.. der sku være tal her...");
            return null;
        }
        // Console.WriteLine("Content length is: " + contentLength);

        string? newline = Reader.ReadLine();
        bool newLinesExists = newline?.SequenceEqual("") ?? false;
        if (newLinesExists is false)
        {
            // Console.WriteLine(
            //     string.Format(
            //         "Jamen kom nu.. der sku være \\r\\n\\r\\n her... ({0})",
            //         string.Concat(newline)
            //     )
            // );
            return null;
        }

        char[] buffer = new char[contentLength];
        _ = Reader.Read(buffer, 0, contentLength);
        string content = string.Concat(buffer);

        RpcMessage? pson = JsonSerializer.Deserialize<RpcMessage>(content);
        return pson;

        // Console.WriteLine(pson?.ToString());
        // Console.WriteLine(pson?.Params?.Deserialize<InitializeRequestParams>());
    }
}
