using System.Text;
using System.Text.Json;

namespace NorgMaestro.Server.Rpc;

public interface IRpcWriter
{
    public Task EncodeAndWrite(object o);
}

public interface IRpcReader
{
    public Task<RpcMessage?> DecodeAsync();
}

public class RpcMessageReader(Stream read) : IRpcReader
{
    private readonly StreamReader _reader = new(read, Encoding.UTF8);

    private const string ContentLengthHeader = "Content-Length: ";

    public async Task<RpcMessage?> DecodeAsync()
    {
        string? header = await _reader.ReadLineAsync();
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

        string? newline = await _reader.ReadLineAsync();
        bool newLinesExists = newline?.SequenceEqual("") ?? false;
        if (newLinesExists is false)
        {
            return null;
        }

        var buffer = new char[contentLength];
        _ = await _reader.ReadAsync(buffer, 0, contentLength);

        RpcMessage? pson = JsonSerializer.Deserialize<RpcMessage>(string.Concat(buffer));

        return pson;
    }
}

public class RpcMessageWriter(Stream write) : IRpcWriter
{
    private StreamWriter Writer { get; init; } = new(write, Encoding.UTF8);

    private const string ContentLengthHeader = "Content-Length: ";

    public async Task EncodeAndWrite(object o)
    {
        string body = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(o));
        await Writer.WriteAsync(string.Concat(ContentLengthHeader, body.Length, "\r\n\r\n", body));
        await Writer.FlushAsync();
    }
}
