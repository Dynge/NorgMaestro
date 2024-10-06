using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class ShutdownHandler(IRpcWriter writer, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly IRpcWriter _writer = writer;

    public Response? HandleRequest()
    {
        _writer.EncodeAndWrite(Notification.Default("Shutting down..."));
        return Response.OfSuccess(_request.Id ?? 0);
    }
}

public class ExitHandler(IRpcWriter writer) : IMessageHandler
{
    private readonly IRpcWriter _writer = writer;

    public Response? HandleRequest()
    {
        _writer.EncodeAndWrite(Notification.Default("Goodbye!"));
        throw new InvalidDataException("Shutting down.");
    }
}
