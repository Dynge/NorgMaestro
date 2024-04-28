using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class InitializeHandler : IMessageHandler
    {
        public required Request Request { get; init; }

        public Response HandleRequest()
        {
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

            return Response.OfSuccess(Request.Id, res);
        }
    }
}
