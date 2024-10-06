using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class EmptyHandler(RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request =request;

    public Response? HandleRequest()
    {
        return _request.Id switch
        {
            int id => Response.OfSuccess(id),
            _ => null,
        };
    }
}
