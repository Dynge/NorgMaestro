using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class NoneHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required RpcMessageWriter Writer { get; init; }

        public Response? HandleRequest()
        {
            Writer.EncodeAndWrite(Notification.Default($"Cannot handle '{Request.Method}'!!.", 1));
            return null;
        }
    }
}
