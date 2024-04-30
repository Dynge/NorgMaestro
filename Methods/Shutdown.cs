using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class ShutdownHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required IRpcWriter Writer { get; init; }

        public Response? HandleRequest()
        {
            Writer.EncodeAndWrite(Notification.Default("Shutting down..."));
            return Response.OfSuccess(Request.Id! ?? 0);
        }
    }

    public class ExitHandler : IMessageHandler
    {
        public required IRpcWriter Writer { get; init; }

        public Response? HandleRequest()
        {
            Writer.EncodeAndWrite(Notification.Default("Goodbye!"));
            throw new InvalidDataException("Shutting down.");
        }
    }
}
