using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class InitializeHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }

        public Response HandleRequest()
        {
            InitializeRequest initRequest = InitializeRequest.From(Request);
            InitializeResultParams res =
                new()
                {
                    Capabilities = new()
                    {
                        CompletionProvider = new()
                        {
                            ResolveProvider = false,
                            TriggerCharacters = ['æ', 'ø', 'å']
                        }
                    }
                };

            return Response.OfSuccess(initRequest.Id, res);
        }
    }

    public class InitializedHandler : IMessageHandler
    {
        public required IRpcWriter Writer { get; init; }

        public Response? HandleRequest()
        {
            Writer.EncodeAndWrite(Notification.Default("Initialized!", 4));
            return null;
        }
    }
}
