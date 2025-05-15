using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class NoneHandler : IMessageHandler
{
    public Task<Response?> HandleRequest()
    {
        return Task.FromResult<Response?>(null);
    }
}
