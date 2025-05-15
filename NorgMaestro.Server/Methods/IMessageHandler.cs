using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public interface IMessageHandler
{
    Task<Response?> HandleRequest();
}
