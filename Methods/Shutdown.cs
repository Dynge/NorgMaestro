using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class ShutdownHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }

        public Response? HandleRequest()
        {
            return Response.OfSuccess(Request.Id, "");
        }
    }

    public class ExitHandler : IMessageHandler
    {
        public Response? HandleRequest()
        {
            throw new InvalidDataException("Shutting down.");
        }
    }
}
