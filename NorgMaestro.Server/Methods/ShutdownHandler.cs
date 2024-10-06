using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class ShutdownHandler(IRpcWriter writer, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage Request = request;
    private readonly IRpcWriter Writer = writer;

    public Response? HandleRequest()
    {
        Writer.EncodeAndWrite(Notification.Default("Shutting down..."));
        return Response.OfSuccess(Request.Id ?? 0);
    }
}

public class ExitHandler(IRpcWriter writer) : IMessageHandler
{
    private readonly IRpcWriter Writer = writer;

    public Response? HandleRequest()
    {
        Writer.EncodeAndWrite(Notification.Default("Goodbye!"));
        throw new InvalidDataException("Shutting down.");
    }
}
