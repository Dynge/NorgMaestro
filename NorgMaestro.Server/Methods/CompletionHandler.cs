using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CompletionHandler(LanguageServerState state, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Response? HandleRequest()
    {
        CompletionRequest completionRequest = CompletionRequest.From(_request);

        var items = completionRequest.Params.CompletionContext?.TriggerCharacter switch
        {
            '{' => GetLinkCompletions(completionRequest.Params),
            _ => GetCategoryCompletions(completionRequest.Params),
        };

        return Response.OfSuccess(completionRequest.Id, items);
    }

    private HashSet<CompletionItem> GetCategoryCompletions(
        CompletionRequestParams completionRequestParams
    )
    {
        HashSet<CompletionItem> uniqueCategories = [];
        foreach (Document doc in _state.Documents.Values)
        {
            if (doc.Uri.Equals(completionRequestParams.TextDocument.Uri))
            {
                continue;
            }
            IEnumerable<CompletionItem> completionItems = doc.Metadata.Categories.Select(
                c => new CompletionItem() { Label = c.Name }
            );
            uniqueCategories.UnionWith(completionItems);
        }
        return uniqueCategories;
    }

    private IEnumerable<CompletionItem> GetLinkCompletions(
        CompletionRequestParams completionRequestParams
    )
    {
        IEnumerable<CompletionItem> completionItems = _state
            .Documents.Values.Where(d => !d.Uri.Equals(completionRequestParams.TextDocument.Uri))
            .Select(d => new CompletionItem()
            {
                Label = d.Metadata.Title?.Name ?? "[No Title]",
                Kind = CompletionKind.File,
                TextEdit = new()
                {
                    Range = new()
                    {
                        Start = new()
                        {
                            Character = completionRequestParams.Postion.Character - 1,
                            Line = completionRequestParams.Postion.Line,
                        },
                        End = new()
                        {
                            Line = completionRequestParams.Postion.Line,
                            Character = (uint)(
                                completionRequestParams.Postion.Character
                                + $"{{:{Path.GetFileNameWithoutExtension(d.Uri.AbsolutePath)}:}}[{d.Metadata.Title?.Name ?? ""}]".Length
                            ),
                        },
                    },
                    NewText =
                        $"{{:{Path.GetFileNameWithoutExtension(d.Uri.AbsolutePath)}:}}[{d.Metadata.Title?.Name ?? ""}]",
                },
            });
        return completionItems;
    }
}
