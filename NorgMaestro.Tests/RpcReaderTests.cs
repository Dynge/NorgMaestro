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

    private void SendToStream(string data)
    {
        _inputWriter.Write(data);
        _inputWriter.Flush();
        _inputStream.Position = 0;
    }

    [Fact]
    public void ShouldReadJsonRpcMessage()
    {
        var data = File.ReadAllText("./Resources/RpcMessages/initialize");
        SendToStream(data);

        var decodedMessage = _reader.Decode();
        if (decodedMessage is null)
        {
            Assert.Fail("decodedMessage is null");
        }
        decodedMessage.Method.Should().Be(MethodType.Initialize);
    }
}
