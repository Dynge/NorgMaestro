using System.Text;
using FluentAssertions;
using NorgMaestro.Server.Rpc;
using static NorgMaestro.Server.Methods.HandlerFactory;

namespace NorgMaestro.Tests;

public sealed class RpcReaderTests
{
    private readonly StreamWriter _inputWriter;
    private readonly Stream _inputStream;
    private readonly RpcMessageReader _reader;

    public RpcReaderTests()
    {
        _inputStream = new MemoryStream { Position = 0 };
        _reader = new RpcMessageReader(_inputStream);
        _inputWriter = new StreamWriter(_inputStream, Encoding.UTF8);
    }

    private async Task SendToStream(string data)
    {
        await _inputWriter.WriteAsync(data);
        await _inputWriter.FlushAsync();
        _inputStream.Position = 0;
    }

    [Fact]
    public async Task ShouldReadJsonRpcMessage()
    {
        var data = await File.ReadAllTextAsync("./Resources/RpcMessages/initialize");
        await SendToStream(data);

        var decodedMessage = await _reader.DecodeAsync();
        if (decodedMessage is null)
        {
            Assert.Fail("decodedMessage is null");
        }
        decodedMessage.Method.Should().Be(MethodType.Initialize);
    }
}
