using System.Text;
using FluentAssertions;
using NorgMaestro.Server.Rpc;

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

    private void SendToStream(string data)
    {
        _inputWriter.Write(data);
        _inputWriter.Flush();
        _inputStream.Position = 0;
    }

    [Fact]
    public void ShouldReadJsonRpcMessage()
    {
        var data = File.ReadAllText("/home/michael/git/neorg-lsp/NorgMaestro.Server/rpc-message");
        SendToStream(data);

        var decodedMessage = _reader.Decode();
        decodedMessage.Should().NotBeNull();
    }
}
