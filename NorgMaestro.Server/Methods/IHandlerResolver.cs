using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public interface IHandlerResolver
{
    bool TryHandleRequest(RpcMessage req);
}
