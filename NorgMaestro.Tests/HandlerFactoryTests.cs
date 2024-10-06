using FluentAssertions;
using NorgMaestro.Server.Methods;
using NorgMaestro.Server.Rpc;
using static NorgMaestro.Server.Methods.HandlerFactory;

namespace NorgMaestro.Tests;

public sealed class HandlerFactoryTests
{
    private readonly HandlerFactory _handlerFactory;

    public HandlerFactoryTests()
    {
        var stream = new MemoryStream();
        _handlerFactory = new(new(), new RpcMessageWriter(stream));
    }

    public static IEnumerable<object[]> MethodNames()
    {
        IEnumerable<object[]> methodNames = typeof(MethodType)
            .GetFields()
            .Select(a => new List<object>() { a.GetRawConstantValue()!.ToString()! }.ToArray());
        return methodNames;
    }

    [Theory]
    [MemberData(nameof(MethodNames))]
    public void ShouldCreateHandlerFromMethodString(string methodName)
    {
        var rpcMessage = new RpcMessage()
        {
            Id = 1,
            Method = methodName,
            JsonRpc = "1",
        };
        var handler = _handlerFactory.CreateHandler(rpcMessage);
        (handler is not CantHandler).Should().BeTrue();
    }

    [Fact]
    public void ShouldCreateCantHandlerOnUnknown()
    {
        var rpcMessage = new RpcMessage()
        {
            Id = 1,
            Method = "unknown",
            JsonRpc = "1",
        };
        var handler = _handlerFactory.CreateHandler(rpcMessage);
        (handler is CantHandler).Should().BeTrue();
    }
}
