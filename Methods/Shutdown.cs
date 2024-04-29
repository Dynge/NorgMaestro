using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class ShutdownHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required RpcMessageWriter Writer { get; init; }

        public Response? HandleRequest()
        {
            Writer.EncodeAndWrite(Notification.Default("Shutting down...", 2));
            return Response.OfSuccess(Request.Id!.Value, "");
        }
    }

    public class ExitHandler : IMessageHandler
    {
        public required RpcMessageWriter Writer { get; init; }

        public Response? HandleRequest()
        {
            Writer.EncodeAndWrite(Notification.Default("Goodbye!", 2));
            throw new InvalidDataException("Shutting down.");
        }
    }
}
