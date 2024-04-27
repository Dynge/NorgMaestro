using CeorgLsp.Rpc;

namespace CeorgLsp.Method
{
    public class InitializationHandler : IMessageHandler
    {
        public required Request Request { get; init; }

        public Response HandleRequest()
        {
            InitializeResultParams res =
                new()
                {
                    Result = new()
                    {
                        Capabilities = new()
                        {
                            CompletionProvider = new() { TriggerCharacters = ['æ', 'ø', 'å'] }
                        }
                    }
                };

            return Response.From(1, res, null);
        }
    }
}
