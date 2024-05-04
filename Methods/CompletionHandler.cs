using NorgMaestro.Rpc;

namespace NorgMaestro.Methods
{
    public class CompletionHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required LanguageServerState State { get; init; }

        public Response HandleRequest()
        {
            CompletionRequest completionRequest = CompletionRequest.From(Request);
            HashSet<CompletionItem> res = new([]);

            foreach (Document doc in State.Documents.Values)
            {
                if (doc.Uri.Equals(completionRequest.Params.TextDocument.Uri))
                {
                    continue;
                }
                IEnumerable<CompletionItem> completionItems = doc.Metadata.Categories.Select(
                    c => new CompletionItem() { Label = c.Name }
                );
                res.UnionWith(completionItems);
            }
            return Response.OfSuccess(completionRequest.Id, res);
        }
    }
}
