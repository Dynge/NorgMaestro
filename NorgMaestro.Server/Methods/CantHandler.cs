using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CantHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }
    public required IRpcWriter Writer { get; init; }

    public Response? HandleRequest()
    {
        Writer.EncodeAndWrite(
            Notification.Default($"Cannot handle '{Request.Method}'!!.", MessageType.Warning)
        );
        return null;
    }
}
