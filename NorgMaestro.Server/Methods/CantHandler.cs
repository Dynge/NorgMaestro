using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CantHandler(IRpcWriter writer, RpcMessage request) : IMessageHandler
{
    private readonly IRpcWriter _writer = writer;
    private readonly RpcMessage _request = request;

    public async Task<Response?> HandleRequest()
    {
        await _writer.EncodeAndWrite(
            Notification.Default($"Cannot handle '{_request.Method}'!!.", MessageType.Warning)
        );
        return null;
    }
}
