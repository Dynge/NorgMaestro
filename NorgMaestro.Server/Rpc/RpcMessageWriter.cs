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
    private readonly Stream _stream = read;
    private readonly Stack<byte> _pushback = [];
    private bool _bomHandled;

    private const string ContentLengthHeader = "Content-Length: ";

    public async Task<RpcMessage?> DecodeAsync()
    {
        await Task.Yield();
        EnsureUtf8BomHandled();

        string? header = ReadAsciiLine();
        if (header is null || !header.StartsWith(ContentLengthHeader))
        {
            return null;
        }

        bool contentLengthExists = int.TryParse(
            string.Concat(header.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit)),
            out int contentLength
        );
        if (!contentLengthExists)
        {
            return null;
        }

        while (true)
        {
            string? nextHeader = ReadAsciiLine();
            if (nextHeader is null)
            {
                return null;
            }

            if (nextHeader.Length == 0)
            {
                break;
            }
        }

        byte[] buffer = new byte[contentLength];
        var totalRead = 0;
        while (totalRead < contentLength)
        {
            int readCount = _stream.Read(buffer, totalRead, contentLength - totalRead);
            if (readCount == 0)
            {
                return null;
            }

            totalRead += readCount;
        }

        string content = Encoding.UTF8.GetString(buffer);

        RpcMessage? pson = JsonSerializer.Deserialize<RpcMessage>(content);

        return pson;
    }

    private string? ReadAsciiLine()
    {
        var bytes = new List<byte>();
        while (true)
        {
            int raw = ReadByte();
            if (raw == -1)
            {
                return bytes.Count == 0 ? null : Encoding.ASCII.GetString(bytes.ToArray());
            }

            byte next = (byte)raw;
            if (next == (byte)'\n')
            {
                if (bytes.Count > 0 && bytes[^1] == (byte)'\r')
                {
                    bytes.RemoveAt(bytes.Count - 1);
                }

                return Encoding.ASCII.GetString(bytes.ToArray());
            }

            bytes.Add(next);
        }
    }

    private int ReadByte()
    {
        if (_pushback.Count > 0)
        {
            return _pushback.Pop();
        }

        return _stream.ReadByte();
    }

    private void EnsureUtf8BomHandled()
    {
        if (_bomHandled)
        {
            return;
        }

        _bomHandled = true;

        int first = ReadByte();
        if (first == -1)
        {
            return;
        }

        if (first != 0xEF)
        {
            _pushback.Push((byte)first);
            return;
        }

        int second = ReadByte();
        if (second == -1)
        {
            _pushback.Push((byte)first);
            return;
        }

        if (second != 0xBB)
        {
            _pushback.Push((byte)second);
            _pushback.Push((byte)first);
            return;
        }

        int third = ReadByte();
        if (third == -1)
        {
            _pushback.Push((byte)second);
            _pushback.Push((byte)first);
            return;
        }

        if (third != 0xBF)
        {
            _pushback.Push((byte)third);
            _pushback.Push((byte)second);
            _pushback.Push((byte)first);
        }
    }
}

public class RpcMessageWriter(Stream write) : IRpcWriter
{
    private StreamWriter Writer { get; init; } = new(write, new UTF8Encoding(false));

    private const string ContentLengthHeader = "Content-Length: ";

    public async Task EncodeAndWrite(object o)
    {
        byte[] bodyBytes = JsonSerializer.SerializeToUtf8Bytes(o, JsonOptions.Default);
        string body = Encoding.UTF8.GetString(bodyBytes);
        await Writer.WriteAsync(string.Concat(ContentLengthHeader, bodyBytes.Length, "\r\n\r\n", body));
        await Writer.FlushAsync();
    }
}
