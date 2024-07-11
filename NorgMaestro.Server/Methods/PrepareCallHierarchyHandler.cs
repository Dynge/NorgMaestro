using NorgMaestro.Rpc;

namespace NorgMaestro.Methods;

public class PrepareCallHierarchyHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }
    public required LanguageServerState State { get; init; }

    public Response HandleRequest()
    {
        PrepareCallHierarchyRequest initRequest = PrepareCallHierarchyRequest.From(Request);
        Document doc = State.Documents[initRequest.Params.TextDocument.Uri];
        CallHierarchyItem[] items =
        [
            new CallHierarchyItem()
            {
                Name = doc.Metadata.Title?.Name ?? "",
                Kind = SymbolKind.File,
                Uri = doc.Uri.AbsolutePath,
                Range = new()
                {
                    Start = new() { Line = 0, Character = 0 },
                    End = new() { Line = 0, Character = 0 },
                },
                SelectionRange = new()
                {
                    Start = new() { Line = 0, Character = 0 },
                    End = new() { Line = 0, Character = 0 },
                },
            }
        ];
        return Response.OfSuccess(initRequest.Id, items);
    }
}
