using CeorgLsp.Rpc;

namespace CeorgLsp.Methods
{
    public class CompletionHandler : IMessageHandler
    {
        public required Request Request { get; init; }

        public Response HandleRequest()
        {
            CompletionResult res = new() { Result = [new() { Label = "Neovim" }], };

            return Response.OfSuccess(Request.Id, res);
        }
    }
}
