using NorgMaestro.Rpc;

namespace NorgMaestro.Methods;

public class EmptyHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }

    public Response? HandleRequest()
    {
        return Request.Id switch
        {
            int id => Response.OfSuccess(id),
            _ => null,
        };
    }
}
