using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class PrepareCallHierarchyHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request =request;
    private readonly LanguageServerState _state =state;

    public Response? HandleRequest()
    {
        PrepareCallHierarchyRequest initRequest = PrepareCallHierarchyRequest.From(_request);
        Document doc = _state.Documents[initRequest.Params.TextDocument.Uri];
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
