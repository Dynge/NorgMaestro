using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Tests;

public sealed class RpcWriterTests
{
    private readonly RpcMessageWriter _writer;
    private readonly StreamReader _inputReader;
    private readonly Stream _outputStream;

    public RpcWriterTests()
    {
        _outputStream = new MemoryStream { Position = 0 };
        _writer = new RpcMessageWriter(_outputStream);
        _inputReader = new StreamReader(_outputStream, Encoding.UTF8);
    }

    [Fact]
    public async Task ShouldEncodeAndWriteToStream()
    {
        List<CompletionItem> itemToEncode = [new() { Label = "Hello" }];
        await _writer.EncodeAndWrite(itemToEncode);

        var itemToEncodeAsJson = JsonSerializer.Serialize(itemToEncode);
        ReadDataFromStream()
            .Should()
            .MatchRegex($"Content-Length: \\d+\\r\\n\\r\\n{Regex.Escape(itemToEncodeAsJson)}");
    }

    private string ReadDataFromStream()
    {
        _outputStream.Position = 0;
        return _inputReader.ReadToEnd();
    }
}
