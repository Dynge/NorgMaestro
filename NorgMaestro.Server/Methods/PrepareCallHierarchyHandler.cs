using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class PrepareCallHierarchyHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage Request =request;
    private readonly LanguageServerState State =state;

    public Response? HandleRequest()
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
