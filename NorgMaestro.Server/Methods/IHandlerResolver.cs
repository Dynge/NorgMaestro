using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public interface IHandlerResolver
{
    Task<bool> TryHandleRequest(RpcMessage req);
}
