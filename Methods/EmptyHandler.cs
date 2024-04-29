using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class EmptyHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }

        public Response? HandleRequest()
        {
            return Request.Id switch
            {
                null => null,
                int id => Response.OfSuccess(id, "")
            };
        }
    }
}
