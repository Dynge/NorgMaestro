using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CantHandler(IRpcWriter writer, RpcMessage request) : IMessageHandler
{
    private readonly IRpcWriter Writer = writer;
    private readonly RpcMessage Request = request;

    public Response? HandleRequest()
    {
        Writer.EncodeAndWrite(
            Notification.Default($"Cannot handle '{Request.Method}'!!.", MessageType.Warning)
        );
        return null;
    }
}
