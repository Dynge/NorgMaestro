using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CompletionHandler(LanguageServerState state, IRpcWriter writer, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage Request = request;
    private readonly IRpcWriter Writer = writer;
    private readonly LanguageServerState State = state;

    public Response? HandleRequest()
    {
        CompletionRequest completionRequest = CompletionRequest.From(Request);

        Writer.EncodeAndWrite(
            Notification.Default(
                $"Created {completionRequest.Params.CompletionContext}!",
                MessageType.Debug
            )
        );
        IEnumerable<CompletionItem> res = [];
        if (completionRequest.Params.CompletionContext?.TriggerCharacter is '{')
        {
            res = GetLinkCompletions(completionRequest.Params);
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

    public IEnumerable<CompletionItem> GetLinkCompletions(
        CompletionRequestParams completionRequestParams
    )
    {
        IEnumerable<CompletionItem> completionItems = State
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
