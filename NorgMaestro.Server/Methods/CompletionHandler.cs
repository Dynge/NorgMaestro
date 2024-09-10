using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CompletionHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }
    public required IRpcWriter Writer { get; init; }
    public required LanguageServerState State { get; init; }

    public Response? HandleRequest()
    {
        CompletionRequest completionRequest = CompletionRequest.From(Request);

        Writer.EncodeAndWrite(Notification.Default($"Created {completionRequest.Params.CompletionContext}!", MessageType.Debug));
        IEnumerable<CompletionItem> res = [];
        if (completionRequest.Params.CompletionContext?.TriggerCharacter == 'a')
        {
            res = GetLinkCompletions(completionRequest.Params.TextDocument.Uri);
        }
        else
        {
            HashSet<CompletionItem> uniqueCategories = [];
            foreach (Document doc in State.Documents.Values)
            {
                if (doc.Uri.Equals(completionRequest.Params.TextDocument.Uri))
                {
                    continue;
                }
                IEnumerable<CompletionItem> completionItems = doc.Metadata.Categories.Select(
                    c => new CompletionItem() { Label = c.Name }
                );
                uniqueCategories.UnionWith(completionItems);
            }
            res = uniqueCategories.ToList();
        }
        return Response.OfSuccess(completionRequest.Id, res);
    }

    public IEnumerable<CompletionItem> GetLinkCompletions(Uri currentDocument)
    {
        IEnumerable<CompletionItem> completionItems = State
            .Documents.Values.Where(d => !d.Uri.Equals(currentDocument))
            .Select(d => new CompletionItem()
            {
                Label =
                    $"{{:{Path.GetFileNameWithoutExtension(d.Uri.AbsolutePath)}:}}[{d.Metadata.Title}]",
            });
        return completionItems;
    }
}
