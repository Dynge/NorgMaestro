using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class EmptyHandler(RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;

    public Task<Response?> HandleRequest()
    {
        return _request.Id switch
        {
            int id => Task.FromResult<Response?>(Response.OfSuccess(id)),
            _ => Task.FromResult<Response?>(null),
        };
    }
}
